using AISIGA.Program.AIS;
using AISIGA.Program.AIS.VALIS;
using AISIGA.Program.Data;
using AISIGA.Program.Experiments;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace AISIGA.Program.IGA
{
    class Island
    {
        private ExperimentConfig Config { get; set; }
        private List<AIS.Antigen> Antigens { get; set; }
        private List<AIS.Antibody> Antibodies { get; set; }
        private Island? Neighbour { get; set; }

        public Island(ExperimentConfig config)
        {
            Config = config;
            Antigens = new List<AIS.Antigen>();
            Antibodies = new List<AIS.Antibody>();
            Neighbour = null;
        }

        public List<Antibody> GetAntibodies()
        {
            return Antibodies;
        }

        public void SetNeighbour(Island neighbour)
        {
            Neighbour = neighbour;
        }

        public void AddAntigen(Antigen antigen)
        {
            this.Antigens.Add(antigen);
        }

        public void AddAntibody(Antibody antibody)
        {
            this.Antibodies.Add(antibody);
        }

        public void InitializeAntibodies(List<double> classDistributionFractions)
        {
            int featureCount = Antigens[0].GetLength();

            // Find the min and max values of the AntiGens
            double[] maxValues = new double[featureCount];
            double[] minValues = new double[featureCount];

            // Initialize with the first antibody
            for (int i = 0; i < featureCount; i++)
            {
                maxValues[i] = Antigens[0].GetFeatureValueAt(i);
                minValues[i] = Antigens[0].GetFeatureValueAt(i);
            }

            // Iterate through the rest
            foreach (var antigen in Antigens.Skip(1))
            {
                for (int i = 0; i < featureCount; i++)
                {
                    if (antigen.GetFeatureValueAt(i) > maxValues[i])
                        maxValues[i] = antigen.GetFeatureValueAt(i);

                    if (antigen.GetFeatureValueAt(i) < minValues[i])
                        minValues[i] = antigen.GetFeatureValueAt(i);
                }
            }

            // Add the slight offset according to radius
            for (int i = 0; i < featureCount; i++)
            {
                maxValues[i] += Config.BaseRadius;
                minValues[i] += Config.BaseRadius;
            }

            // Set radius based on the min and max values
            // And set class based on class distribution
            foreach (Antibody antibody in Antibodies)
            {
                // Select a class based on the distribution
                double randomValue = RandomProvider.GetThreadRandom().NextDouble();
                double cumulativeProbability = 0.0;
                int selectedClass = -1;
                for (int i = 0; i < classDistributionFractions.Count; i++)
                {
                    cumulativeProbability += classDistributionFractions[i];
                    if (randomValue <= cumulativeProbability)
                    {
                        selectedClass = i;
                        break;
                    }
                }

                antibody.AssingRandomClassAndRadius(Config.BaseRadius, selectedClass);
                antibody.AssignRandomFeatureValuesAndMultipliers(maxValues, minValues, Config.UseHyperSpheres, Config.UseUnboundedRegions, Config.RateOfUnboundedRegions);

            }
        }

        private void SortByFitness()
        {
            VALIS.AssingAGClassByVoting(Antibodies, Antigens);
            // Sort the antibodies by fitness
            Antibodies = Antibodies
            .OrderByDescending(a => CalculateAntibodyFitness(a, Antibodies, Antigens))
            .ToList();
        }

        public void RecieveMigration(List<Antibody> migrants)
        {
            // Add migrants
            this.Antibodies.AddRange(migrants);
            // Sort the antibodies by fitness
            Dictionary<int, int> originalClassDistribution = Antibodies
                .GroupBy(ab => ab.GetClass())
                .ToDictionary(g => g.Key, g => g.Count());
            // Remove the excess antibodies
            ReplaceByClass(originalClassDistribution);
        }

        public void Migrate()
        {
            if (this.Neighbour != null)
            {
                int AmountToMigrate = (int)((Config.MigrationRate * (this.Antigens.Count * Config.PopulationSizeFractionOfDatapoints)) / Config.NumberOfIslands);
                List<Antibody> ToMigrate = new List<Antibody>();
                ToMigrate.AddRange(Antibodies.Take(AmountToMigrate));
                List<Antibody> CopiedMigrants = new List<Antibody>();
                foreach (var antibody in ToMigrate)
                {
                    Antibody copy = new Antibody(antibody.GetClass(), antibody.GetBaseRadius(), antibody.GetFeatureValues(), antibody.GetFeatureMultipliers(), antibody.GetFeatureDimTypes(), antibody.GetFitness(), true);
                    CopiedMigrants.Add(copy);
                }
                this.Neighbour.RecieveMigration(CopiedMigrants);
            }
            else
            {
                Console.WriteLine("No neighbour to migrate to.");
            }
        }

        public void RunGeneration()
        {
            SortByFitness();
            // Save clas distribution for later 
            Dictionary<int, int> originalClassDistribution = Antibodies
                .GroupBy(ab => ab.GetClass())
                .ToDictionary(g => g.Key, g => g.Count());

            // Select the best antibodies
            int NumberOfParents = (int)(Config.PercentageOfParents * ((this.Antigens.Count * Config.PopulationSizeFractionOfDatapoints) / Config.NumberOfIslands));
            for (int i = 0; i < NumberOfParents; i += 2)
            {
                // Perform crossover
                Antibody parent1 = EVOFunctions.TournamentSelect(Antibodies, Config.TournamentSize);
                Antibody parent2 = EVOFunctions.TournamentSelect(Antibodies, Config.TournamentSize);
                (Antibody child1, Antibody child2) = EVOFunctions.CrossoverAntibodies(parent1, parent2);

                // Mutate the children
                if (RandomProvider.GetThreadRandom().NextDouble() < Config.MutationRate)
                {
                    child1 = EVOFunctions.MutateAntibody(child1);
                }
                if (RandomProvider.GetThreadRandom().NextDouble() < Config.MutationRate)
                {
                    child2 = EVOFunctions.MutateAntibody(child2);
                }

                // Add the children back to the population
                Antibodies.Add(child1);
                Antibodies.Add(child2);
            }
            // Sort the antibodies by fitness
            SortByFitness();
            // Remove the excess antibodies
            ReplaceByClass(originalClassDistribution);
        }


        public void ReplaceByClass(Dictionary<int, int> originalClassDistribution)
        {
            // Get the class groups (including children)
            Dictionary<int, List<Antibody>> classGroups = Antibodies
                .GroupBy(ab => ab.GetClass())
                .ToDictionary(g => g.Key, g => g.OrderByDescending(ab => ab.GetFitness().GetTotalFitness()).ToList());

            // Create final balanced population
            List<Antibody> balancedPopulation = new List<Antibody>();

            // Total target size
            int totalTargetSize = (int)(this.Antigens.Count * Config.PopulationSizeFractionOfDatapoints);

            // Sum of original class distribution
            double totalOriginal = originalClassDistribution.Values.Sum();

            foreach (var kvp in classGroups)
            {
                int classId = kvp.Key;
                List<Antibody> sorted = kvp.Value;

                // Fraction of this class in original population
                double fraction = originalClassDistribution.ContainsKey(classId)
                    ? (double)originalClassDistribution[classId] / totalOriginal
                    : 0;

                // Target population size for this class
                int targetSize = (int)Math.Round(fraction * totalTargetSize);

                // Add top antibodies for this class
                balancedPopulation.AddRange(sorted.Take(targetSize));
            }

            // Final size safety net (in case of rounding error)
            balancedPopulation = balancedPopulation
                .OrderByDescending(ab => ab.GetFitness().GetTotalFitness())
                .Take(totalTargetSize)
                .ToList();

            Antibodies = balancedPopulation;
        }



        private double CalculateAntibodyFitness(Antibody antibody, List<Antibody> allAntibodies, List<Antigen> allAntigens)
        {
            if (antibody.GetFitness().GetIsCalculated())
            {
                return antibody.GetFitness().GetTotalFitness();
            }
            else 
            {
                (List<Antigen> matchedAntigens, double[] matchScores) = FitnessFunctions.GetMatchedAntigens(antibody, allAntigens);
                double TruePositives = FitnessFunctions.CalcTruePositives(antibody, matchedAntigens);
                double FalsePositives = FitnessFunctions.CalcFalsePositives(antibody, matchedAntigens); ;
                double AllPositivesOfSameClass = FitnessFunctions.CalcAllPositivesOfClass(antibody, allAntigens);
                

                antibody.GetFitness().SetCorrectness(FitnessFunctions.CalculateCorrectness(TruePositives, FalsePositives) * Config.aScoreMultiplier);
                antibody.GetFitness().SetCoverage(FitnessFunctions.CalculateCoverage(TruePositives, AllPositivesOfSameClass) * Config.bScoreMultiplier);
                antibody.GetFitness().SetUniqueness(FitnessFunctions.CalculateUniqueness(TruePositives, allAntibodies, matchedAntigens) * Config.cScoreMultiplier);
                antibody.GetFitness().SetValidAvidity(FitnessFunctions.CalculateAvidity(matchedAntigens, matchScores) * Config.dScoreMultiplier);
                antibody.GetFitness().SetInvalidAvidity(FitnessFunctions.CalculateInvalidAvidity(matchedAntigens, matchScores) * Config.eScoreMultiplier);
                antibody.GetFitness().SetTotalFitness(antibody.GetFitness().GetCorrectness() 
                    + antibody.GetFitness().GetCoverage()
                    + antibody.GetFitness().GetUniqueness()
                    + antibody.GetFitness().GetValidAvidity()
                    - antibody.GetFitness().GetInvalidAvidity());
                antibody.GetFitness().SetIsCalculated(true);
                return antibody.GetFitness().GetTotalFitness();

            }
        }

    }
}

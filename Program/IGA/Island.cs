using AISIGA.Program.AIS;
using AISIGA.Program.AIS.VALIS;
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

        public void InitializeAntibodies()
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

            // Create antibodies based on the min and max values
            foreach (Antibody antibody in Antibodies)
            {
                antibody.AssingRandomClassAndRadius(Config.BaseRadius);
                antibody.AssignRandomFeatureValuesAndMultipliers(maxValues, minValues, Config.UseHyperSpheres, Config.UseUnboundedRegions);

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
            SortByFitness();
            // Remove the excess antibodies
            ReplaceByClass();
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
            ReplaceByClass();
        }


        public void ReplaceByClass()
        {
            int classCount = LabelEncoder.ClassCount;
            int populationPerClass = (int)((this.Antigens.Count * Config.PopulationSizeFractionOfDatapoints) / Config.NumberOfIslands) / classCount;

            Dictionary<int, List<Antibody>> classGroups = new();

            // Initialize class groups
            for (int i = 0; i < classCount; i++)
                classGroups[i] = new List<Antibody>();

            // Group antibodies by class
            foreach (var ab in Antibodies)
            {
                classGroups[ab.GetClass()].Add(ab);
            }

            // Trim each group to desired size
            List<Antibody> balancedPopulation = new();
            foreach (var kvp in classGroups)
            {
                var sorted = kvp.Value.OrderByDescending(ab => ab.GetFitness().GetTotalFitness()).ToList();
                balancedPopulation.AddRange(sorted.Take(populationPerClass));
            }

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

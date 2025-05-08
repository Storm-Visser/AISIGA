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

            
            // Set AB class based on AG class distribution
            int totalCount = Antibodies.Count;

            // Calculate target distribution count for each class
            List<int> targetCounts = classDistributionFractions
                .Select(fraction => (int)Math.Round(fraction * totalCount))
                .ToList();

            // Force each class to have at least 10 antibodies
            int extraAdded = 0;
            foreach (int i in targetCounts)
            {
                if (i < 10)
                {
                    targetCounts[i] = 10; // Ensure a minimum of 10 antibodies per class
                    extraAdded += 10;
                }
            }

                //Take extra added from the class with the most antibodies
                if (extraAdded > 0)
            {
                int maxIndex = targetCounts
                    .Select((val, idx) => new { val, idx })
                    .Where(x => targetCounts[x.idx] > 1)
                    .OrderByDescending(x => x.val)
                    .First().idx;

                targetCounts[maxIndex] -= extraAdded;
            }

            // Start assigning the classes
            List<int> currentCounts = new List<int>(new int[targetCounts.Count]);

            var rnd = RandomProvider.GetThreadRandom();
            foreach (Antibody antibody in Antibodies)
            {
                List<int> availableClasses = new List<int>();

                // Only pick from classes that haven't reached their target
                for (int i = 0; i < targetCounts.Count; i++)
                {
                    if (currentCounts[i] < targetCounts[i])
                        availableClasses.Add(i);
                }

                // Select one of the remaining classes randomly
                int selectedClass = availableClasses[rnd.Next(availableClasses.Count)];

                if (!Config.UseClassRatioLocking)
                {
                    selectedClass = rnd.Next(0, LabelEncoder.ClassCount);
                }

                // Assign and update count
                currentCounts[selectedClass]++;

                antibody.AssingRandomClassAndRadius(Config.BaseRadius, selectedClass);
                // Also assign the feature values and multipliers
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
            Dictionary<int, int> originalClassDistribution = Antibodies
                .GroupBy(ab => ab.GetClass())
                .ToDictionary(g => g.Key, g => g.Count());
            // Add migrants
            this.Antibodies.AddRange(migrants);
            // Sort the antibodies by fitness and class
            // Remove the excess antibodies
            if (Config.UseClassRatioLocking)
            {
                ReplaceByClass(originalClassDistribution);
            }
            else
            {
                ReplaceByFitness();
            }
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
            // Save clas distribution for later 
            Dictionary<int, int> originalClassDistribution = Antibodies
                .GroupBy(ab => ab.GetClass())
                .ToDictionary(g => g.Key, g => g.Count());

            // Select the best antibodies
            int NumberOfParents = (int)(Config.PercentageOfParents * ((this.Antigens.Count * Config.PopulationSizeFractionOfDatapoints) / Config.NumberOfIslands));
            for (int i = 0; i < NumberOfParents; i += 2)
            {
                Antibody parent1;
                Antibody parent2;
                // Perform crossover
                if (RandomProvider.GetThreadRandom().NextDouble() < Config.CrossoverRate)
                {
                    if (Config.UseTournamentSelection)
                    {
                        // Select parents using tournament selection
                        parent1 = EVOFunctions.TournamentSelect(Antibodies, Config.TournamentSize);
                        parent2 = EVOFunctions.TournamentSelect(Antibodies, Config.TournamentSize);
                    }
                    else
                    {
                        // Select parents using elitism selection
                        parent1 = EVOFunctions.ElitismSelection(Antibodies);
                        parent2 = EVOFunctions.ElitismSelection(Antibodies);
                    }
                    (Antibody child1, Antibody child2) = EVOFunctions.CrossoverAntibodies(parent1, parent2);

                    // Mutate the children
                    if (RandomProvider.GetThreadRandom().NextDouble() < Config.MutationRate)
                    {
                        child1 = EVOFunctions.MutateAntibody(child1, Antigens);
                    }
                    if (RandomProvider.GetThreadRandom().NextDouble() < Config.MutationRate)
                    {
                        child2 = EVOFunctions.MutateAntibody(child2, Antigens);
                    }

                    // Add the children back to the population
                    Antibodies.Add(child1);
                    Antibodies.Add(child2);
                }
            }
            // Sort the antibodies by fitness
            SortByFitness();
            // Remove the excess antibodies
            if (Config.UseClassRatioLocking)
            {
                ReplaceByClass(originalClassDistribution);
            }
            else
            {
                ReplaceByFitness();
            }
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

        public void ReplaceByFitness()
        {
            // Total target size
            int totalTargetSize = (int)(this.Antigens.Count * Config.PopulationSizeFractionOfDatapoints );

            Antibodies = Antibodies
                .OrderByDescending(ab => ab.GetFitness().GetTotalFitness())
                .Take(totalTargetSize)
                .ToList();
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

using AISIGA.Program.AIS;
using AISIGA.Program.AIS.VALIS;
using AISIGA.Program.Data;
using AISIGA.Program.Experiments;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private AbstractExperimentConfig Config { get; set; }
        private List<AIS.Antigen> Antigens { get; set; }
        private List<AIS.Antibody> Antibodies { get; set; }
        private Island? Neighbour { get; set; }

        public int Generation { get; set; }

        public Island(AbstractExperimentConfig config)
        {
            Config = config;
            Antigens = new List<AIS.Antigen>();
            Antibodies = new List<AIS.Antibody>();
            Neighbour = null;
            Generation = 0;
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

        public void AddAllAntigens(List<Antigen> antigens)
        {
            this.Antigens = antigens;
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
                if (Config.UseUnboundedRatioLocking)
                {
                    if (Config.UseSoftClassRatiosWUnboundedLocking)
                    {
                        ReplaceByUnboundedRatioWithPartialElitismAndSoftBalanceClasses(Config.ElitismPercentage);
                    }
                    else
                    {
                        ReplaceByUnboundedRatioOnly(Config.ElitismPercentage);
                    }
                }
                else
                {

                    ReplaceByClassWithPartialElitism(originalClassDistribution, Config.ElitismPercentage);
                }
            }
            else
            {
                ReplaceByPartialFitness(Config.ElitismPercentage);
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
            Generation++;
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
                if (Config.UseUnboundedRatioLocking)
                {
                    if (Config.UseSoftClassRatiosWUnboundedLocking)
                    {
                        ReplaceByUnboundedRatioWithPartialElitismAndSoftBalanceClasses(Config.ElitismPercentage);
                    }
                    else
                    {
                        ReplaceByUnboundedRatioOnly(Config.ElitismPercentage);
                    }
                }
                else
                {

                    ReplaceByClassWithPartialElitism(originalClassDistribution, Config.ElitismPercentage);
                }
            }
            else
            {
                ReplaceByPartialFitness(Config.ElitismPercentage);
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

        public void ReplaceByClassWithPartialElitism(Dictionary<int, int> originalClassDistribution, double eliteFraction)
        {
            // Get the class groups
            Dictionary<int, List<Antibody>> classGroups = Antibodies
                .GroupBy(ab => ab.GetClass())
                .ToDictionary(g => g.Key, g => g.OrderByDescending(ab => ab.GetFitness().GetTotalFitness()).ToList());

            List<Antibody> balancedPopulation = new List<Antibody>();
            int totalTargetSize = (int)(this.Antigens.Count * Config.PopulationSizeFractionOfDatapoints);
            double totalOriginal = originalClassDistribution.Values.Sum();
            Random rng = new Random();

            foreach (var kvp in classGroups)
            {
                int classId = kvp.Key;
                List<Antibody> sorted = kvp.Value;

                double fraction = originalClassDistribution.ContainsKey(classId)
                    ? (double)originalClassDistribution[classId] / totalOriginal
                    : 0;

                int targetSize = (int)Math.Round(fraction * totalTargetSize);

                int eliteCount = (int)Math.Round(targetSize * eliteFraction);
                int randomCount = targetSize - eliteCount;

                // Add elite individuals
                var elites = sorted.Take(eliteCount);
                balancedPopulation.AddRange(elites);

                // Add random individuals from the remainder (if enough exist)
                var remainder = sorted.Skip(eliteCount).ToList();
                if (remainder.Count >= randomCount)
                {
                    var randoms = remainder.OrderBy(_ => rng.Next()).Take(randomCount);
                    balancedPopulation.AddRange(randoms);
                }
                else
                {
                    // If not enough, add all and compensate later
                    balancedPopulation.AddRange(remainder);
                }
            }

            // Final size safety net (in case of rounding or class imbalance)
            balancedPopulation = balancedPopulation
                .OrderByDescending(ab => ab.GetFitness().GetTotalFitness())
                .Take(totalTargetSize)
                .ToList();

            Antibodies = balancedPopulation;
        }


        public void ReplaceByPartialFitness(double eliteFraction)
        {
            // Total target size
            int totalTargetSize = (int)(this.Antigens.Count * Config.PopulationSizeFractionOfDatapoints);

            // Fraction of population to retain as elites
            double elitismFraction = eliteFraction;
            int eliteCount = (int)Math.Round(elitismFraction * totalTargetSize);
            int randomCount = totalTargetSize - eliteCount;

            // Sort by fitness
            var sorted = Antibodies.OrderByDescending(ab => ab.GetFitness().GetTotalFitness()).ToList();

            // Take top N elites
            List<Antibody> selected = new List<Antibody>(sorted.Take(eliteCount));

            // Randomly sample the remaining from the rest (excluding the elites)
            Random rng = new Random();
            var nonElites = sorted.Skip(eliteCount).OrderBy(_ => rng.Next()).Take(randomCount);

            selected.AddRange(nonElites);

            // Final replacement
            Antibodies = selected;
        }

        public void ReplaceByFitness()
        {
            // Total target size
            int totalTargetSize = (int)(this.Antigens.Count * Config.PopulationSizeFractionOfDatapoints);

            Antibodies = Antibodies
                .OrderByDescending(ab => ab.GetFitness().GetTotalFitness())
                .Take(totalTargetSize)
                .ToList();
        }

        public void ReplaceByUnboundedRatioWithPartialElitismAndSoftBalanceClasses(double eliteFraction)
        {
            int totalTargetSize = (int)(this.Antigens.Count * Config.PopulationSizeFractionOfDatapoints);
            int desiredUBCount = (int)(totalTargetSize * Config.RateOfUnboundedRegions);

            // Group antibodies by class for soft diversity
            var classGroups = Antibodies
                .GroupBy(ab => ab.GetClass())
                .ToDictionary(g => g.Key, g => g.OrderByDescending(ab => ab.GetFitness().GetTotalFitness()).ToList());

            // Soft-balanced initial selection
            List<Antibody> initialPool = new List<Antibody>();
            int remainingSlots = totalTargetSize;
            int numClasses = classGroups.Count;

            foreach (var kvp in classGroups)
            {
                int share = totalTargetSize / numClasses;
                var candidates = kvp.Value.Take(share);
                initialPool.AddRange(candidates);
                remainingSlots -= candidates.Count();
            }

            // Fill remaining slots with best overall
            if (remainingSlots > 0)
            {
                var extra = Antibodies
                    .Except(initialPool)
                    .OrderByDescending(ab => ab.GetFitness().GetTotalFitness())
                    .Take(remainingSlots);
                initialPool.AddRange(extra);
            }

            // Enforce UB ratio
            int currentUBCount = initialPool.Count(ab => ab.GetFeatureDimTypes().Any(t => t == 1 || t == 2));

            if (currentUBCount < desiredUBCount)
            {
                // Add UB, remove bounded
                int needed = desiredUBCount - currentUBCount;

                var toAdd = Antibodies
                    .Where(ab => ab.GetFeatureDimTypes().Any(t => t == 1 || t == 2) && !initialPool.Contains(ab))
                    .OrderByDescending(ab => ab.GetFitness().GetTotalFitness())
                    .Take(needed)
                    .ToList();

                var toRemove = initialPool
                    .Where(ab => ab.GetFeatureDimTypes().All(t => t == 0))
                    .OrderBy(ab => ab.GetFitness().GetTotalFitness())
                    .Take(toAdd.Count)
                    .ToList();

                for (int i = 0; i < Math.Min(toAdd.Count, toRemove.Count); i++)
                {
                    initialPool.Remove(toRemove[i]);
                    initialPool.Add(toAdd[i]);
                }
            }
            else if (currentUBCount > desiredUBCount)
            {
                // Remove excess UB, add bounded
                int excess = currentUBCount - desiredUBCount;

                var toRemove = initialPool
                    .Where(ab => ab.GetFeatureDimTypes().Any(t => t == 1 || t == 2))
                    .OrderBy(ab => ab.GetFitness().GetTotalFitness())
                    .Take(excess)
                    .ToList();

                var toAdd = Antibodies
                    .Where(ab => ab.GetFeatureDimTypes().All(t => t == 0) && !initialPool.Contains(ab))
                    .OrderByDescending(ab => ab.GetFitness().GetTotalFitness())
                    .Take(toRemove.Count)
                    .ToList();

                for (int i = 0; i < Math.Min(toAdd.Count, toRemove.Count); i++)
                {
                    initialPool.Remove(toRemove[i]);
                    initialPool.Add(toAdd[i]);
                }
            }

            // Final trim to maintain target size
            Antibodies = initialPool
                .OrderByDescending(ab => ab.GetFitness().GetTotalFitness())
                .Take(totalTargetSize)
                .ToList();
        }

        public void ReplaceByUnboundedRatioOnly(double eliteFraction)
        {
            int totalTargetSize = (int)(this.Antigens.Count * Config.PopulationSizeFractionOfDatapoints);
            int desiredUBCount = (int)(totalTargetSize * Config.RateOfUnboundedRegions);

            // Select elites
            var sortedByFitness = Antibodies.OrderByDescending(ab => ab.GetFitness().GetTotalFitness()).ToList();
            int eliteCount = (int)(totalTargetSize * eliteFraction);
            List<Antibody> initialPool = sortedByFitness.Take(eliteCount).ToList();

            // Fill the rest with randoms
            var remainder = sortedByFitness.Skip(eliteCount).ToList();
            var rng = RandomProvider.GetThreadRandom();
            int randomCount = totalTargetSize - eliteCount;
            var randoms = remainder.OrderBy(_ => rng.Next()).Take(randomCount);
            initialPool.AddRange(randoms);

            // Count current UB
            int currentUBCount = initialPool.Count(ab => ab.GetFeatureDimTypes().Any(t => t == 1 || t == 2));

            if (currentUBCount < desiredUBCount)
            {
                // Add UB, remove bounded
                int needed = desiredUBCount - currentUBCount;

                var toAdd = Antibodies
                    .Where(ab => ab.GetFeatureDimTypes().Any(t => t == 1 || t == 2) && !initialPool.Contains(ab))
                    .OrderByDescending(ab => ab.GetFitness().GetTotalFitness())
                    .Take(needed)
                    .ToList();

                var toRemove = initialPool
                    .Where(ab => ab.GetFeatureDimTypes().All(t => t == 0))
                    .OrderBy(ab => ab.GetFitness().GetTotalFitness())
                    .Take(toAdd.Count)
                    .ToList();

                for (int i = 0; i < Math.Min(toAdd.Count, toRemove.Count); i++)
                {
                    initialPool.Remove(toRemove[i]);
                    initialPool.Add(toAdd[i]);
                }
            }
            else if (currentUBCount > desiredUBCount)
            {
                // Remove excess UB, add bounded
                int excess = currentUBCount - desiredUBCount;

                var toRemove = initialPool
                    .Where(ab => ab.GetFeatureDimTypes().Any(t => t == 1 || t == 2))
                    .OrderBy(ab => ab.GetFitness().GetTotalFitness())
                    .Take(excess)
                    .ToList();

                var toAdd = Antibodies
                    .Where(ab => ab.GetFeatureDimTypes().All(t => t == 0) && !initialPool.Contains(ab))
                    .OrderByDescending(ab => ab.GetFitness().GetTotalFitness())
                    .Take(toRemove.Count)
                    .ToList();

                for (int i = 0; i < Math.Min(toAdd.Count, toRemove.Count); i++)
                {
                    initialPool.Remove(toRemove[i]);
                    initialPool.Add(toAdd[i]);
                }
            }

            // Final trim to maintain target size
            Antibodies = initialPool
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

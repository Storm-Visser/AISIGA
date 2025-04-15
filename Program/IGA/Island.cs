using AISIGA.Program.AIS;
using AISIGA.Program.Experiments;
using System;
using System.Collections.Generic;
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

        public void SetNeighbour(Island neighbour)
        {
            Neighbour = neighbour;
        }

        public void AddAntigen(Antigen Antigen)
        {
            this.Antigens.Add(Antigen);
        }

        public void AddAntibody(Antibody Antibodie)
        {
            this.Antibodies.Add(Antibodie);
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
                antibody.AssignRandomFeatureValuesAndMultipliers(maxValues, minValues);
                antibody.AssingRandomClass();
            }
        }

        private void SortByFitness()
        {
            // Sort the antibodies by fitness
            Antibodies = Antibodies
            .OrderByDescending(a => a.GetFitness().GetTotalFitness())
            .ToList();
        }

        public void RecieveMigration(List<Antibody> migrants)
        {
            // Add migrants
            this.Antibodies.AddRange(migrants);
            // Sort the antibodies by fitness
            SortByFitness();
            // Remove the excess antibodies
            this.Antibodies = this.Antibodies.Take(Config.PopulationSize).ToList();

        }

        public void Migrate()
        {
            if (this.Neighbour != null)
            {
                int AmountToMigrate = (int)(Config.MigrationRate * Config.PopulationSize);
                List<Antibody> ToMigrate = new List<Antibody>();
                ToMigrate.AddRange(Antibodies.Take(AmountToMigrate));
                List<Antibody> CopiedMigrants = new List<Antibody>();
                foreach (var antibody in ToMigrate)
                {
                    Antibody copy = new Antibody(antibody.GetClass(), antibody.GetBaseRadius(), antibody.GetFeatureValues(), antibody.GetFeatureMultipliers(), antibody.GetFitness(), true);
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
            int AmountToCrossover = (int)(Config.MigrationRate * Config.PopulationSize);
            List<Antibody> selectedAntibodies = Antibodies.Take(Config.PopulationSize).ToList();
            for (int i = 0; i < AmountToCrossover; i += 2)
            {
                // Perform crossover
                Antibody parent1 = selectedAntibodies[i];
                Antibody parent2 = selectedAntibodies[i + 1];
                (Antibody child1, Antibody child2) = EVOFunctions.CrossoverAntibodies(parent1, parent2);

                // Mutate the children
                if (RandomProvider.GetThreadRandom().NextDouble() < Config.MutationRate)
                {
                    child1 = EVOFunctions.MutateAntibody(child1);
                }
                if (RandomProvider.GetThreadRandom().NextDouble() < Config.MutationRate)
                {
                    child1 = EVOFunctions.MutateAntibody(child1);
                }

                // Add the children back to the population
                Antibodies.Add(child1);
                Antibodies.Add(child2);
            }
            // Sort the antibodies by fitness
            SortByFitness();
            // Remove the excess antibodies
            this.Antibodies = this.Antibodies.Take(Config.PopulationSize).ToList();
        }
    }
}

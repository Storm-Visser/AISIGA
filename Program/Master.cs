using AISIGA.Program.AIS.VALIS;
using AISIGA.Program.Experiments;
using AISIGA.Program.IGA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AISIGA.Program
{
    class Master
    {
        private ExperimentConfig Config { get; set; }
        private List<Island> Islands { get; set; }

        private List<AIS.Antigen> AntigensTrain { get; set; }

        private List<AIS.Antigen> AntigensTest { get; set; }

        private List<AIS.Antibody> Antibodies { get; set; }

        // UI Variables

        public Master(ExperimentConfig config)
        {
            Config = config;
            EVOFunctions.Config = this.Config;
            FitnessFunctions.Config = this.Config;
            Islands = new List<Island>();
            AntigensTrain = new List<AIS.Antigen>();
            AntigensTest = new List<AIS.Antigen>();
            Antibodies = new List<AIS.Antibody>();
        }

        public void Initialize()
        {
            InitIslands();
            InitAntigensAndAntibodies();
            DivideAntigenAndAntibodies();
            RandomizeAntibodies();
            StartExperiment();

        }

        private void InitIslands()
        {
            for (int i = 0; i < Config.NumberOfIslands; i++)
            {
                Island island = new Island(this.Config);
                Islands.Add(island);

                //connect the neighbours
                if (i > 0)
                {
                    island.SetNeighbour(Islands[i - 1]);
                }

                if (i == Config.NumberOfIslands - 1)
                {
                    island.SetNeighbour(Islands[0]);
                }
            }
            Islands[0].SetNeighbour(Islands[Islands.Count - 1]);
        }

        private void InitAntigensAndAntibodies()
        {
            //Create the Antigens and Antibodies
            (this.AntigensTrain, this.AntigensTest) = Data.DataHandler.TranslateDataToAntigens(Config.DataSetNr, Config.TrainingTestSplit);
            for (int i = 0; i < Config.PopulationSize; i++)
            {
                this.Antibodies.Add(new AIS.Antibody(-1, Config.BaseRadius, this.AntigensTrain[0].GetLength()));
            }
        }

        private void DivideAntigenAndAntibodies()
        {
            //Divide the Antigens into the islands Round robin style
            for (int i = 0; i < this.AntigensTrain.Count; i++)
            {
                int islandIndex = i % 4;
                this.Islands[islandIndex].AddAntigen(this.AntigensTrain[i]);
                //add the antibodies aswell while we are at it
                if (i < this.Antibodies.Count)
                {
                    this.Islands[islandIndex].AddAntibody(this.Antibodies[i]);
                }
            }

            
        }

        private void RandomizeAntibodies()
        {
            foreach (var island in Islands)
            {
                island.InitializeAntibodies();
            }
        }

        public void StartExperiment()
        {
            int islandCount = Islands.Count;

            Barrier barrier = new Barrier(islandCount, (b) =>
            {
                System.Diagnostics.Trace.WriteLine("All islands have reached the barrier. Starting migration...");
                // This code runs ONCE after all threads hit the barrier
                foreach (var island in Islands)
                {
                    island.Migrate(); // Shared migration logic
                }
            });

            List<Task> islandTasks = new List<Task>();
            int migrationInterval = (int)(Config.MigrationFrequency * Config.NumberOfGenerations);
            foreach (var island in Islands)
            {
                var task = Task.Run(() =>
                {
                    for (int gen = 0; gen < Config.NumberOfGenerations; gen++)
                    {
                        island.RunGeneration(); // Run 1 generation

                        
                        if ((gen + 1) % migrationInterval == 0)
                        {
                            barrier.SignalAndWait(); // Wait for others
                        }
                    }
                });

                islandTasks.Add(task);
            }

            Task.WaitAll(islandTasks.ToArray());

            //Done Threading
            CollectResults();

        }

        private void CollectResults()
        {
            VALIS.AssingAGClassByVoting(this.Antibodies, this.AntigensTrain);
            VALIS.AssingAGClassByVoting(this.Antibodies, this.AntigensTest);

            // Calculate the fitness of the antibodies
            double trainFitness = FitnessFunctions.CalculateTotalFitness(this.AntigensTrain);
            double testFitness = FitnessFunctions.CalculateTotalFitness(this.AntigensTest);

            System.Diagnostics.Trace.WriteLine(testFitness);
            System.Diagnostics.Trace.WriteLine(trainFitness);

        }
    }
}

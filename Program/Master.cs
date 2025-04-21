using AISIGA.Program.AIS;
using AISIGA.Program.AIS.VALIS;
using AISIGA.Program.Experiments;
using AISIGA.Program.IGA;
using AISIGA.UI;
using LiveChartsCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml.Serialization;

namespace AISIGA.Program
{
    class Master
    {
        private ExperimentConfig Config { get; set; }
        private List<Island> Islands { get; set; }

        private List<AIS.Antigen> Antigens { get; set; }

        private DashboardWindow DashboardWindow { get; set; } // UI

        // UI Variables

        public Master(ExperimentConfig config, DashboardWindow dashboardWindow)
        {
            Config = config;
            EVOFunctions.Config = this.Config;
            FitnessFunctions.Config = this.Config;
            Islands = new List<Island>();
            Antigens = new List<AIS.Antigen>();
            DashboardWindow = dashboardWindow;
        }

        public void Initialize()
        {
            InitIslands();
            InitAntigensAndAntibodies();
            DivideAntigenAndAntibodies();
            RandomizeAntibodies();
            CollectResults(false);
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
            this.Antigens = Data.DataHandler.TranslateDataToAntigens(Config.DataSetNr, Config.TrainingTestSplit);
            this.Antigens = this.Antigens.OrderBy(x => RandomProvider.GetThreadRandom().Next()).ToList();
        }

        private void DivideAntigenAndAntibodies()
        {
            List<Antibody> antibodies = new List<Antibody>();
            for (int i = 0; i < (this.Antigens.Count * Config.PopulationSizeFractionOfDatapoints); i++)
            {
                antibodies.Add(new AIS.Antibody(-1, Config.BaseRadius, this.Antigens[0].GetLength()));
            }
            //Divide the Antigens into the islands Round robin style
            for (int i = 0; i < this.Antigens.Count; i++)
            {
                int islandIndex = i % 4;
                this.Islands[islandIndex].AddAntigen(this.Antigens[i]);
                //add the antibodies aswell while we are at it
                if (i < antibodies.Count)
                {
                    this.Islands[islandIndex].AddAntibody(antibodies[i]);
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

                        if(gen % (migrationInterval / 3) == 0)
                        {
                            // Update the UI with the results
                            if (island == Islands[0])
                            {
                                UpdateIslandUI(0);
                            }
                            else if (island == Islands[1])
                            {
                                UpdateIslandUI(1);
                            }
                            else if (island == Islands[2])
                            {
                                UpdateIslandUI(2);
                            }
                            else if (island == Islands[3])
                            {
                                UpdateIslandUI(3);
                                UpdateTotalUI();
                            }
                        }

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
            CollectResults(true);

        }

        private void UpdateIslandUI(int nr)
        {
            ObservableCollection < ISeries > target = new ObservableCollection<ISeries>();
            List<Antibody> antibodies = Islands[nr].GetAntibodies();
            switch (nr) 
            { 
                case 0:
                    target = DashboardWindow.SmallSeries1;
                    break;
                case 1:
                    target = DashboardWindow.SmallSeries2;
                    break;
                case 2:
                    target = DashboardWindow.SmallSeries3;
                    break;
                case 3:
                    target = DashboardWindow.SmallSeries4;
                    break;
                default:
                    target = DashboardWindow.SmallSeries1;
                    break;
            }

            // Update the UI with the results
            DashboardWindow.Dispatcher.Invoke(() =>
            {
                DashboardWindow.AddToSeries(target, CalculateUIMetrics(antibodies, false));
            });
        }
        
        private void UpdateTotalUI()
        {
            // Update the UI with the results
            DashboardWindow.Dispatcher.Invoke(() =>
            {
                DashboardWindow.AddToSeries(DashboardWindow.LargeSeries, CalculateUIMetrics(this.GatherAntibodies(), true));
            });
        }

        private double[] CalculateUIMetrics(List<Antibody> antibodies, bool main)
        {
            double[] metrics = new double[6];
            if (main) { metrics = new double[7]; }
            metrics[0] = antibodies.Average(a => a.GetFitness().GetTotalFitness());
            metrics[1] = antibodies.Average(a => a.GetFitness().GetCorrectness());
            metrics[2] = antibodies.Average(a => a.GetFitness().GetCoverage());
            metrics[3] = antibodies.Average(a => a.GetFitness().GetUniqueness());
            metrics[4] = antibodies.Average(a => a.GetFitness().GetValidAvidity());
            metrics[5] = antibodies.Average(a => a.GetFitness().GetInvalidAvidity());
            if (main)
            {
                VALIS.AssingAGClassByVoting(this.GatherAntibodies(), this.Antigens);
                (metrics[6], _) = FitnessFunctions.CalculateTotalFitness(this.Antigens);
            }

            return metrics;
        }

        public List<Antibody> GatherAntibodies()
        {
            return Islands.SelectMany(i => i.GetAntibodies()).ToList();
        }

        private void CollectResults(bool ShowWindow)
        {
            VALIS.AssingAGClassByVoting(this.GatherAntibodies(), this.Antigens);

            // Calculate the fitness of the antibodies
            (double trainFitness, double trainUnassigned) = FitnessFunctions.CalculateTotalFitness(this.Antigens);

            System.Diagnostics.Trace.WriteLine(trainFitness);
            System.Diagnostics.Trace.WriteLine(trainUnassigned);
            ShowClassDistribution();
            if (ShowWindow)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ResultsWindow resultsWindow = new ResultsWindow();
                    resultsWindow.ShowClassificationResults(this.Antigens, trainFitness);
                });
            }
        }

        private void ShowClassDistribution()
        {
            for (int i = 0; i < LabelEncoder.ClassCount; i++)
            {
                double totalOfClass = 0;
                foreach (Antibody AB in this.GatherAntibodies())
                {
                    if (AB.GetClass() == i)
                    {
                        totalOfClass += 1;
                    }
                }
                System.Diagnostics.Trace.WriteLine($"Class {i}: {totalOfClass / (this.Islands[0].GetAntibodies().Count * 4)}");
            }
        }
    }
}

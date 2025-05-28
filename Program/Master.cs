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
using System.Diagnostics;
using AISIGA.Program.Experiments.ExpResults;
using System.IO;

namespace AISIGA.Program
{
    class Master
    {
        private AbstractExperimentConfig Config { get; set; }
        private List<Island> Islands { get; set; }
        private List<Antibody> BestAntibodyNetwork { get; set; }
        private double BestAntibodyNetworkTestAccuracy { get; set; }
        private double BestAntibodyNetworkTrainAccuracy { get; set; }
        private List<AIS.Antigen> TrainAntigens { get; set; }
        private List<AIS.Antigen> TestAntigens { get; set; }
        private DashboardWindow DashboardWindow { get; set; } // UI
        private bool shownUI { get; set; }

        // UI Variables

        public Master(AbstractExperimentConfig config, DashboardWindow dashboardWindow)
        {
            Config = config;
            EVOFunctions.Config = this.Config;
            FitnessFunctions.Config = this.Config;
            this.BestAntibodyNetwork = new List<Antibody>();
            this.BestAntibodyNetworkTestAccuracy = -1;
            this.BestAntibodyNetworkTrainAccuracy = -1;
            Islands = new List<Island>();
            TrainAntigens = new List<AIS.Antigen>();
            DashboardWindow = dashboardWindow;
            shownUI = false;
        }

        public void Initialize()
        {
            List<Antigen> allAntigens = Data.DataHandler.TranslateDataToAntigens(Config.DataSetNr);
            List<Result> results = new List<Result>();
            for (int run = 0; run < 20; run++)
            {
                List<(List<Antigen> Train, List<Antigen> Test)> folds = Data.DataHandler.GenerateStratifiedKFolds(allAntigens, Config.KFoldCount);

                for (int i = 0; i < folds.Count; i++)
                {
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    InitIslands();
                    this.TrainAntigens = folds[i].Train;
                    this.TestAntigens = folds[i].Test;
                    DivideAntigenAndAntibodies();
                    StartExperiment();
                    stopwatch.Stop();
                    System.Diagnostics.Trace.WriteLine($"Run {run + 1}, Fold {i + 1}: Elapsed time: {stopwatch.Elapsed.TotalSeconds} seconds");
                    System.Diagnostics.Trace.WriteLine($"Run {run + 1}, Fold {i + 1}: Test Accuracy: {BestAntibodyNetworkTestAccuracy}%");
                    System.Diagnostics.Trace.WriteLine($"Run {run + 1}, Fold {i + 1}: Train Accuracy: {BestAntibodyNetworkTrainAccuracy}%");
                    results.Add(new Result(run, i, BestAntibodyNetworkTestAccuracy, BestAntibodyNetworkTrainAccuracy, stopwatch.Elapsed.TotalSeconds));
                    this.Reset();
                    if (Config.UseUI && !shownUI)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            this.DashboardWindow = new DashboardWindow();
                            this.DashboardWindow.Show();
                        });
                        //stop creating new windows after one run/fold
                        shownUI = true;
                    }
                }
            }
            // Save the results to a file
            SaveResultsToCsv(results, "results.csv");
            // Calculate metrics & print them
            CalculateResultMetrics(results);
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

        private void DivideAntigenAndAntibodies()
        {
            List<Antibody> antibodies = new List<Antibody>();
            for (int i = 0; i < (this.TrainAntigens.Count * Config.PopulationSizeFractionOfDatapoints); i++)
            {
                antibodies.Add(new AIS.Antibody(-1, -1, this.TrainAntigens[0].GetLength()));
            }
            RandomizeAntibodies(antibodies);

            if (Config.DivideAntigens)
            {
                //Divide the TrainAntigens into the islands Round robin style
                for (int i = 0; i < this.TrainAntigens.Count; i++)
                {
                    int islandIndex = i % 4;
                    this.Islands[islandIndex].AddAntigen(this.TrainAntigens[i]);
                }
            }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    this.Islands[i].AddAllAntigens(this.TrainAntigens);
                }
            }

            for (int i = 0; i < antibodies.Count; i++) {
                int islandIndex = i % 4;
                this.Islands[islandIndex].AddAntibody(antibodies[i]);
            }            
        }

        private void RandomizeAntibodies(List<Antibody> antibodies)
        {
            List<double> classDistributionFractions = Data.DataHandler.CalcClassDistribution(this.TrainAntigens);
            int featureCount = TrainAntigens[0].GetLength();

            List<double[]> classMinValues = new List<double[]>();
            List<double[]> classMaxValues = new List<double[]>();

            for (int j = 0; j < classDistributionFractions.Count; j++)
            {
                List<Antigen> TrainAntigensOfOneClass = TrainAntigens
                    .Where(ag => ag.GetActualClass() == j)
                    .ToList();
                // Find the min and max values of the AntiGens per class
                double[] maxValues = new double[featureCount];
                double[] minValues = new double[featureCount];

                // Initialize with the first antibody
                for (int i = 0; i < featureCount; i++)
                {
                    maxValues[i] = TrainAntigensOfOneClass[0].GetFeatureValueAt(i);
                    minValues[i] = TrainAntigensOfOneClass[0].GetFeatureValueAt(i);
                }

                // Iterate through the rest
                foreach (var antigen in TrainAntigensOfOneClass.Skip(1))
                {
                    for (int i = 0; i < featureCount; i++)
                    {
                        if (antigen.GetFeatureValueAt(i) > maxValues[i])
                            maxValues[i] = antigen.GetFeatureValueAt(i);

                        if (antigen.GetFeatureValueAt(i) < minValues[i])
                            minValues[i] = antigen.GetFeatureValueAt(i);
                    }
                }

                // Add the slight offset of 10%
                for (int i = 0; i < featureCount; i++)
                {
                    maxValues[i] *= 1.1;
                    minValues[i] *= 1.1;
                }
                classMinValues.Add(minValues);
                classMaxValues.Add(maxValues);
            }

            


            // Set AB class based on AG class distribution
            int totalCount = antibodies.Count;

            // Calculate target distribution count for each class
            List<int> targetCounts = classDistributionFractions
                .Select(fraction => (int)Math.Round(fraction * totalCount))
                .ToList();

            //Make sure there are no sisues with rounding
            while (targetCounts.Sum() != totalCount)
            {
                if (targetCounts.Sum() < totalCount)
                {
                    targetCounts[RandomProvider.GetThreadRandom().Next(0, targetCounts.Count)] += 1;
                }
                else
                {
                    targetCounts[RandomProvider.GetThreadRandom().Next(0, targetCounts.Count)] -= 1;
                }
            }

            // Force each class to have at least 10 antibodies
            int extraAdded = 0;
            foreach (int i in targetCounts)
            {
                if (i < 1)
                {
                    targetCounts[i] = 1; // Ensure a minimum of 1 antibodies per class
                    extraAdded += 1;
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
            foreach (Antibody antibody in antibodies)
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

                antibody.AssingClass(selectedClass);

                // Assign the feature values and multipliers
                antibody.AssignRandomFeatureValuesAndMultipliers(classMaxValues, classMinValues, Config.UseHyperSpheres, Config.UseUnboundedRegions, Config.RateOfUnboundedRegions);

                // Radius based on a random AB
                // Get all AG of same class
                List<Antigen> sameClassAntigens = TrainAntigens
                                    .Where(ag => ag.GetActualClass() == selectedClass)
                                    .ToList();
                // Select a random one
                Antigen selectedAntigen = sameClassAntigens[rnd.Next(0, sameClassAntigens.Count)];
                // Get the distance to that AG
                double SqrdDistance = 0.0;
                for (int i = 0; i < antibody.GetFeatureMultipliers().Length; i++)
                {
                    double diff = antibody.GetFeatureValueAt(i) - selectedAntigen.GetFeatureValueAt(i);
                    SqrdDistance += diff * diff;
                }

                antibody.AssingRadius(Math.Sqrt(SqrdDistance));
            }
        }

        public void StartExperiment()
        {
            int islandCount = Islands.Count;
            int MigCount = 0;

            Barrier barrier = new Barrier(islandCount, (b) =>
            {
                // This code runs ONCE after all threads hit the barrier
                foreach (var island in Islands)
                {
                    island.Migrate(); // Shared migration logic
                }
                MigCount++;
                if (Config.MasterMigrationFreq > 0)
                {
                    if (MigCount >= Config.MasterMigrationFreq)
                    {
                        List<Antibody> allAntibodies = GatherAntibodies();
                        VALIS.AssingAGClassByVoting(allAntibodies, this.TestAntigens);
                        (double newFitness, _) = FitnessFunctions.CalculateTotalFitness(this.TestAntigens);
                        if (newFitness > BestAntibodyNetworkTestAccuracy)
                        {
                            List<Antibody> CopiedBestAntibodies = new List<Antibody>();
                            foreach (Antibody AB in allAntibodies)
                            {
                                Antibody copyAB = new Antibody(AB.GetClass(), AB.GetBaseRadius(), AB.GetFeatureValues(), AB.GetFeatureMultipliers(), AB.GetFeatureDimTypes(), AB.GetFitness(), true);
                                CopiedBestAntibodies.Add(copyAB);
                            }
                            BestAntibodyNetwork = CopiedBestAntibodies;
                            BestAntibodyNetworkTestAccuracy = newFitness;
                            (BestAntibodyNetworkTrainAccuracy, _) = FitnessFunctions.CalculateTotalFitness(this.TrainAntigens);
                        }
                        MigCount = 0;
                    }
                }
                if (Config.UseUI && !shownUI)
                {
                    List<Antibody> antibodies = this.GatherAntibodies();
                    VALIS.AssingAGClassByVoting(antibodies, this.TrainAntigens);
                    (double trainAcc, _) = FitnessFunctions.CalculateTotalFitness(this.TrainAntigens);
                    VALIS.AssingAGClassByVoting(antibodies, this.TestAntigens);
                    (double testAcc, _) = FitnessFunctions.CalculateTotalFitness(this.TestAntigens);
                    // Update the UI with the results
                    UpdateIslandUI(0);
                    UpdateIslandUI(1);
                    UpdateIslandUI(2);
                    UpdateIslandUI(3);
                    UpdateTotalUI(antibodies, trainAcc, testAcc);
                }
            })
            {

            };
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
                DashboardWindow.AddToSeries(target, CalculateUIMetrics(antibodies, false, 0, 0));
            });
        }
        
        private void UpdateTotalUI(List<Antibody> antibodies, double testAcc, double trainAcc)
        {
            // Update the UI with the results
            DashboardWindow.Dispatcher.Invoke(() =>
            {
                DashboardWindow.AddToSeries(DashboardWindow.LargeSeries, CalculateUIMetrics(antibodies, true, testAcc, trainAcc));
            });
        }

        private double[] CalculateUIMetrics(List<Antibody> antibodies, bool main, double TestAcc, double TrainAcc)
        {
            double[] metrics = new double[6];
            if (main) { metrics = new double[8]; }
            metrics[0] = antibodies.Average(a => a.GetFitness().GetTotalFitness()) *10;
            metrics[1] = antibodies.Average(a => a.GetFitness().GetCorrectness()) * 10;
            metrics[2] = antibodies.Average(a => a.GetFitness().GetCoverage()) * 10;
            metrics[3] = antibodies.Average(a => a.GetFitness().GetUniqueness()) * 10;
            metrics[4] = antibodies.Average(a => a.GetFitness().GetValidAvidity()) * 10;
            metrics[5] = antibodies.Average(a => a.GetFitness().GetInvalidAvidity()) * 10;
            if (main)
            {
                metrics[6] = TestAcc;
                metrics[7] = TrainAcc;
            }

            return metrics;
        }

        public List<Antibody> GatherAntibodies()
        {
            return Islands.SelectMany(i => i.GetAntibodies()).ToList();
        }

        private void CollectResults(bool ShowWindow)
        {
            if (Config.MasterMigrationFreq == 0)
            {
                List<Antibody> allAntibodies = GatherAntibodies();
                VALIS.AssingAGClassByVoting(allAntibodies, this.TestAntigens);
                (double newFitness, _) = FitnessFunctions.CalculateTotalFitness(this.TestAntigens);

                List<Antibody> CopiedBestAntibodies = new List<Antibody>();
                foreach (Antibody AB in allAntibodies)
                {
                    Antibody copyAB = new Antibody(AB.GetClass(), AB.GetBaseRadius(), AB.GetFeatureValues(), AB.GetFeatureMultipliers(), AB.GetFeatureDimTypes(), AB.GetFitness(), true);
                    CopiedBestAntibodies.Add(copyAB);
                }
                BestAntibodyNetwork = CopiedBestAntibodies;
                BestAntibodyNetworkTestAccuracy = newFitness;
            }

            VALIS.AssingAGClassByVoting(this.BestAntibodyNetwork, this.TrainAntigens);
            (double trainFitness, _) = FitnessFunctions.CalculateTotalFitness(this.TrainAntigens);
            this.BestAntibodyNetworkTrainAccuracy = trainFitness;


            //ShowClassDistribution();
            if (ShowWindow && Config.UseUI && !shownUI)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ResultsWindow resultsWindow = new ResultsWindow();
                    resultsWindow.ShowClassificationResults(this.TestAntigens, this.BestAntibodyNetworkTestAccuracy);
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
                //System.Diagnostics.Trace.WriteLine($"Class {i}: {totalOfClass}");
                System.Diagnostics.Trace.WriteLine($"Class {i}: {totalOfClass / (this.Islands[0].GetAntibodies().Count * 4)}");
            }
        }

        private void SaveResultsToCsv(List<Result> results, string filePath)
        {
            var sb = new StringBuilder();

            // Write CSV header
            sb.AppendLine("RunNumber,FoldNumber,TestAccuracy,TrainAccuracy,Time");

            // Write each result as a CSV row
            foreach (var r in results)
            {
                sb.AppendLine($"{r.RunNumber},{r.FoldNumber},{r.TestAccuracy},{r.TrainAccuracy},{r.Time}");
            }

            // Write to file
            File.WriteAllText(filePath, sb.ToString());
        }

        private void CalculateResultMetrics(List<Result> results)
        {
            // Average and std dev per fold
            var metricsPerFold = results
                .GroupBy(r => r.FoldNumber)
                .Select(g => new
                {
                    Fold = g.Key,
                    AvgTestAccuracy = g.Average(r => r.TestAccuracy),
                    StdDevTestAccuracy = Math.Sqrt(g.Select(r => Math.Pow(r.TestAccuracy - g.Average(x => x.TestAccuracy), 2)).Average()),
                    AvgTrainAccuracy = g.Average(r => r.TrainAccuracy),
                    StdDevTrainAccuracy = Math.Sqrt(g.Select(r => Math.Pow(r.TrainAccuracy - g.Average(x => x.TrainAccuracy), 2)).Average()),
                    AvgTime = g.Average(r => r.Time),
                    StdDevTime = Math.Sqrt(g.Select(r => Math.Pow(r.Time - g.Average(x => x.Time), 2)).Average())
                })
                .ToList();

            foreach (var foldMetrics in metricsPerFold)
            {
                System.Diagnostics.Trace.WriteLine(
                    $"Fold {foldMetrics.Fold}: " +
                    $"Test Acc = {foldMetrics.AvgTestAccuracy:F2} ± {foldMetrics.StdDevTestAccuracy:F2}, " +
                    $"Train Acc = {foldMetrics.AvgTrainAccuracy:F2} ± {foldMetrics.StdDevTrainAccuracy:F2}, " +
                    $"Time = {foldMetrics.AvgTime:F2}s ± {foldMetrics.StdDevTime:F2}s");
            }

            // Overall average and std dev across all folds and runs
            double avgTestAccuracy = results.Average(r => r.TestAccuracy);
            double stdDevTestAccuracy = Math.Sqrt(results.Select(r => Math.Pow(r.TestAccuracy - avgTestAccuracy, 2)).Average());

            double avgTrainAccuracy = results.Average(r => r.TrainAccuracy);
            double stdDevTrainAccuracy = Math.Sqrt(results.Select(r => Math.Pow(r.TrainAccuracy - avgTrainAccuracy, 2)).Average());

            double avgTime = results.Average(r => r.Time);
            double stdDevTime = Math.Sqrt(results.Select(r => Math.Pow(r.Time - avgTime, 2)).Average());

            System.Diagnostics.Trace.WriteLine(
                $"Overall Test Accuracy: {avgTestAccuracy:F2} ± {stdDevTestAccuracy:F2}");
            System.Diagnostics.Trace.WriteLine(
                $"Overall Train Accuracy: {avgTrainAccuracy:F2} ± {stdDevTrainAccuracy:F2}");
            System.Diagnostics.Trace.WriteLine(
                $"Overall Time: {avgTime:F2} seconds ± {stdDevTime:F2} seconds");
        }


        private void Reset()
        {
            this.Islands.Clear(); // Clear the islands for the next fold
            this.BestAntibodyNetwork.Clear();
            this.BestAntibodyNetworkTestAccuracy = -1;
            this.BestAntibodyNetworkTrainAccuracy = -1;
            this.TrainAntigens.Clear();
            this.TestAntigens.Clear();
        }
    }
}

using AISIGA.Program.AIS;
using AISIGA.Program.Tests;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace AISIGA.Program.Data
{
    public static class DataHandler
    {
        public static (List<Antigen>, List<Antigen>) TranslateDataToAntigens(int DataSetNr)
        {
            List<Antigen> Antigens = new List<Antigen>();

            String filePath = "";
            int labelIndex = 0;

            switch (DataSetNr)
            {
                case 0:
                    // Load the test dataset
                    filePath = "Program/Data/Datasets/spirals.txt";
                    labelIndex = 2;
                    break;
                case 1:
                    filePath = "Program/Data/Datasets/wine.data";
                    labelIndex = 0;
                    break;
                case 2:
                    filePath = "Program/Data/Datasets/diabetes.csv";
                    labelIndex = 8;
                    break;
                case 3:
                    filePath = "Program/Data/Datasets/ionosphere.data";
                    labelIndex = 0;
                    break;
                case 4:
                    filePath = "Program/Data/Datasets/sonar.txt";
                    labelIndex = 60;
                    break;
                case 5:
                    filePath = "Program/Data/Datasets/iris.data";
                    labelIndex = 4;
                    break;
                case 6:
                    filePath = "Program/Data/Datasets/glass.data";
                    labelIndex = 9;
                    break;
                default:
                    throw new ArgumentException("Invalid dataset number");
            }

            // 1. Read all entries into raw feature vectors
            List<double[]> rawFeatureVectors = new List<double[]>();
            List<string> labels = new List<string>();

            var entries = System.IO.File.ReadAllLines(filePath);

            foreach (var entry in entries)
            {
                if (string.IsNullOrWhiteSpace(entry)) continue;

                var values = entry.Split(',');

                if (values.Any(p => string.IsNullOrWhiteSpace(p))) continue;

                try
                {
                    double[] featureValues = values
                        .Where((_, index) => index != labelIndex)
                        .Select(v => double.Parse(v, CultureInfo.InvariantCulture))
                        .ToArray();

                    rawFeatureVectors.Add(featureValues);
                    labels.Add(values[labelIndex]);
                }
                catch (FormatException ex)
                {
                    Console.WriteLine($"Error parsing entry: {entry}. Exception: {ex.Message}");
                    continue;
                }
            }

            // 2. Calculate min and max per feature dimension
            int featureCount = rawFeatureVectors[0].Length;
            double[] mins = new double[featureCount];
            double[] maxs = new double[featureCount];

            for (int i = 0; i < featureCount; i++)
            {
                mins[i] = rawFeatureVectors.Min(f => f[i]);
                maxs[i] = rawFeatureVectors.Max(f => f[i]);
            }

            // 3. Normalize and create antigens
            for (int i = 0; i < rawFeatureVectors.Count; i++)
            {
                double[] normalized = new double[featureCount];
                for (int j = 0; j < featureCount; j++)
                {
                    double value = rawFeatureVectors[i][j];
                    if (mins[j] == maxs[j])
                        normalized[j] = 0.0; // Avoid divide by zero
                    else
                        normalized[j] = (value - mins[j]) / (maxs[j] - mins[j]);
                }

                Antigen newAntigen = new Antigen(-1, LabelEncoder.Encode(labels[i]), featureCount);
                newAntigen.SetFeatureValues(normalized);
                Antigens.Add(newAntigen);
            }

            return SplitDataSet(Antigens, 0.8);
        }

        public static List<double> CalcClassDistribution(List<Antigen> antigens)
        {
            int totalAntigens = antigens.Count;
            int[] classCounts = new int[LabelEncoder.ClassCount];

            // Count the number of occurrences of each class in the antigens
            foreach (var antigen in antigens)
            {
                classCounts[antigen.GetActualClass()]++;
            }

            // Calculate fractions
            List<double> classFractions = new List<double>();
            for (int i = 0; i < LabelEncoder.ClassCount; i++)
            {
                double fraction = (double)classCounts[i] / totalAntigens;
                classFractions.Add(fraction);
            }

            return classFractions;
        }

        public static (List<Antigen>, List<Antigen>) SplitDataSet(List<Antigen> antigens, double fraction)
        {
            List<Antigen> trainSet = new List<Antigen>();
            List<Antigen> testSet = new List<Antigen>();

            var random = new Random();

            // Group by class label
            var groupedByClass = antigens
                .GroupBy(a => a.GetActualClass())
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var kvp in groupedByClass)
            {
                var classLabel = kvp.Key;
                var items = kvp.Value;

                // Shuffle the list
                items = items.OrderBy(_ => random.Next()).ToList();

                int trainCount = (int)(items.Count * fraction);

                trainSet.AddRange(items.Take(trainCount));
                testSet.AddRange(items.Skip(trainCount));
            }
            return (trainSet, testSet);
        }
    }
}

using AISIGA.Program.AIS;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace AISIGA.Program.Data
{
    static class DataHandler
    {
        public static List<Antigen> TranslateDataToAntigens(int DataSetNr)
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
                    // Load the first dataset
                    break;
                case 2:
                    // Load the second dataset
                    break;
                case 3:
                    // Load the third dataset
                    break;
                default:
                    throw new ArgumentException("Invalid dataset number");
            }

            var entries = System.IO.File.ReadAllLines(filePath);

            foreach (var entry in entries)
            {
                if (string.IsNullOrWhiteSpace(entry)) continue;

                var values = entry.Split(',');

                if (values.Any(p => string.IsNullOrWhiteSpace(p))) continue;

                double[] featureValues;

                try
                {
                    featureValues = values
                        .Where((_, index) => index != labelIndex)
                        .Select(v => double.Parse(v, CultureInfo.InvariantCulture))
                        .ToArray();
                }
                catch (FormatException ex)
                {
                    Console.WriteLine($"Error parsing entry: {entry}. Exception: {ex.Message}");
                    continue;
                }

                Antigen newAntigen = new Antigen(-1, LabelEncoder.Encode(values[labelIndex]), featureValues.Length);
                newAntigen.SetFeatureValues(featureValues);
                Antigens.Add(newAntigen);
            }


            return (Antigens);
        }


    }
}

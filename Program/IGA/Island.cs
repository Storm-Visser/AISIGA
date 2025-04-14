using AISIGA.Program.AIS;
using AISIGA.Program.Experiments;
using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
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
    }
}

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

        private List<AIS.Antigen> Antigens { get; set; }
        private List<AIS.Antibody> Antibodies { get; set; }

        // UI Variables

        public Master(ExperimentConfig config)
        {
            Config = config;
            Islands = new List<Island>();
            Antigens = new List<AIS.Antigen>();
            Antibodies = new List<AIS.Antibody>();
            //Initialize();
        }

        public void Initialize()
        {
            InitIslands();
            InitAntigensAndAntibodies();
            DivideAntigenAndAntibodies();
            RandomizeAntibodies();
            Console.WriteLine("Done");

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
            this.Antigens = Data.DataHandler.TranslateDataToAntigens(Config.DataSetNr);
            for (int i = 0; i < Config.PopulationSize; i++)
            {
                this.Antibodies.Add(new AIS.Antibody(-1, Config.BaseRadius, this.Antigens[0].GetLength()));
            }
        }

        private void DivideAntigenAndAntibodies()
        {
            //Shuffle first
            this.Antigens = this.Antigens.OrderBy(a => RandomProvider.GetThreadRandom().Next()).ToList();

            //Divide the Antigens into the islands Round robin style
            for (int i = 0; i < this.Antigens.Count; i++)
            {
                int islandIndex = i % 4;
                this.Islands[islandIndex].AddAntigen(this.Antigens[i]);
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
    }
}

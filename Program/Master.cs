using AISIGA.Program.Experiments;
using AISIGA.Program.IGA;
using System;
using System.Collections.Generic;
using System.Linq;
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
            DivideAntibodies();
        }

        private void InitIslands()
        {
            for (int i = 0; i < Config.NumberOfIslands; i++)
            {
                Island island = new Island();
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
        }

        private void InitAntigensAndAntibodies()
        {
            this.Antigens = Data.DataHandler.TranslateDataToAntigens(Config.DataSetNr);
            foreach (AIS.Antigen antigen in Antigens)
            {
                this.Antibodies.Add(new AIS.Antibody(-1, -1, this.Antigens[0].GetLength()));
            }
        }

        private void DivideAntibodies()
        {

        }
    }
}

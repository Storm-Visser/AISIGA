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
        private List<AIS.Antigen> Antigens { get; set; }
        private List<AIS.Antibody> Antibodies { get; set; }

        private Island Neighbour { get; set; }

        public Island()
        {
            Antigens = new List<AIS.Antigen>();
            Antibodies = new List<AIS.Antibody>();
        }

        public void SetNeighbour(Island neighbour)
        {
            Neighbour = neighbour;
        }
    }
}

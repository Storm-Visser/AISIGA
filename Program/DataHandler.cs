using AISIGA.Program.AIS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace AISIGA.Program
{
    static class DataHandler
    {
        public static List<Antigen> TranslateDataToAntigens(int DataSetNr)
        {
            List<Antigen> Antigens = new List<Antigen>();

            List<Object> Dataset = new List<Object>();
            switch (DataSetNr)
            {
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

            foreach (Object Datapoint in Dataset)
            {
                //Antigen newAntigen = new Antigen(
                //    assignedClass: Datapoint.GetAssignedClass(),
                //    actualClass: Datapoint.GetActualClass(),
                //    baseRadius: Datapoint.GetBaseRadius(),
                //    amountOfFeatures: Datapoint.GetAmountOfFeatures()
                //);
                //Antigens.Add(newAntigen);
            }


            return Antigens;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISIGA.Program.Experiments.ExpResults
{
    struct Result
    {
        public int RunNumber { get; set; }
        public int FoldNumber { get; set; }
        public double TestAccuracy { get; set; }
        public double TrainAccuracy { get; set; }
        public double Time { get; set; }

        public Result(int runNumber, int foldNumber, double testAccuracy, double trainAccuracy, double time)
        {
            RunNumber = runNumber;
            FoldNumber = foldNumber;
            TestAccuracy = testAccuracy;
            TrainAccuracy = trainAccuracy;
            Time = time;
        }
    }
}

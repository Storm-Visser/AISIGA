using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISIGA.Program.Experiments.ExpResults
{
    struct FitnessSnapshot
    {
        public int RunNumber { get; set; }
        public int FoldNumber { get; set; }
        public int Generation { get; set; }

        public double TestAccuracy { get; set; }
        public double TrainAccuracy { get; set; }

        public double TotalFitness { get; set; }
        public double Correctness { get; set; }
        public double Coverage { get; set; }
        public double Uniqueness { get; set; }
        public double ValidAvidity { get; set; }
        public double InvalidAvidity { get; set; }

        public FitnessSnapshot(
            int runNumber,
            int foldNumber,
            int generation,
            double testAccuracy,
            double trainAccuracy,
            double totalFitness,
            double correctness,
            double coverage,
            double uniqueness,
            double validAvidity,
            double invalidAvidity)
        {
            RunNumber = runNumber;
            FoldNumber = foldNumber;
            Generation = generation;
            TestAccuracy = testAccuracy;
            TrainAccuracy = trainAccuracy;
            TotalFitness = totalFitness;
            Correctness = correctness;
            Coverage = coverage;
            Uniqueness = uniqueness;
            ValidAvidity = validAvidity;
            InvalidAvidity = invalidAvidity;
        }
    }
}

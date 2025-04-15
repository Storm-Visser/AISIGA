using AISIGA.Program.Experiments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISIGA.Program.IGA
{
    class Fitness
    {
        private bool IsCalculated { get; set; }
        private double TotalFitness { get; set; }
        private double Correctness { get; set; }
        private double Coverage { get; set; }
        private double Uniqueness { get; set; }
        private double ValidAvidity { get; set; }
        private double InvalidAvidity { get; set; }

        public Fitness()
        {
            IsCalculated = false;
            TotalFitness = 0;
            Correctness = 0;
            Coverage = 0;
            Uniqueness = 0;
            ValidAvidity = 0;
            InvalidAvidity = 0;
        }

        public double GetTotalFitness()
        {
            if (!IsCalculated)
            {
                CalculateFitness();
            }
            return TotalFitness;
        }

        private void CalculateFitness()
        {
            //todo add the connection to fitness functions below
            TotalFitness = Correctness + Coverage + Uniqueness + ValidAvidity + InvalidAvidity;
            IsCalculated = true;
        }
    }
    static class FitnessFunctions
    {
        public static ExperimentConfig ?Config { get; set; }
    }
}

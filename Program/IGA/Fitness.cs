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

        public Fitness(Fitness fitness, bool IsCalculationStillValid)
        {
            IsCalculated = IsCalculationStillValid;
            TotalFitness = fitness.TotalFitness;
            Correctness = fitness.Correctness;
            Coverage = fitness.Coverage;
            Uniqueness = fitness.Uniqueness;
            ValidAvidity = fitness.ValidAvidity;
            InvalidAvidity = fitness.InvalidAvidity;
        }

        public bool GetIsCalculated()
        {
            return IsCalculated;
        }

        public void SetIsCalculated(bool isCalculated)
        {
            IsCalculated = isCalculated;
        }

        public double GetTotalFitness()
        {
            return TotalFitness;
        }

        public void SetTotalFitness(double totalFitness)
        {
            TotalFitness = totalFitness;
        }

        public double GetCorrectness()
        {
            return Correctness;
        }

        public void SetCorrectness(double correctness)
        {
            Correctness = correctness;
        }

        public double GetCoverage()
        {
            return Coverage;
        }

        public void SetCoverage(double coverage)
        {
            Coverage = coverage;
        }

        public double GetUniqueness()
        {
            return Uniqueness;
        }

        public void SetUniqueness(double uniqueness)
        {
            Uniqueness = uniqueness;
        }

        public double GetValidAvidity()
        {
            return ValidAvidity;
        }

        public void SetValidAvidity(double validAvidity)
        {
            ValidAvidity = validAvidity;
        }

        public double GetInvalidAvidity()
        {
            return InvalidAvidity;
        }

        public void SetInvalidAvidity(double invalidAvidity)
        {
            InvalidAvidity = invalidAvidity;
        }
    }

}

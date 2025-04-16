using AISIGA.Program.AIS;
using AISIGA.Program.Experiments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
    static class FitnessFunctions
    {
        public static ExperimentConfig ?Config { get; set; }

        public static double CalculateCorrectness(double TruePositives, double FalsePositives)
        {
            return (TruePositives - (FalsePositives * 2)) / FalsePositives + TruePositives;
        }
        public static double CalculateCoverage(double TruePositives, double AllPositives)
        {
            return TruePositives/AllPositives;
        }

        public static double CalculateUniqueness(double TruePositives, List<Antibody> antibodies, List<Antigen> matchedAntigens)
        {
            return CalculateSharedAffinity(antibodies, matchedAntigens) /TruePositives;
        }



        public static double CalculateAvidity()
        {
            return 0.0;
        }

        public static double CalculateInvalidAvidity()
        {
            return 0.0;
        }


        private static double CalculateSharedAffinity(List<Antibody> antibodies, List<Antigen> matchedAntigens)
        {
            double sharedAffinity = 0.0;
            
            return sharedAffinity;
        }

        private static bool IsAntigenMatched(Antibody antibody, Antigen antigen)
        {
            if (Config == null)
            {
                throw new Exception("Config is not set. Please set the config before calling this function.");
            }
            // Make a sum of the distance in each dimension
            double distance = 0.0;            

            // Loop through all dimensions
            for (int i = 0; i < antibody.GetFeatureMultipliers().Length; i++)
            {
                
                if (Config.UseUnboundedRegions)
                {
                    //If we use UBR, we need to check if thsi specific dim is a unbounded
                    if (true)
                    {
                        // If so, we calculate the distance without squaring it
                        distance += (antigen.GetFeatureValueAt(i) - antibody.GetFeatureValueAt(i)) * antibody.GetFeatureMultipliers()[i];
                    }
                }
                else
                {
                    // If we dont use UBR or this dim is not unbounded, we calculate the distance and square it
                    distance += Math.Pow((antigen.GetFeatureValueAt(i) - antibody.GetFeatureValueAt(i)) * antibody.GetFeatureMultipliers()[i], 2) ;
                }
                    
            }
            // Then we substract the base radius of the AB
            distance -= antibody.GetBaseRadius();
            // If the total is smaller than or equal to 0, we have a match
            return distance <= 0;
        }

        public static double GetMatchedAntigens(Antibody antibody, List<Antigen> antigens)
        {


            return 0.0;
        }

    }
}

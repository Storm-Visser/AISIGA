using AISIGA.Program.AIS;
using AISIGA.Program.Experiments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AISIGA.Program.IGA
{
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



        public static double CalculateAvidity(List<Antigen> matchedAntigens, double[] matchScores)
        {
            double totalAvidity = 0.0;
            int matchedCount = 0;
            for (int i = 0; i < matchedAntigens.Count; i++)
            {
                Antigen AG = matchedAntigens[i];
                if (AG.GetAssignedClass() == AG.GetActualClass())
                {
                    totalAvidity += Sigmoid(matchScores[i]);
                    matchedCount++;
                }
            }
            return totalAvidity / matchedCount;
        }

        public static double CalculateInvalidAvidity(List<Antigen> matchedAntigens, double[] matchScores)
        {
            double totalAvidity = 0.0;
            int matchedCount = 0;
            for (int i = 0; i < matchedAntigens.Count; i++)
            {
                Antigen AG = matchedAntigens[i];
                if (AG.GetAssignedClass() != AG.GetActualClass())
                {
                    totalAvidity += Sigmoid(matchScores[i]);
                    matchedCount++;
                }
            }
            return totalAvidity / matchedCount;
        }


        private static double CalculateSharedAffinity(List<Antibody> antibodies, List<Antigen> matchedAntigens)
        {
            double sharedAffinity = 0.0;
            foreach (Antigen AG in matchedAntigens)
            {
                double sharedCount = 0.0;
                // Loop through all antibodies
                foreach (Antibody AB in antibodies)
                {
                    // Check if the antibody matches the antigen
                    double distance = IsAntigenMatched(AB, AG);
                    if (distance <= 0)
                    {
                        sharedCount++;
                    }
                }
                sharedAffinity += 1/ sharedCount;
            }
            
            return sharedAffinity;
        }

        private static double Sigmoid(double x)
        {
            return 1 - (1 / (1 + Math.Exp(-x)));
        }

        private static double IsAntigenMatched(Antibody antibody, Antigen antigen)
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
                    //If we use UBR, we need to check if this specific dim is a unbounded
                    if (antibody.GetFeatureDimTypes()[i] == 1 )
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
            return distance;
        }

        public static (List<Antigen>, double[]) GetMatchedAntigens(Antibody antibody, List<Antigen> antigens)
        {
            List <Antigen> matchedAntigens = new List<Antigen>();
            double[] matchScores = new double[antigens.Count];

            for (int i = 0; i < antigens.Count; i++)
            {
                Antigen antigen = antigens[i];
                double distance = IsAntigenMatched(antibody, antigen);
                if (distance <= 0)
                {
                    matchedAntigens.Add(antigen);
                    matchScores[i] = distance;
                }
            }

            return (matchedAntigens, matchScores);
        }

        public static double CalcTruePositives(Antibody antibody, List <Antigen> matchedAntigens)
        {
            double truePositives = 0;
            foreach (Antigen AG in matchedAntigens)
            {
                if (antibody.GetClass() == AG.GetActualClass())
                {
                    truePositives++;
                }
            }
            return truePositives;
        }

        public static double CalcFalsePositives(Antibody antibody, List<Antigen> matchedAntigens)
        {
            double falsePositives = 0;
            foreach (Antigen AG in matchedAntigens)
            {
                if (antibody.GetClass() != AG.GetActualClass())
                {
                    falsePositives++;
                }
            }
            return falsePositives;
        }
    }
}

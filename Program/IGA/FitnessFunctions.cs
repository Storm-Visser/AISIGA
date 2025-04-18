using AISIGA.Program.AIS;
using AISIGA.Program.Experiments;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AISIGA.Program.IGA
{
    static class FitnessFunctions
    {
        public static ExperimentConfig? Config { get; set; }

        public static double CalculateCorrectness(double TruePositives, double FalsePositives)
        {
            double result = (TruePositives - (FalsePositives * 2)) / (FalsePositives + TruePositives);

            return double.IsNaN(result) ? 0 : result;
        }
        public static double CalculateCoverage(double TruePositives, double AllPositives)
        {
            if (AllPositives == 0) return 0;
            return TruePositives / AllPositives;
        }

        public static double CalculateUniqueness(double TruePositives, List<Antibody> antibodies, List<Antigen> matchedAntigens)
        {
            if (TruePositives == 0) return 0;
            return CalculateSharedAffinity(antibodies, matchedAntigens) / TruePositives;
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
            if (matchedCount == 0) return 0.0;
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
            if (matchedCount == 0) return 0.0;
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
                    double distance = CalcAGtoABDistance(AB, AG);
                    if (distance <= 0)
                    {
                        sharedCount++;
                    }
                }
                // Avoid division by zero
                if (sharedCount > 0)
                {
                    sharedAffinity += 1 / sharedCount;
                }
                else
                {
                    sharedAffinity += 0;
                }
            }

            return sharedAffinity;
        }

        private static double Sigmoid(double x)
        {
            return 1 - (1 / (1 + Math.Exp(-x)));
        }


        /*
         * This function is a bit complicated, so ill try to explain.
         * We keep track of the total distance in each dimension. In case of 2d, the x distance and the y distance
         * So we have to loop through each dim.
         * Ill explain witha an example based on the dim type
         * 
         * ----Normal case (HS & HE)----
         * In a normal case, we simply add the distance per dim from the center, scaled with the multiplier. 
         * So assume a AB at (0,0) with a radius of 1 and a multiplier of (0.5, 2.0).
         * With AG (1,0) , which we expect t ebe outside because of the multiplier (radius in x dim is 0.5)
         * The distance of the AG (1,0) would be ((1-0) / 0.5)^2 + ((0-0) / 2.0)^2 = 4 + 0 = 4
         * As this distance is larger than the squared radius, we say that the pointis not in the radius.
         * ----Normal case (HS & HE)----
         * 
         * ----Unbounded case (UB)----
         * If the dimention we are looking at is unbounded, we do not square the distance.
         * As a result, every point that is smaller than the ab center will return something negative, 
         *  resulting in far away points being smaller than the radius squared
         * Take the same AB(0,0) with a radius of 1 and a multiplier of (1,1), and the first (x) dimention open.
         * Then AG (-100, 0) would be considered part of this AB
         * because ((-100 - 0) / 1) + ((0 - 0) / 1) ^ 2 = -100, which is smaller than the radius squared
         * Then, due to the interactions between dimentions, the radius of the second dim also increases.
         * AG (0, 10) would not be in the AB, But AG (-100, 10) is;
         * because ((-100 - 0) / 1) + ((10 - 0) / 1) ^ 2 = -100 + 100 = 0, which is smaller than the radius squared
         * So, the Y radius in the area smaller than the AB becomes -sqrt(X) || sqrt(X)
         * ----Unbounded case (UB)----
         * 
         * In order to allow the AB's to "change direction" to detect anything thats larger, a GetFeatureDimTypes()[x]
         * value of 2 will swith the AB and AG around the -, having the opposite effect
         * 
         * -----NOTE-----
         * This behavior works for the larger than or smaller than radius calculation, 
         * but is a bit weird for actual distance for unbounded regions, beware when using that directly
         */
        public static double CalcAGtoABDistance(Antibody antibody, Antigen antigen)
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

                if (Config.UseUnboundedRegions && antibody.GetFeatureDimTypes()[i] == 1)
                {
                    //If we use UBR, we need to check if this specific dim is a unbounded
                    // If so, we calculate the distance without squaring it
                    distance += (antigen.GetFeatureValueAt(i) - antibody.GetFeatureValueAt(i)) / antibody.GetFeatureMultipliers()[i];

                }
                else if (Config.UseUnboundedRegions && antibody.GetFeatureDimTypes()[i] == 2)
                {
                    //If we use UBR, we need to check if this specific dim is a unbounded
                    // If so, we calculate the distance without squaring it
                    // but swith the AB and AG around the - to change direction of the unbounded part
                    distance += (antibody.GetFeatureValueAt(i) - antigen.GetFeatureValueAt(i)) / antibody.GetFeatureMultipliers()[i];

                }
                else
                {
                    // If we dont use UBR or this dim is not unbounded, we calculate the distance and square it
                    distance += Math.Pow((antigen.GetFeatureValueAt(i) - antibody.GetFeatureValueAt(i)) / antibody.GetFeatureMultipliers()[i], 2);
                }
            }
            // Then we substract the base radius of the AB
            distance -= (antibody.GetBaseRadius() * antibody.GetBaseRadius());
            // If the total is smaller than or equal to 0, we have a match
            return distance;
        }

        public static (List<Antigen>, double[]) GetMatchedAntigens(Antibody antibody, List<Antigen> antigens)
        {
            List<Antigen> matchedAntigens = new List<Antigen>();
            List<double> matchScoresList = new List<double>();

            for (int i = 0; i < antigens.Count; i++)
            {
                Antigen antigen = antigens[i];
                double distance = CalcAGtoABDistance(antibody, antigen);
                if (distance <= 0)
                {
                    matchedAntigens.Add(antigen);
                    matchScoresList.Add(distance);
                }
            }
            double[] matchScores = new double[matchScoresList.Count];
            for (int i = 0; i < matchScoresList.Count; i++)
            {
                matchScores[i] = matchScoresList[i];
            }
            return (matchedAntigens, matchScores);
        }

        public static double CalcTruePositives(Antibody antibody, List<Antigen> matchedAntigens)
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

        public static double CalcAllPositivesOfClass(Antibody antibody, List<Antibody> antibodies)
        {
            double TotalABsOfSameClass = 0;
            foreach (Antibody ab in antibodies)
            {
                if (antibody.GetClass() == ab.GetClass())
                {
                    TotalABsOfSameClass++;
                }
            }
            
            return TotalABsOfSameClass == 0 ? 1 : TotalABsOfSameClass;
        }

        public static (double, double) CalculateTotalFitness(List<Antigen> antigens)
        {
            double total = 0.0;
            double totalCorrect = 0.0;
            double totalUnassigned = 0.0;

            foreach (Antigen AG in antigens)
            {
                total++;
                if (AG.GetAssignedClass() == AG.GetActualClass())
                {
                    totalCorrect++;
                }
                if (AG.GetAssignedClass() == -1)
                {
                    totalUnassigned++;
                }
            }

            return ((totalCorrect / total) * 100, (totalUnassigned / total) * 100);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISIGA.Program.AIS
{
    class Antibody
    {
        private int Class;
        private int BaseRadius;
        private double[] FeatureValues;
        private double[] FeatureMultipliers;

        public Antibody(int assignedClass, int baseRadius, int amountOfFeatures)
        {
            Class = assignedClass;
            BaseRadius = baseRadius;
            FeatureValues = new double[amountOfFeatures];
            FeatureMultipliers = new double[amountOfFeatures];
        }

        public int GetClass()
        {
            return Class;
        }

        public void SetClass(int value)
        {
            Class = value;
        }

        public int GetBaseRadius()
        {
            return BaseRadius;
        }

        public void SetBaseRadius(int value)
        {
            BaseRadius = value;
        }

        public double[] GetFeatureValues()
        {
            return FeatureValues;
        }

        public void SetFeatureValues(double[] values)
        {
            FeatureValues = values;
        }

        public double[] GetFeatureMultipliers()
        {
            return FeatureMultipliers;
        }

        public void SetFeatureMultipliers(double[] values)
        {
            FeatureMultipliers = values;
        }

        public void AssignRandomFeatureValuesAndMultipliers(double[] MaxFeatureValues, double[] MinFeatureValues)
        {
            Random random = RandomProvider.GetThreadRandom();
            for (int i = 0; i < FeatureValues.Length; i++)
            {
                FeatureValues[i] = random.NextDouble() * (MaxFeatureValues[i] - MinFeatureValues[i]) + MinFeatureValues[i];
                FeatureMultipliers[i] = (random.NextDouble() * 1.9) + 0.1;
            }
        }
    }
}

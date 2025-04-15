using AISIGA.Program.IGA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISIGA.Program.AIS
{
    class Antibody
    {
        private int Class { get; set; }
        private double BaseRadius { get; set; }
        private double[] FeatureValues { get; set; }
        private double[] FeatureMultipliers { get; set; }
        private Fitness Fitness { get; set; }

        public Antibody(int assignedClass, double baseRadius, int amountOfFeatures)
        {
            Class = assignedClass;
            BaseRadius = baseRadius;
            FeatureValues = new double[amountOfFeatures];
            FeatureMultipliers = new double[amountOfFeatures];
            Fitness = new Fitness();
        }

        public Antibody(int assignedClass, double baseRadius, double[] featureValues, double[] featureMultipliers, Fitness fitness) 
        {
            Class = assignedClass;
            BaseRadius = baseRadius;
            FeatureValues = featureValues;
            FeatureMultipliers = featureMultipliers;
            Fitness = fitness;
        }   

        public int GetClass()
        {
            return Class;
        }

        public void SetClass(int value)
        {
            Class = value;
        }

        public double GetBaseRadius()
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

        public int GetLength()
        {
            return FeatureValues.Length;
        }

        public Fitness GetFitness()
        {
            return Fitness;
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

        public void AssingRandomClass()
        {
            this.Class = RandomProvider.GetThreadRandom().Next(0, LabelEncoder.ClassCount);
        }

        public double GetFeatureValueAt(int index)
        {
            if (index < 0 || index >= FeatureValues.Length)
                throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
            return FeatureValues[index];
        }

        public double GetFeatureMultiplierAt(int index)
        {
            if (index < 0 || index >= FeatureMultipliers.Length)
                throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
            return FeatureMultipliers[index];
        }
    }
}

﻿using AISIGA.Program.IGA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;

namespace AISIGA.Program.AIS
{
    class Antibody
    {
        private int Class { get; set; }
        private double BaseRadius { get; set; }
        private double[] FeatureValues { get; set; }
        private double[] FeatureMultipliers { get; set; }
        private int[] FeatureDimTypes { get; set; }
        private Fitness Fitness { get; set; }

        public Antibody(int assignedClass, double baseRadius, int amountOfFeatures)
        {
            Class = assignedClass;
            BaseRadius = baseRadius;
            FeatureValues = new double[amountOfFeatures];
            FeatureMultipliers = new double[amountOfFeatures];
            FeatureDimTypes = new int[amountOfFeatures];
            Fitness = new Fitness();
        }

        public Antibody(int assignedClass, double baseRadius, double[] featureValues, double[] featureMultipliers, int[] featureDimTypes, Fitness fitness, bool IsCalculationStillValid) 
        {
            Class = assignedClass;
            BaseRadius = baseRadius;

            // Deep copy the arrays
            FeatureValues = (double[])featureValues.Clone();
            FeatureMultipliers = (double[])featureMultipliers.Clone();
            FeatureDimTypes = (int[])featureDimTypes.Clone();

            // Deep copy the fitness object (make sure Fitness has a copy constructor!)
            Fitness = new Fitness(fitness, IsCalculationStillValid);
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

        public void SetBaseRadius(double value)
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

        public int[] GetFeatureDimTypes()
        {
            return FeatureDimTypes;
        }

        public void SetFeatureDimTypes(int[] values)
        {
            FeatureDimTypes = values;
        }

        public int GetLength()
        {
            return FeatureValues.Length;
        }

        public Fitness GetFitness()
        {
            return Fitness;
        }


        public void AssignRandomFeatureValuesAndMultipliers(List<double[]> ClassMaxFeatureValues, List<double[]> ClassMinFeatureValues, bool useHyperSpheres, bool useUnbounded, double UnboundedChance, bool useUBRatioLock)
        {
            //cance of an ab to be unbounded, prevent everything from being unbounded
            bool IsThisABUnbounded = false;
            if(useUnbounded)
            {
                IsThisABUnbounded = (RandomProvider.GetThreadRandom().NextDouble() < UnboundedChance);
            }
            for (int i = 0; i < FeatureValues.Length; i++)
            {
                double[] MaxFeatureValues = ClassMaxFeatureValues[Class];
                double[] MinFeatureValues = ClassMinFeatureValues[Class];
                FeatureValues[i] = RandomProvider.GetThreadRandom().NextDouble() * (MaxFeatureValues[i] - MinFeatureValues[i]) + MinFeatureValues[i];
                if (useHyperSpheres)
                {
                    FeatureMultipliers[i] = 1.0;
                }
                else
                {
                    FeatureMultipliers[i] = (RandomProvider.GetThreadRandom().NextDouble() * 1.9) + 0.1;
                }
                if (IsThisABUnbounded && !useUBRatioLock)
                {
                    FeatureDimTypes[i] = RandomProvider.GetThreadRandom().Next(1, 3);
                    
                }
                else
                {
                    FeatureDimTypes[i] = 0;
                }
            }
            if (IsThisABUnbounded && useUBRatioLock)
            {
                int amountOfUBInAB = (int)(UnboundedChance * FeatureValues.Length);
                if (amountOfUBInAB == 0)
                {
                    amountOfUBInAB = 1;
                }
                int[] allIndices = Enumerable.Range(0, FeatureValues.Length).ToArray();
                allIndices = allIndices.OrderBy(_ => RandomProvider.GetThreadRandom().Next()).ToArray();

                for (int i = 0; i < amountOfUBInAB; i++) 
                {
                    FeatureDimTypes[allIndices[i]] = RandomProvider.GetThreadRandom().Next(1, 3);
                }

            }
        }

        public void AssingClass(int selectedClass)
        {
            this.Class = selectedClass;
        }
        public void AssingRadius(double baseRadius)
        {
            this.BaseRadius = baseRadius * RandomProvider.GetThreadRandom().NextDouble() * 1.9 + 0.1;
        }
        public void AssingClassAndRadius(double baseRadius, int selectedClass)
        {
            this.Class = selectedClass;
            this.BaseRadius =  baseRadius * RandomProvider.GetThreadRandom().NextDouble() * 1.9 + 0.1;
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

        public double GetFeatureDimTypeAt(int index)
        {
            if (index < 0 || index >= FeatureDimTypes.Length)
                throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
            return FeatureDimTypes[index];
        }
    }
}

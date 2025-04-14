using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISIGA.Program.AIS
{
    class Antigen
    {
        private int AssingedClass { get; set; }
        private int ActualClass { get; set; }
        private double[] FeatureValues { get; set; }

        public Antigen(int assignedClass, int actualClass, int amountOfFeatures)
        {
            AssingedClass = assignedClass;
            ActualClass = actualClass;
            FeatureValues = new double[amountOfFeatures];
        }

        public int GetAssignedClass()
        {
            return AssingedClass;
        }

        public int GetActualClass()
        {
            return ActualClass;
        }


        public double[] GetFeatureValues()
        {
            return FeatureValues;
        }

        public void SetFeatureValues(double[] values)
        {
            FeatureValues = values;
        }

        public void AssingFeatureValue(int featureIndex, int value)
        {
            FeatureValues[featureIndex] = value;
        }

        public int GetLength()
        {
            return FeatureValues.Length;
        }

        public double GetFeatureValueAt(int index)
        {
            if (index < 0 || index >= FeatureValues.Length)
                throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
            return FeatureValues[index];
        }
    }
}

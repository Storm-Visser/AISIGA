using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISIGA.Program.AIS
{
    class Antigen
    {
        private int AssingedClass;
        private int ActualClass;
        private int BaseRadius;
        private int[] FeatureValues;

        public Antigen(int assignedClass, int actualClass, int baseRadius, int amountOfFeatures)
        {
            AssingedClass = assignedClass;
            ActualClass = actualClass;
            BaseRadius = baseRadius;
            FeatureValues = new int[amountOfFeatures];
        }

        public int GetAssignedClass()
        {
            return AssingedClass;
        }

        public int GetActualClass()
        {
            return ActualClass;
        }

        public int GetBaseRadius()
        {
            return BaseRadius;
        }

        public int[] GetFeatureValues()
        {
            return FeatureValues;
        }

        public void AssingFeatureValue(int featureIndex, int value)
        {
            FeatureValues[featureIndex] = value;
        }
    }
}

using AISIGA.Program.AIS;
using AISIGA.Program.Experiments;
using AISIGA.Program.IGA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISIGA.Program.Tests.MethodTests
{
    static class MutationTest
    {
        public static void TestMutation() 
        {
            AbstractExperimentConfig config = new TestConfig();
            LabelEncoder.Encode("a");
            LabelEncoder.Encode("b");
            LabelEncoder.Encode("c");
            Antibody testAB = new Antibody(0,1,3);
            testAB.AssignRandomFeatureValuesAndMultipliers([2.0, 2.0, 2.0], [-2.0, -2.0, -2.0], config.UseHyperSpheres, config.UseUnboundedRegions, config.RateOfUnboundedRegions);
            testAB.AssingRandomClassAndRadius(config.BaseRadius, 1);
            List<Antigen> allAntigens = new List<Antigen>();

            System.Diagnostics.Debug.WriteLine("TestAB Before: ");
            System.Diagnostics.Debug.WriteLine($"Class: {testAB.GetClass()}, BaseR: {testAB.GetBaseRadius()}, " +
                $"FV; [{testAB.GetFeatureValues()[0]}, {testAB.GetFeatureValues()[1]}, {testAB.GetFeatureValues()[2]}], " +
                $"FM; [{testAB.GetFeatureMultipliers()[0]}, {testAB.GetFeatureMultipliers()[1]}, {testAB.GetFeatureMultipliers()[2]}]");

            EVOFunctions.Config = config;
            EVOFunctions.MutateAntibody(testAB, allAntigens);

            System.Diagnostics.Debug.WriteLine("TestAB After: ");
            System.Diagnostics.Debug.WriteLine($"Class: {testAB.GetClass()}, BaseR: {testAB.GetBaseRadius()}, " +
                $"FV; [{testAB.GetFeatureValues()[0]}, {testAB.GetFeatureValues()[1]}, {testAB.GetFeatureValues()[2]}], " +
                $"FM; [{testAB.GetFeatureMultipliers()[0]}, {testAB.GetFeatureMultipliers()[1]}, {testAB.GetFeatureMultipliers()[2]}]");
        }
    }
}

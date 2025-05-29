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
    static class CrossoverTest
    {
        public static void TestCrossover()
        {
            AbstractExperimentConfig config = new TestConfig();
            LabelEncoder.Encode("a");
            LabelEncoder.Encode("b");
            LabelEncoder.Encode("c");
            Antibody testABP1 = new Antibody(1, 1, 3);
            Antibody testABP2 = new Antibody(2, 2, 3);

            // Fix: Replace incorrect array initialization syntax with proper List<double[]> initialization
            testABP1.AssignRandomFeatureValuesAndMultipliers(
                new List<double[]> { new double[] { 1.9, 1.9, 1.9 } },
                new List<double[]> { new double[] { 1.0, 1.0, 1.0 } },
                config.UseHyperSpheres,
                config.UseUnboundedRegions,
                config.RateOfUnboundedRegions,
                config.UseUnboundedRatioLocking
            );

            testABP2.AssignRandomFeatureValuesAndMultipliers(
                new List<double[]> { new double[] { 2.9, 2.9, 2.9 } },
                new List<double[]> { new double[] { 2.0, 2.0, 2.0 } },
                config.UseHyperSpheres,
                config.UseUnboundedRegions,
                config.RateOfUnboundedRegions,
                config.UseUnboundedRatioLocking
            );

            System.Diagnostics.Debug.WriteLine("TestAB parents: ");
            System.Diagnostics.Debug.WriteLine($"1; Class: {testABP1.GetClass()}, BaseR: {testABP1.GetBaseRadius()}, " +
                $"FV; [{testABP1.GetFeatureValues()[0]}, {testABP1.GetFeatureValues()[1]}, {testABP1.GetFeatureValues()[2]}], " +
                $"FM; [{testABP1.GetFeatureMultipliers()[0]}, {testABP1.GetFeatureMultipliers()[1]}, {testABP1.GetFeatureMultipliers()[2]}]");
            System.Diagnostics.Debug.WriteLine($"2; Class: {testABP2.GetClass()}, BaseR: {testABP2.GetBaseRadius()}, " +
                $"FV; [{testABP2.GetFeatureValues()[0]}, {testABP2.GetFeatureValues()[1]}, {testABP2.GetFeatureValues()[2]}], " +
                $"FM; [{testABP2.GetFeatureMultipliers()[0]}, {testABP2.GetFeatureMultipliers()[1]}, {testABP2.GetFeatureMultipliers()[2]}]");

            EVOFunctions.Config = config;
            (Antibody testABC1, Antibody testABC2) = EVOFunctions.CrossoverAntibodies(testABP1, testABP2);

            System.Diagnostics.Debug.WriteLine("TestAB children: ");
            System.Diagnostics.Debug.WriteLine($"1; Class: {testABC1.GetClass()}, BaseR: {testABC1.GetBaseRadius()}, " +
                $"FV; [{testABC1.GetFeatureValues()[0]}, {testABC1.GetFeatureValues()[1]}, {testABC1.GetFeatureValues()[2]}], " +
                $"FM; [{testABC1.GetFeatureMultipliers()[0]}, {testABC1.GetFeatureMultipliers()[1]}, {testABC1.GetFeatureMultipliers()[2]}]");
            System.Diagnostics.Debug.WriteLine($"2; Class: {testABC2.GetClass()}, BaseR: {testABC2.GetBaseRadius()}, " +
                $"FV; [{testABC2.GetFeatureValues()[0]}, {testABC2.GetFeatureValues()[1]}, {testABC2.GetFeatureValues()[2]}], " +
                $"FM; [{testABC2.GetFeatureMultipliers()[0]}, {testABC2.GetFeatureMultipliers()[1]}, {testABC2.GetFeatureMultipliers()[2]}]");
        }
    }
}

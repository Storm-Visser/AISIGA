using AISIGA.Program.AIS;
using AISIGA.Program.Experiments;
using AISIGA.Program.IGA;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISIGA.Program.Tests.MethodTests
{
    class DetectionOfAGByABTest
    {
        public static void TestAGDetection()
        {
            ExperimentConfig config = new TestConfig();
            FitnessFunctions.Config = config;
            TestHypersphere();
            TestHyperellipsoid();
            TestOneUnboundedRegion();
            TestOneUnboundedRegionOppositeDir();
        }
        private static void TestHypersphere()
        {
            double[] abFeatureValues = new double[] { 5.0, 5.0 };
            double[] abMultipliers = new double[] { 1.0, 1.0 };
            int[] abDimTypes = new int[] { 0, 0 };

            Antibody ab = new Antibody(0, 1.0, abFeatureValues, abMultipliers, abDimTypes, new Fitness(), true);

            // Antigen inside the hypersphere
            Antigen inside = new Antigen(0, 0, 2);
            inside.SetFeatureValues(new double[] { 5.5, 5.5 });

            // Antigen on the edge
            Antigen onEdge = new Antigen(0, 0, 2);
            onEdge.SetFeatureValues(new double[] { 6.0, 5.0 });

            // Antigen outside the hypersphere
            Antigen outside = new Antigen(0, 0, 2);
            outside.SetFeatureValues(new double[] { 7.0, 5.0 });

            // Run the test
            double resultInside = FitnessFunctions.CalcAGtoABDistance(ab, inside);
            double resultOnEdge = FitnessFunctions.CalcAGtoABDistance(ab, onEdge);
            double resultOutside = FitnessFunctions.CalcAGtoABDistance(ab, outside);
            System.Diagnostics.Trace.WriteLine("Hypersphere detection test");
            System.Diagnostics.Trace.WriteLine($"Inside antigen: {resultInside} (should be <= 0)");
            System.Diagnostics.Trace.WriteLine($"On-edge antigen: {resultOnEdge} (should be <= 0)");
            System.Diagnostics.Trace.WriteLine($"Outside antigen: {resultOutside} (should be > 0)");
        }

        private static void TestHyperellipsoid()
        {
            // Create antibody at (5, 5) with squared radius 1^2 = 1
            Antibody ab = new Antibody(
                assignedClass: 0,
                baseRadius: 1.0,
                featureValues: new double[] { 5.0, 5.0 },
                featureMultipliers: new double[] { 0.5, 3 },
                featureDimTypes: new int[] { 0, 0 },
                fitness: new Fitness(),
                IsCalculationStillValid: true
            );

            // Inside: close to center
            Antigen insideAg = new Antigen(0, 0, 2);
            insideAg.SetFeatureValues(new double[] { 5.0, 7.0 });

            // Edge: distance should match the radius
            Antigen edgeAg = new Antigen(0, 0, 2);
            edgeAg.SetFeatureValues(new double[] { 4.5, 5.0 });

            // Outside: a bit further from center
            Antigen outsideAg = new Antigen(0, 0, 2);
            outsideAg.SetFeatureValues(new double[] { 4.0, 5.0 });

            // Evaluate distances
            double inside = FitnessFunctions.CalcAGtoABDistance(ab, insideAg);
            double edge = FitnessFunctions.CalcAGtoABDistance(ab, edgeAg);
            double outside = FitnessFunctions.CalcAGtoABDistance(ab, outsideAg);

            // Output
            System.Diagnostics.Trace.WriteLine("Hyperellipsiod detection test");
            System.Diagnostics.Trace.WriteLine($"Inside antigen: {inside} (should be <= 0)");
            System.Diagnostics.Trace.WriteLine($"Edge antigen: {edge} (should be <= 0)");
            System.Diagnostics.Trace.WriteLine($"Outside antigen: {outside} (should be > 0)");
        }

        private static void TestOneUnboundedRegion()
        {
            // Create antibody at (5, 5)
            Antibody ab = new Antibody(
                assignedClass: 0,
                baseRadius: 1.0,
                featureValues: new double[] { 5.0, 5.0 },
                featureMultipliers: new double[] { 0.5, 3 },
                featureDimTypes: new int[] { 2, 0 },
                fitness: new Fitness(),
                IsCalculationStillValid: true
            );

            // Inside: close to center
            Antigen insideAg = new Antigen(0, 0, 2);
            insideAg.SetFeatureValues(new double[] { 4.8, 6.0 });

            // something far away but inside the unbounded region
            Antigen farInsideAg = new Antigen(0, 0, 2);
            farInsideAg.SetFeatureValues(new double[] { -1000, 15.0 });

            // Outside: a bit further from center
            Antigen outsideAg = new Antigen(0, 0, 2);
            outsideAg.SetFeatureValues(new double[] { 1000, 5.0 });

            // Evaluate distances
            double inside = FitnessFunctions.CalcAGtoABDistance(ab, insideAg);
            double edge = FitnessFunctions.CalcAGtoABDistance(ab, farInsideAg);
            double outside = FitnessFunctions.CalcAGtoABDistance(ab, outsideAg);

            // Output
            System.Diagnostics.Trace.WriteLine("Unbounded detection test");
            System.Diagnostics.Trace.WriteLine($"Inside antigen: {inside} (should be <= 0)");
            System.Diagnostics.Trace.WriteLine($"Far inside antigen: {edge} (should be (big negative nr) <= 0)");
            System.Diagnostics.Trace.WriteLine($"Outside antigen: {outside} (should be > 0)");
        }

        private static void TestOneUnboundedRegionOppositeDir()
        {
            // Create antibody at (5, 5)
            Antibody ab = new Antibody(
                assignedClass: 0,
                baseRadius: 1.0,
                featureValues: new double[] { 5.0, 5.0 },
                featureMultipliers: new double[] { 0.5, 3 },
                featureDimTypes: new int[] { 1, 0 },
                fitness: new Fitness(),
                IsCalculationStillValid: true
            );

            // Inside: close to center
            Antigen insideAg = new Antigen(0, 0, 2);
            insideAg.SetFeatureValues(new double[] { 4.8, 6.0 });

            // something far away but inside the unbounded region
            Antigen farInsideAg = new Antigen(0, 0, 2);
            farInsideAg.SetFeatureValues(new double[] { 1000, 15.0 });

            // Outside: a bit further from center
            Antigen outsideAg = new Antigen(0, 0, 2);
            outsideAg.SetFeatureValues(new double[] { -1000, 5.0 });

            // Evaluate distances
            double inside = FitnessFunctions.CalcAGtoABDistance(ab, insideAg);
            double edge = FitnessFunctions.CalcAGtoABDistance(ab, farInsideAg);
            double outside = FitnessFunctions.CalcAGtoABDistance(ab, outsideAg);

            // Output
            System.Diagnostics.Trace.WriteLine("Unbounded detection test");
            System.Diagnostics.Trace.WriteLine($"Inside antigen: {inside} (should be <= 0)");
            System.Diagnostics.Trace.WriteLine($"Far inside antigen: {edge} (should be (big negative nr) <= 0)");
            System.Diagnostics.Trace.WriteLine($"Outside antigen: {outside} (should be > 0)");
        }
    }
}

using AISIGA.Program.Tests.MethodTests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISIGA.Program.Tests
{
    static class Tests
    {
        public static void Run()
        { 
            CrossoverTest.TestCrossover();
            MutationTest.TestMutation();
            DetectionOfAGByABTest.TestAGDetection();
        }

    }
}

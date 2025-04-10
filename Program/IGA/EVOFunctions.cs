using AISIGA.Program.AIS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISIGA.Program.IGA
{
    static class EVOFunctions
    {
        public static Antibody MutateAntibody(Antibody antibody)
        {
            //todo: Implement mutation logic

            return antibody;
        }

        public static (Antibody Child1, Antibody Child2) CrossoverAntibodies(Antibody Parent1, Antibody Parent2)
        {
            //todo: Implement crossover logic

            return (Parent1, Parent2);
        }
    }
}

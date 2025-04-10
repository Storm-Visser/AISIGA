using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISIGA.Program
{
    static class RandomProvider
    {
        [ThreadStatic]
        private static Random? _threadRandom;

        public static Random GetThreadRandom()
        {
            return _threadRandom ??= new Random(Guid.NewGuid().GetHashCode());
        }

    }
}

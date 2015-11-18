using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    /// <summary>
    /// A "global" Random object
    /// </summary>
    class StaticRandom
    {
        public static Random Random;

        static StaticRandom()
        {
            Random = new Random(0);
        }

        /// <summary>
        /// Sets the seed
        /// </summary>
        /// <param name="seed">the new seed</param>
        public static void SetSeed(int seed)
        {
            Random = new Random(seed);
        }
    }
}

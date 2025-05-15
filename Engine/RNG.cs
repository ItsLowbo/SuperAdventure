using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public static class RNG
    {
        private static Random _generator = new Random();

        public static int RandomNumberBetween(int minimumValue, int maximumValue)
        {
            return _generator.Next(minimumValue, maximumValue + 1);
        }
    }
}

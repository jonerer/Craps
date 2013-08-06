using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Org.BouncyCastle.Security;

namespace Craps
{
    public interface IRandomnessSource
    {
        int NextRandom(int min, int max);
    }

    class LocalComputerRandomness : IRandomnessSource
    {
        public SecureRandom rand { get; set; }

        public LocalComputerRandomness()
        {
            rand = new SecureRandom();
        }

        public override string ToString()
        {
            return "Local computer";
        }

        public int NextRandom(int min, int max)
        {
            return rand.Next(min, max);
        }
    }
}

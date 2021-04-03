using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace UWP.Помошники
{
    struct Point
    {
        public BigInteger X { get; private set; }
        public BigInteger Y { get; private set; }

        public Point(BigInteger x, BigInteger y)
        {
            this.X = x;
            this.Y = y;
        }
    }
}

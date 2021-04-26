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

        public override string ToString()
        {
            return string.Format("({0},{1})", X, Y);
        }

        public static bool operator ==(Point p1,Point p2)
        {
            if (p1.X == p2.X && p1.Y == p2.Y)
                return true;
            return false;
        }

        public static bool operator !=(Point p1,Point p2)
        {
            if (p1.X != p2.X && p1.Y != p2.Y)
                return true;
            return false;
        }
    }
}

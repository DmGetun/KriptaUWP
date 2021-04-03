using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace UWP.Помошники
{
    class Elliptic
    {
        private  BigInteger a = 2;
        private  BigInteger b = 7;
        private  BigInteger p = 11;

        public Elliptic(BigInteger a, BigInteger b, BigInteger p)
        {
            if (!CheckK(a, b, p)) throw new Error("asd");
            this.a = a;
            this.b = b;
            this.p = p;
        }

        private bool CheckK(BigInteger a, BigInteger b, BigInteger p)
        {
            BigInteger value = (4 * BigInteger.Pow(a,3) + 27 * BigInteger.Pow(b,2)) % p;
            if (value == 0) return false;
            return true;
        }

        public Point GetValue(int number, Point point)
        {
            string num = Convert.ToString(number, 2);
            Point rez = new Point(0, 0);
            BigInteger x = point.X;
            BigInteger y = point.Y;

            for (int i = 0; i < num.Length; i++)
            {
                if(i == 0)
                {
                    rez = point;
                    continue;
                }
                int index = Convert.ToInt32(char.GetNumericValue(num[i]));
                if(index == 1)
                {
                    Point dou = DoublePoint(rez); 
                    rez = SumPoint(DoublePoint(rez), point);
                }
                else
                {
                    rez = DoublePoint(rez);
                }
            }

            return rez;
        }

        public Point CheckPoint(Point point)
        {
            BigInteger X = point.X;
            BigInteger Y = point.Y;

            BigInteger right = (BigInteger.Pow(X, 3) + a * X + b) % p;
            if (right < 0) right = p + right;
            BigInteger left = BigInteger.Pow(Y, 2) % p;
            if (left < 0) left = p + left;

            if (right == left) return point;
            throw new Error("Указанная точка G не принадлежит кривой");
        }

        private Point DoublePoint(Point point)
        {
            Point rez = new Point(0, 0);

            int x = (int)point.X;
            int y = (int)point.Y;

            int f = (int)F(p) - 1;

            BigInteger ap = ((3 * (int)Math.Pow(x, 2) + a) * BigInteger.Pow(2 * y, f)) % p;
            if (ap < 0) ap = p + ap;
            BigInteger x3 = (BigInteger.Pow(ap, 2) - (2 * x)) % p;
            if (x3 < 0) x3 = p + x3;
            BigInteger y3 = (ap * (x - x3) - y) % p;
            if (y3 < 0) y3 = p + y3;
            
            return new Point((int)x3,(int)y3);
        }

        private Point SumPoint(Point point1,Point point2)
        {
            Point rez = new Point(0, 0);
            BigInteger x1 = point1.X;
            BigInteger y1 = point1.Y;
            BigInteger x2 = point2.X;
            BigInteger y2 = point2.Y;

            int f = (int)F(p) - 1;

            BigInteger ap = (y2 - y1) * BigInteger.Pow(x2 - x1, f) % p;
            if (ap < 0) ap = p + ap;
            BigInteger x3 = (BigInteger.Pow(ap, 2) - x1 - x2) % p;
            if (x3 < 0) x3 = p + x3;
            BigInteger y3 = (ap * (x1 - x3) - y1) % p;
            if (y3 < 0) y3 = p + y3;

            return new Point((int)x3, (int)y3);
        }

        private BigInteger F(BigInteger n)
        {
            BigInteger result = n;
            for (BigInteger i = 2; i * i <= n; ++i)
                if (n % i == 0)
                {
                    while (n % i == 0)
                        n /= i;
                    result -= result / i;
                }
            if (n > 1)
                result -= result / n;
            return result;
        }
    }
}

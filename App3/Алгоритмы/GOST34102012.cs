using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using UWP.Помошники;

namespace UWP.Алгоритмы
{
    class GOST34102012 : Algorithm
    {
        private BigInteger p,q;

        public override string Name => "ГОСТ Р 34.10-2012";

        public override string DefaultKey => "a=2\rb=7\rp=11\rXa=6\rG=(10,9)";

        public override bool IsReplaceText => true;

        public int Xa { get; private set; }
        public Point Yu { get; private set; }

        public override string CheckKey(string key)
        {
            throw new NotImplementedException();
        }

        public override string Decrypt(string cipherText, Config config)
        {
            string text = cipherText.Split(',')[0];
            BigInteger[] numbers = ParseText(cipherText);
            BigInteger r = numbers[0];
            BigInteger s = numbers[1];
            int[] keys = ParseKey(config.Key);
            int a = keys[0];
            int b = keys[1];
            int p = keys[2];
            int Xa = keys[3];
            BigInteger x_ = new BigInteger(keys[4]);
            BigInteger y_ = new BigInteger(keys[5]);
            Elliptic elliptic = new Elliptic(a, b, p);
            Point G = elliptic.CheckPoint(new Point(x_, y_));
            BigInteger q = elliptic.Calculate_Q();

            Yu = elliptic.GetValue(Xa, G);
            BigInteger hash = GetHashMessage(text,p);
            if (!(0 < r || s < q))
                return "Подпись не верна.";

            BigInteger f = F(q) - 1;

            BigInteger u1 = s * Pow(hash, f, q) % q;
            if (u1 < 0) u1 += q;
            BigInteger u2 = -r * Pow(hash, f, q) % q;
            if (u2 < 0) u2 += q;

            Point P = elliptic.SumPoint(elliptic.GetValue((int)u1, G), elliptic.GetValue((int)u2, Yu));
            BigInteger res = P.X % q;
            if (res < 0) res += q;
            if (res == r)
                return "Подпись верна.";
            return "Подпись не верна.";
        }

        public override string Encrypt(string plainText, Config config)
        {
            int[] keys = ParseKey(config.Key);
            var alf = Alphabet.GenerateAlphabet();

            BigInteger a = keys[0];
            BigInteger b = keys[1];
            BigInteger p = keys[2];
            Xa = keys[3];
            Elliptic elliptic = new Elliptic(a, b, p);
            Point G = elliptic.CheckPoint(new Point(keys[4], keys[5]));
            BigInteger q = elliptic.Calculate_Q();

            BigInteger hash = GetHashMessage(plainText, p);

            int k = 3;
            Point P = new Point();
            BigInteger r = 0;
            BigInteger s = 0;
            do
            {
                k = new Random().Next(1, (int)q);
                P = elliptic.GetValue(k, G);
                r = P.X % q;
                if (r < 0) r += q;
                s = (k * hash + r * Xa) % q;
                if (s < 0) s += q;
            }
            while (r == 0 || s == 0);

            return $"{plainText},({r},{s})";
        }

        public override string KeyView()
        {
            string one = $"Секретный ключ:{Xa}";
            string two = $"Открытый ключ:({Yu.X},{Yu.Y})";
            return one + two;
        }

        private BigInteger[] ParseText(string cipherText)
        {
            cipherText = cipherText.Substring(cipherText.IndexOf(','));
            cipherText = cipherText.Replace("(", "").Replace(")", "");
            string[] keys = cipherText.Split(',', StringSplitOptions.RemoveEmptyEntries);
            BigInteger number = 0;
            BigInteger[] numbers = new BigInteger[keys.Length];
            for (int i = 0; i < keys.Length; i++)
            {
                if (BigInteger.TryParse(keys[i], out number))
                    numbers[i] = number;
                else throw new Error(Error.InvalidValueKey);
            }
            return numbers;
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

        private BigInteger Pow(BigInteger x, BigInteger p, BigInteger m)
        {
            BigInteger r = 1;
            x %= m;
            for (int i = 1; i <= p; i++)
            {
                r = (r * x) % m;
            }

            return r;
        }

        private BigInteger GetHashMessage(string plainText, BigInteger p)
        {
            var alf = Alphabet.GenerateAlphabet();
            int h = 0;
            foreach (char s in plainText)
            {
                int index = Alphabet.GetSymbol(alf, s);
                h = (int)((Math.Pow((h + index), 2)) % (int)p);
            }

            return h == 0 ? h + 1 : h;
        }

        private int[] ParseKey(string key)
        {
            string[] keys = key.Split('\r');
            int[] numbers = new int[keys.Length + 1];
            int number = 0;
            for (int i = 0; i < keys.Length - 1; i++)
            {
                if (int.TryParse(keys[i].Substring(keys[i].IndexOf('=') + 1), out number))
                {
                    numbers[i] = number;
                }
                else throw new Error(Error.InvalidValueKey);
            }

            string temp = keys[keys.Length - 1].Replace("(", "").Replace(")", "");
            string[] pt = temp.Substring(temp.IndexOf("=") + 1).Split(",");
            int X, Y;
            if (!int.TryParse(pt[0], out X))
                throw new Error(Error.InvalidValueKey);
            if (!int.TryParse(pt[1], out Y))
                throw new Error(Error.InvalidValueKey);
            numbers[numbers.Length - 2] = X;
            numbers[numbers.Length - 1] = Y;

            return numbers;
        }

        public override string GenerateKey()
        {
            throw new NotImplementedException();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using UWP.Алгоритмы;
using UWP.Помошники;

namespace UWP.Алгоритмы
{
    class GOST341094 : Algorithm
    {
        private BigInteger p,q,x,a;

        public override string Name => "ГОСТ Р 34.10-94";

        public override string DefaultKey => "P=31\rQ=5\rx=4";

        public override bool IsReplaceText => true;

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
            BigInteger[] keys = ParseKey(config.Key);
            p = keys[0];
            q = keys[1];
            //a = Calculate_a(p, q);
            BigInteger y = Pow(a, x, p);
            BigInteger h = GetHashMessage(text,q);
            BigInteger v = Pow(h, q - 2, q);
            BigInteger z1 = (s * v) % q;
            BigInteger z2 = ((q - r) * v) % q;
            BigInteger u = ((BigInteger.Pow(a, (int)z1) * BigInteger.Pow(y, (int)z2)) % p) % q;
            if (u == r)
                return "Подпись верна";
            return "Подпись не верна";
        }

        private BigInteger[] ParseText(string cipherText)
        {
            cipherText = cipherText.Substring(cipherText.IndexOf(','));
            cipherText = cipherText.Replace("(", "").Replace(")", "");
            string[] keys = cipherText.Split(',',StringSplitOptions.RemoveEmptyEntries);
            BigInteger number = 0;
            BigInteger[] numbers = new BigInteger[keys.Length];
            for(int i = 0;i < keys.Length; i++)
            {
                if (BigInteger.TryParse(keys[i], out number))
                    numbers[i] = number;
                else throw new Error(Error.InvalidValueKey);
            }
            return numbers;
        }

        public override string Encrypt(string plainText, Config config)
        {
            BigInteger[] keys = ParseKey(config.Key);
            p = keys[0];
            q = keys[1];
            x = keys[2];
            //a = Calculate_a(p,q);
            a = 8;
            //BigInteger k = Calculate_k(q);
            BigInteger k = 2;
            BigInteger h = GetHashMessage(plainText, q);
            BigInteger r = Pow(a, k, p) % q;
            BigInteger s = (x * r + k * h) % q;

            return $"{plainText},({r},{s})";
        }

        private BigInteger Calculate_k(BigInteger q)
        {
            return new Random().Next(1, (int)q);
        }

        private BigInteger Calculate_a(BigInteger p, BigInteger q)
        {
            Random rand = new Random();
            BigInteger f;
            do
            {
                BigInteger d = rand.Next(2, (int)p - 1);
                f = Pow(d, (p - 1) / q, p);
            }
            while (f == 1);

            return f;
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

        private BigInteger[] ParseKey(string key)
        {
            string[] keys = key.Split('\r');
            BigInteger[] result = new BigInteger[keys.Length];
            for(int i = 0; i < keys.Length; i++)
            {
                BigInteger number = 0;
                if (BigInteger.TryParse(keys[i].Substring(keys[i].IndexOf('=') + 1), out number))
                    result[i] = number;
                else throw new Error(Error.InvalidValueKey);
            }
            return result;
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

        public override string GenerateKey()
        {
            throw new NotImplementedException();
        }

        public override string KeyView()
        {
            string one = $"Открытые ключи:p={p},q={q},a={a}\r";
            string two = $"Секретный ключ:x={x}";
            return one + two;
        }
    }
}

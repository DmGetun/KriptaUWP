using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using UWP.Помошники;

namespace UWP.Алгоритмы
{
    class RSACP : Algorithm
    {
        public override string Name => "ЦП RSA";

        public override string DefaultKey => "p=3\rq=7";

        public override bool IsReplaceText => true;

        BigInteger N, E;

        public override string CheckKey(string key)
        {
            throw new NotImplementedException();
        }

        public override string Decrypt(string cipherText, Config config)
        {
            string[] textN = ParseText(cipherText);
            string message = textN[0];
            BigInteger S = BigInteger.Parse(textN[1]);

            BigInteger f = F(N) - 1;

            BigInteger hash = GetHashMessage(message, N);

            BigInteger m = Pow(S, E, N);

            if (m == hash)
                return string.Format("{0} == {1}.Подпись верна",hash,m);
            return "Что-то пошло не так...";
        }

        private string[] ParseText(string cipherText)
        {
            string[] temp = cipherText.Split('\r');
            string values = temp[1].Trim();
            string[] parse = values.Split(',');
            string message = parse[0];
            string cp = parse[1];
            return new string[] { message, cp };
        }

        public override string Encrypt(string plainText, Config config)
        {
            BigInteger[] keys = ParseKey(config.Key);
            BigInteger p = keys[0];
            BigInteger q = keys[1];

            N = p * q;

            BigInteger f = (p - 1) * (q - 1);

            E = GetMutually(f);

            BigInteger D = GetD(N,E, f);

            BigInteger hash = GetHashMessage(plainText,N);

            BigInteger S = Pow(hash, D, N);

            return string.Format("Получателю отправляется:\r {0},{1}", plainText, S);
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

        private BigInteger GetHashMessage(string plainText,BigInteger p)
        {
            var alf = Alphabet.GenerateAlphabet();
            int h = 0;
            foreach(char s in plainText)
            {
                int index = Alphabet.GetSymbol(alf, s);
                h = (int)((Math.Pow((h + index), 2)) % (int)p);
            }
            return h;
        }

        private BigInteger GetD(BigInteger n, BigInteger e, BigInteger f)
        {
            Random rand = new Random();
            BigInteger value = rand.Next((int)n);
            while ((e * value) % f != 1)
                value = rand.Next((int)n);
            return value;
        }

        private BigInteger GetMutually(BigInteger f)
        {
            Random rand = new Random();
            BigInteger value = rand.Next() % f;
            while (!IsMutuAllySimple(value, f))
            {
                value = rand.Next() % f;
            }

            return value;
        }

        private BigInteger[] ParseKey(string key)
        {
            string[] keys = key.Split('\r');
            BigInteger[] numbers = new BigInteger[keys.Length];

            for(int i = 0; i < keys.Length; i++)
            {
                BigInteger number = 0;
                if (!BigInteger.TryParse(keys[i].Substring(keys[i].IndexOf("=") + 1), out number))
                    throw new Error(Error.InvalidValueKey);
                numbers[i] = number;
            }
            return numbers;
        }

        public override string GenerateKey()
        {
            throw new NotImplementedException();
        }

        public override string KeyView()
        {
            return "";
        }

        private bool IsMutuAllySimple(BigInteger a, BigInteger b)
        {
            return NOD(a, b) == 1;
        }

        public static BigInteger NOD(BigInteger a, BigInteger b)
        {
            while (b != 0)
            {
                var temp = b;
                b = a % b;
                a = temp;
            }
            return a;
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

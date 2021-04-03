using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using UWP.Помошники;

namespace UWP.Алгоритмы
{
    class ECC : Algorithm
    {
        public override string Name => "Шифр ЕСС";

        public override string DefaultKey => "a=2\rb=7\rp=11\rk=4\rCu=6\rG=(10,9)";

        public string Key { get; private set; }

        public override bool IsReplaceText => false;

        public override string CheckKey(string key)
        {
            throw new NotImplementedException();
        }

        private int[] ParseKey(string key)
        {
            string[] keys = key.Split('\r');
            int[] numbers = new int[keys.Length + 1];
            int number = 0;
            for(int i = 0; i < keys.Length - 1; i++)
            {
                if (int.TryParse(keys[i].Substring(keys[i].IndexOf('=') + 1), out number))
                {
                    numbers[i] = number;
                }
                else throw new Error(Error.InvalidValueKey);
            }

            string temp = keys[keys.Length - 1].Replace("(","").Replace(")","");
            string[] pt = temp.Substring(temp.IndexOf("=") + 1).Split(",");
            int X = int.Parse(pt[0]);
            int Y = int.Parse(pt[1]);
            numbers[numbers.Length - 2] = X;
            numbers[numbers.Length - 1] = Y;

            return numbers;
        }

        public override string Decrypt(string cipherText, Config config)
        {
            int[] keys = ParseKey(config.Key);

            int[] values = ParseText(cipherText);
            int e = values[2];

            BigInteger a = keys[0];
            BigInteger b = keys[1];
            BigInteger p = keys[2];
            BigInteger k = keys[3];
            int Cu = keys[4];

            Elliptic elliptic = new Elliptic(a, b, p);
            Point R = elliptic.CheckPoint(new Point(values[0], values[1]));

            int f = (int)F(p) - 1;

            Point Q = elliptic.GetValue(Cu, R);

            BigInteger M = (e * BigInteger.Pow(Q.X,f)) % p;
           
            return string.Format("Расшифрованная подпись: {0}", M);
        }

        private int[] ParseText(string cipherText)
        {
            cipherText = cipherText.Substring(cipherText.IndexOf(":") + 1);
            cipherText = cipherText.Replace("(", "").Replace(")", "");
            string[] rez = cipherText.Split(",");
            int[] numbers = new int[rez.Length];
            for(int i = 0; i < rez.Length; i++)
            {
                int num = 0;
                if (!int.TryParse(rez[i], out num)) throw new Error("Ошибка перевода подписи");
                numbers[i] = num;
            }

            return numbers;
        }

        public override string Encrypt(string plainText, Config config)
        {
            int[] keys = ParseKey(config.Key);
            Key = config.Key;
            int m = 0;

            if (!int.TryParse(plainText, out m)) throw new Error("Ошибка: шифровать нужно число");

            BigInteger a = keys[0];
            BigInteger b = keys[1];
            BigInteger p = keys[2];
            int k = keys[3];
            int Cu = keys[4];

            Elliptic elliptic = new Elliptic(a, b, p);
            Point G = elliptic.CheckPoint(new Point(keys[5], keys[6]));

            Point Du = elliptic.GetValue(Cu, G);

            Point R = elliptic.GetValue(k, G);
            Point P = elliptic.GetValue(k, Du);

            BigInteger e = (m * P.X) % p;

            return string.Format("Пользователю будет отправлено:(({0},{1}),{2})",R.X,R.Y,e);
        }

        public override string GenerateKey()
        {
            throw new NotImplementedException();
        }

        public override string KeyView()
        {
            return Key.Replace("\r"," ");
        }

        private static BigInteger F(BigInteger n)
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

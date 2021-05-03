using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using UWP.Помошники;

namespace UWP.Алгоритмы
{
    class RSA : Algorithm
    {
        public override string Name => "Шифр RSA";

        public override string DefaultKey => "P=123\rQ=677";

        public override bool IsReplaceText => true;

        public BigInteger N { get; private set; }

        private BigInteger P = 123, Q = 677;
        private BigInteger e;

        public override string CheckKey(string key)
        {
            string[] keys = key.Split('\r');
            long P = Convert.ToInt64(keys[0]);
            long Q = Convert.ToInt64(keys[1]);

            if (!IsTheNumberSimple(P))
                throw new Error("Число P должно быть простым");
            if (!IsTheNumberSimple(Q))
                throw new Error("Число Q должно быть простым");

            return key;
        }

        private void ParseKey(string key)
        {
            key = CheckKey(key);
            string[] keys = key.Split('\r');
            BigInteger[] numbers = new BigInteger[keys.Length];
            int i = 0;
            foreach(string k in keys)
            {
                numbers[i++] = BigInteger.Parse(k.Substring(k.IndexOf('=') + 1));
            }
            P = numbers[0];
            Q = numbers[1];
        }
        /*
            Функция расшифрования.
            Вычисляем функцию Эйлера от N,
            вычисляем d.
            Получаем из шифртекста число,
            возводим это число в степень d по модулю N.
        */
        public override string Decrypt(string cipherText, Config config)
        {
            List<string> text = cipherText.Split(' ').ToList<string>();
            StringBuilder rez = new StringBuilder();
            var alf = Alphabet.GenerateAlphabet();
            int len = text.Count;
            char[] mass = new char[len];
            string key = config.Key;
            ParseKey(key);
            BigInteger f = F(N);
            BigInteger d = Calculate_d(f);
            Parallel.For(0,len, i=>
            {
                 var bi = BigInteger.Parse(text[i]);
                 
                 bi = Pow(bi, d, N);

                 int index = Convert.ToInt32(bi.ToString());
                 mass[i] = alf[index];
            });

            return new string(mass);
        }

        private BigInteger Pow(BigInteger x, BigInteger p, BigInteger m)
        {
            BigInteger r = 1;
            x %= m;
            for(int i = 1;i <= p; i++)
            {
                r = (r * x) % m;
            }

            return r;
        }
        /*
            Функция шифрования.
            Вычисляем N,f,d,e
            Получаем индекс символа в алфавите,
            возводим индекс в степень e по модулю N
        */
        public override string Encrypt(string plainText, Config config)
        {
            string alf = new string (Alphabet.GenerateAlphabet().Values.ToArray());
            List<string> rez = new List<string>();
            string key = config.Key;
            ParseKey(key);
            BigInteger[] numbers = new BigInteger[plainText.Length];
            N = P * Q;
            BigInteger f = (P - 1) * (Q - 1);

            BigInteger d = Calculate_d(f);
            e = Calculate_e(d,f);

            int len = plainText.Length;

            Parallel.For(0, len, i =>
            {
                 int index = alf.IndexOf(plainText[i]) + 1;
                 BigInteger number = (long)index;

                 number = Pow(number, e, N);
                 numbers[i] = number;
            });

            foreach(int number in numbers)
                rez.Add(number.ToString());

            return string.Join(' ', rez);

        }

        public override string KeyView()
        {
            return "";
        }
        // проверка на простоту
        private bool IsTheNumberSimple(long n)
        {
            if (n < 2)
                return false;

            if (n == 2)
                return true;

            for (long i = 2; i < n; i++)
                if (n % i == 0)
                    return false;

            return true;
        }
        // вычисление функции Эйлера
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
        // вычисление d
        private BigInteger Calculate_d(BigInteger m)
        {
            BigInteger d = m - 1;

            for (BigInteger i = 2; i <= m; i++)
                if ((m % i == 0) && (d % i == 0)) //если имеют общие делители
                {
                    d--;
                    i = 1;
                }

            return d;
        }
        // вычисление e
        private BigInteger Calculate_e(BigInteger d, BigInteger m)
        {
            BigInteger e = 10;

            while (true)
            {
                if ((e * d) % m == 1)
                    break;
                else
                    e++;
            }

            return e;
        }

        public override string GenerateKey()
        {
            Random rand = new Random();
            do
            {
                P = rand.Next(1, 500);
            }
            while (!IsTheNumberSimple((long)P));
            do
            {
                Q = rand.Next(1, 500);
            }
            while (!IsTheNumberSimple((long)Q));

            return $"P={P}\rQ={Q}";
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using UWP.Помошники;

namespace UWP.Алгоритмы
{
    class Elgamal : Algorithm
    {
        private BigInteger p,g,x;

        public override string Name => "Шифр Elgamal";

        public override string DefaultKey => "p=71\rg=2\rx=30";

        public override bool IsReplaceText => true;

        public override string CheckKey(string key)
        {
            string[] keys = key.Split('\r');
            int[] numbers = new int[keys.Length];
            StringBuilder str = new StringBuilder();

            for(int i = 0; i < keys.Length; i++)
            {
                string k = keys[i].Trim();
                string number = k.Substring(k.IndexOf('=') + 1);
                str.Append(number + " ");
            }

            return str.ToString();
        }

        private BigInteger[] TransformKey(string key)
        {
            string[] keys = key.Trim().Split(" ");
            return keys.Select(x => BigInteger.Parse(x)).ToArray();
        }

        /*
            Функция расшифрования.
            Разбиваем строку на массив из a,b
            записываем их в свои массивы.
            Получаем индекс открытого символа,
            умножая b на (a^(-1))^x по модулю p.
        */
        public override string Decrypt(string cipherText, Config config)
        {
            var keys = TransformKey(CheckKey(config.Key));
            var alf = Alphabet.GenerateAlphabet();
            int[] textNumbers = GetNumbersText(cipherText);
            int len = textNumbers.Length;
            int[] coef = new int[len / 2];
            int[] numbers = new int[len / 2];
            BigInteger p = keys[0];
            BigInteger x = keys[2];
            for (int i = 0,j = 0; i < len; i+= 2,j++)
            {
                coef[j] = textNumbers[i];
                numbers[j] = textNumbers[i+1];
            }
            char[] plain = new char[numbers.Length];
            Parallel.For(0, coef.Length, i =>
              {
                  BigInteger a = coef[i];
                  BigInteger b = numbers[i];

                  BigInteger M = mul(b, Pow(a, p - 1 - x, p), p);
                  plain[i] = alf[(int)M];
              });
            return new string(plain);
        }
        /*
            Разбить зашифрованную строку на массив из коэффициентов a,b 
        */
        private int[] GetNumbersText(string cipherText)
        {
            string[] value = cipherText.Trim().Split(" ");
            int[] numbers = new int[value.Length * 2];
            int j = 0;
            for(int i = 0; i < value.Length; i++)
            {
                string temp = value[i].Replace("(", "").Replace(")", "");
                string[] values = temp.Split(",");
                numbers[j++] = int.Parse(values[0]);
                numbers[j++] = int.Parse(values[1]);
            }

            return numbers;
        }
        /*
            Функция расшифрования.
            Вычисляем y, возведя g в степень x по модулю p.
            Получаем индекс символа в алфавите,
            Генерируем рандомизатор, взаимно простой с функцией Эйлера,
            вычисляем a, возведя g в степень k по модулю p
            Вычисляем b умножая y в степени k на индекс по модулю p
        */
        public override string Encrypt(string plainText, Config config)
        {
            var keys = TransformKey(CheckKey(config.Key));
            var alf = Alphabet.GenerateAlphabet();

            p = keys[0];
            if (!IsTheNumberSimple(p)) return "Число p не простое.";

            g = keys[1];
            x = keys[2];
            BigInteger y = Pow(g, x, p);
            BigInteger[] numbers = new BigInteger[plainText.Length];
            BigInteger[] coef = new BigInteger[plainText.Length];
            var rand = new Random();
            Parallel.For(0, plainText.Length, i =>
            {
                int m = Alphabet.GetSymbol(alf, plainText[i]);
                BigInteger k = rand.Next() % (p - 2) - 1;
                BigInteger a = Pow(g, k, p);
                BigInteger b = mul(Pow(y, k, p), m, p);
                coef[i] = a;
                numbers[i] = b;
            });

            return FormKey(numbers,coef);
        }
        // формируем ключ из коэффициентов a,b
        private string FormKey(BigInteger[] numbers, BigInteger[] coef)
        {
            StringBuilder rez = new StringBuilder();
            for(int i = 0; i < numbers.Length; i++)
            {
                string str = string.Format("({0},{1}) ", coef[i], numbers[i]);
                rez.Append(str);
            }
            return rez.ToString();
        }

        public override string KeyView()
        {
            return "";
        }
        // проверка на простоту
        private bool IsTheNumberSimple(BigInteger n)
        {
            if (n < 2)
                return false;

            if (n == 2)
                return true;

            for (BigInteger i = 2; i < n; i++)
                if (n % i == 0)
                    return false;

            return true;
        }
        // возведение в степень по модулю
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
        // умножение чисел по модулю
        BigInteger mul(BigInteger a, BigInteger b, BigInteger n)
        {// a*b mod n
            BigInteger sum = 0;

            for (BigInteger i = 0; i < b; i++)
            {
                sum += a;

                if (sum >= n)
                {
                    sum -= n;
                }
            }

            return sum;
        }

        public override string GenerateKey()
        {
            Random rand = new Random();
            do
            {
                p = rand.Next(1, 500);
            } while (!IsTheNumberSimple(p));

            x = rand.Next(2, (int)p);
            g = rand.Next(2, (int)p);

            return $"p={p}\rg={g}\rx={x}";
        }
    }
}

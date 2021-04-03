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
        private BigInteger g,x,y,p;

        public override string Name => "Шифр Elgamal";

        public override string DefaultKey => "p=71 \rg=2 \rx=30";

        public override bool IsReplaceText => true;

        /*
            Проверка ключа.
            Получаем строковый ключ,
            разбиваем его на части,
            вырезаем числа
        */
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
        /*
            Функция конвертации строковых значений в числа
            Получаем строку из чисел, разбиваем по пробелам.
            Конвертируем строковые значения в числовые и добавляем в массив чисел,
            если конвертация невозможна, возвращаем ошибку.
        */
        private BigInteger[] TransformKey(string key)
        {
            string[] keys = key.Trim().Split(" ");
            BigInteger number = 0;
            BigInteger[] numbers = new BigInteger[keys.Length];
            int i = 0;
            foreach(string k in keys)
            {
                if (!BigInteger.TryParse(k, out number))
                    throw new Error(Error.InvalidValueKey);
                numbers[i++] = number;
            }
            return numbers;
        }
        /*
            Функция расшифрования.
            Получаем ключ и текст, формируем ключи из строки.
            Разбиваем входной текст на пары a и b,
            в цикле вычисляем каждое значение индекса символа открытого текста.
            Для этого умножаем b на a в степени p - 1 - x по модулю p
            Полученное число используем как индекс открытого символа
        */
        public override string Decrypt(string cipherText, Config config)
        {
            var keys = TransformKey(CheckKey(config.Key));
            var alf = Alphabet.GenerateAlphabet();
            int[] textNumbers = GetNumbersText(cipherText);
            int len = textNumbers.Length;
            int[] coef = new int[len / 2];
            int[] numbers = new int[len / 2];
            p = keys[0];
            x = keys[2];
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
            Разбиваем числа (a,b) из строки зашифрованного текста на a и b
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
            Функция шифрования.
            Получаем ключ и текст, формируем ключи из строки.
            Вычисляем y, возведя g в степень x по модулю p.
            В цикле по символам открытого текста:
            получаем индекс символа из алфавита
            Генерируем случайное число k,
            вычисляем число a, возведя g в степерь k по модулю p,
            Вычисляем число b, умножая y в степени k на m по модулю p
            Записывам (a,b) в результирующую строку.
        */
        public override string Encrypt(string plainText, Config config)
        {
            var keys = TransformKey(CheckKey(config.Key));
            var alf = Alphabet.GenerateAlphabet();

            p = keys[0];
            if (!IsTheNumberSimple(p)) return "Число p не простое.";

            g = keys[1];
            x = keys[2];
            y = Pow(g, x, p);
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
        /*
            Функция формирование результтирующей строки.
            Получаем массив чисел и коэффициенты.
            Собираем их в строку (a,b)...
        */
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
            string one = string.Format("Открытые ключи: p={0}, g={1}, y={2}\r", p, g, y);
            string two = string.Format("Секретный ключ: x={0}", x);
            return one + two;
        }
        /*
            Функция проверки числа на простоту.
            Начиная с числа 2 до нашего проверяемого числа,
            делим наше число на каждое следующее с остатком.
            Если остаток 0, значит число не простое.
        */
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

        /*
            Функция возведения числа в степень по модулю 
        */
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

        /*
            Функция умножения двух чисел по модулю n 
        */
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
            throw new NotImplementedException();
        }
    }
}

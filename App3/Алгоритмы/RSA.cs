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

        public override string DefaultKey => "P=113\rQ=677";

        public override bool IsReplaceText => true;

        public BigInteger N { get; private set; }

        private BigInteger P = 113, Q = 677;
        private BigInteger d;
        private BigInteger e;

        /*
            Проверка ключа на формат и простоту чисел 
        */
        public override string CheckKey(string key)
        {
            string[] keys = key.Split('\r');
            BigInteger p, q;
            if (!BigInteger.TryParse(keys[0].Substring(keys[0].IndexOf('=') + 1), out p))
                throw new Error(Error.InvalidValueKey);
            if(!BigInteger.TryParse(keys[0].Substring(keys[0].IndexOf('=') + 1),out q))
                throw new Error(Error.InvalidValueKey);

            if (!IsTheNumberSimple(p))
                throw new Error("Число P должно быть простым");
            if (!IsTheNumberSimple(q))
                throw new Error("Число Q должно быть простым");

            return key;
        }
        /*
            Разбиение ключа на числа P и Q 
        */
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
            Получаем текст и ключ,
            Разбиваем ключ на P и Q.
            Вычисляем их произведение и функцию Эйлера от произведения.
            Вычисляем D.
            Разбиваем зашифрованный текст на числа,
            Возводим число в степень D по модулю N.
            По полученному индексу получаем символ открытого текста.
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

            BigInteger N = P * Q;
            BigInteger f = (P - 1) * (Q - 1);
            d = Calculate_d(e,f);
            Parallel.For(0,len, i=>
            {
                 var bi = BigInteger.Parse(text[i]);
                 
                 bi = Pow(bi, d, N);

                 int index = Convert.ToInt32(bi.ToString());
                 mass[i] = alf[index];
            });

            return new string(mass);
        }
        /*
            Функция возведения числа в степень по модулю 
        */
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
            Получаем текст и ключ,
            Разбиваем ключ на P и Q.
            Вычисляем их произведение и функцию Эйлера от произведения.
            Вычисляем D.
            Вычисляем E.
            Получаем индекс символа открытого текста из алфавита.
            Возводим индекс символа в степень E по модулю N.
            Полученное число добавляем в массив зашифрованных чисел.
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

            e = Calculate_e(f);
            d = Calculate_d(e,f);

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
        /*
            Функция вывода ключа  
        */
        public override string KeyView()
        {
            string one = string.Format("e,n = ({0},{1})\r", e, N);
            string two = string.Format("d,n = ({0},{1})", d, N);
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
            Функция расчета числа E.
            Получаем значение фукнции Эйлера,
            Присваиваем d это значение.
            Начиная с 2 до f ищем общие делители у чисел,
            если они есть, уменьшаем d на 1 и начинаем цикл заново.
        */
        private BigInteger Calculate_e(BigInteger f)
        {
            BigInteger d = f;

            for (BigInteger i = 2; i <= f; i++)
                if ((f % i == 0) && (d % i == 0)) //если имеют общие делители
                {
                    d--;
                    i = 1;
                }

            return d + f;
        }
        /*
            Функция расчета числа D.
            Ищем сравнимое с E по модулю m число.
        */
        private BigInteger Calculate_d(BigInteger e, BigInteger m)
        {
            BigInteger d = 10;

            while (true)
            {
                if ((e * d) % m == 1)
                    break;
                d++;
            }

            return d;
        }

        public override string GenerateKey()
        {
            Random rand = new Random();
            int P = 0, Q = 0;
            do
            {
                P = rand.Next(1, 1000);
            } while (!IsTheNumberSimple(new BigInteger(P)));

            do
            {
                Q = rand.Next(1, 1000);
            } while (!IsTheNumberSimple(new BigInteger(Q)));

            return $"P={P}\rQ={Q}";
        }
    }
}

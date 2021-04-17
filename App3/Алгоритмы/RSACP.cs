using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UWP.Помошники;

namespace UWP.Алгоритмы
{
    class RSACP : Algorithm
    {
        public override string Name => "ЦП RSA";

        public override string DefaultKey => "p=3\rq=7";

        public override bool IsReplaceText => true;

        BigInteger N, E, D;

        public override string CheckKey(string key)
        {
            string pattern = @"\w+,\d+";
            if (!Regex.IsMatch(key, pattern))
                throw new Error("Неверный формат сообщения для получателя");
            return key;
        }
        /*
            Функция проверки подписи.
            Разбиваем текст на сообщение и подпись S.
            Вычисляем функцию Эйлера от N,
            вычисляем хеш сообщения.
            Вычисляем m, возведя S в степень E по модулю N.
        */
        public override string Decrypt(string cipherText, Config config)
        {
            string[] textN = ParseText(CheckKey(cipherText));
            string message = textN[0];
            BigInteger S;
            if (!BigInteger.TryParse(textN[1], out S))
                throw new Error("Неверный формат сообщения для получателя");

            BigInteger f = F(N);
            BigInteger hash = GetHashMessage(message, N);


            BigInteger m = Pow(S, E, N);

            if (m == hash)
                return string.Format("{0} == {1}.Подпись верна",hash,m);
            return "Что-то пошло не так...";
        }
        /*
            Функция получения текста и подписи S.
            разбиваем строку по символу переноса строки, удаляем лишние пробелы.
            Разбиваем строку по запятой.
        */
        private string[] ParseText(string cipherText)
        {
            string[] temp = cipherText.Split('\r');
            string values = temp[1].Trim();
            string[] parse = values.Split(',');
            string message = parse[0];
            string cp = parse[1];
            return new string[] { message, cp };
        }
        /*
            Функция генерации подписи.
            Получаем на вход текст и ключ,
            ключ разбиваем на значения P и Q.
            Находим функцию Эйлера от произведения P и Q.
            Генерируем число E, взаимно простое с f
            Вычисляем D, произведение которого на E должно быть сравнимо с единицой по модулю f.
            Вычисляем хеш сообщения.
            Вычисляем подпись, возведя значение хеша в степень D по модулю N.
        */
        public override string Encrypt(string plainText, Config config)
        {
            BigInteger[] keys = ParseKey(config.Key);
            BigInteger p = keys[0];
            BigInteger q = keys[1];
            if (!IsTheNumberSimple(p)) throw new Error("Число p не простое");
            if (!IsTheNumberSimple(q)) throw new Error("Число q не простое");

            N = p * q;

            BigInteger f = (p - 1) * (q - 1);

            E = GetMutually(f);

            D = GetD(N, E, f);
            BigInteger hash = GetHashMessage(plainText,N);

            BigInteger S = Pow(hash, D, N);

            return string.Format("Получателю отправляется:\r{0},{1}", plainText, S);
        }
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
            Функция свертки.
            В цикле вычисляем значение h по формуле h[i] = (h[i-1] + index)^2 mod p
        */
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
        /*
            Функция получения числа D
            Генерируем случайное число, меньшее чем n.
            делаем это до тех пор, пока
            произведение числа e на полученное число по модулю f не будет равно 1.
        */
        private BigInteger GetD(BigInteger n, BigInteger e, BigInteger f)
        {
            Random rand = new Random();
            BigInteger value = rand.Next((int)n);
            while ((e * value) % f != 1)
                value = rand.Next((int)n);
            return value;
        }
        /*
            Генерируем число, взаимно простое с f.
            Повторяем генерацию до достижения взаимной простоты с f. 
        */
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
        /*
            Функция разбиения ключа на значения.
            Разбиваем строку ключа по символам новой строки,
            вырезаем все числа, конвертируем их в числовые типы.
            Если конвертация не удалась, возвращаем сообщение об ошибке.
        */
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
            string one = $"Открытый ключ: E={E},N={N}\r";
            string two = $"Закрытый ключ: D={D}";
            return one + two;
        }
        /*
            Функция проверки чисел на взаимную простоту. 
        */
        private bool IsMutuAllySimple(BigInteger a, BigInteger b)
        {
            return NOD(a, b) == 1;
        }
        /*
            Функция вычисления НОД двух чисел.
        */
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
        /*
            Функция вычисления функции эйлера.
        */
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

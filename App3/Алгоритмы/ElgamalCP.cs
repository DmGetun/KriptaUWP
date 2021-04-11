using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using UWP.Помошники;

namespace UWP.Алгоритмы
{
    class ElgamalCP : Algorithm
    {
        public override string Name => "ЦП Elgamal";

        public override string DefaultKey => "p=23\rg=12\rx=5";

        public override bool IsReplaceText => true;

        BigInteger p, g, Y,x;

        public override string CheckKey(string key)
        {
            throw new NotImplementedException();
        }
        /*
            Функция расшифрования.
            Разбиваем сообщение (m,a,b) по переменным,
            вычисляем хеш сообщения.
            Вычисляем число A1 и A2.
            Если числа равны, подпись верна. Иначе подпись не верна.
        */
        public override string Decrypt(string cipherText, Config config)
        {
            var keys = ParseText(cipherText);
            string text = keys.Item1;
            BigInteger a = keys.Item2[0];
            BigInteger b = keys.Item2[1];
            
            BigInteger hash = GetHashMessage(text, p);

            BigInteger A1 = (BigInteger.Pow(Y,(int)a) * BigInteger.Pow(a,(int)b)) % p;
            BigInteger A2 = Pow(g, hash, p);
            if (A1 == A2)
                return $"{A1}=={A2}\rПодпись верна";
            return "Подпись не верна";
        }
        /*
            Функция разбиения текста вида (m,a,b)
            Удалить скобки, разбить по запятым.
            Сообщение m записать в кортеж как строку,
            Числа a и b записать как массив чисел.
        */
        private Tuple<string,BigInteger[]> ParseText(string text)
        {
            text = text.Substring(text.IndexOf('\r') + 1).Replace("(","").Replace(")","");
            string[] k = text.Split(',');
            BigInteger[] keys = new BigInteger[k.Length];
            for(int i = 1; i < k.Length; i++)
            {
                BigInteger number = 0;
                if (!BigInteger.TryParse(k[i], out number))
                    throw new Error("asd");
                keys[i] = number;
            }

            return new Tuple<string, BigInteger[]>(k[0],new BigInteger[] { keys[1],keys[2]});
        }
        /*
            Разбить ключ на параметры p,g,x.
            Проверить p на простоту и x.
            Вычислить Y,
            Вычислить функцию f,
            вычислить хеш сообщения.
            Сгенерировать число K, взаимно простое с p-1.
            Посчитать число a и b,
            Отправить пользователю сообщение вида (m,a,b)
        */
        public override string Encrypt(string plainText, Config config)
        {
            BigInteger[] keys = ParseKey(config.Key);
            p = keys[0];
            if (!IsTheNumberSimple((long)p))
                throw new Error("Ошибка: число p должно быть простым");
            g = keys[1];
            x = keys[2];
            if (!(x < p - 1))
                throw new Error("Ошибка: число x должно быть меньше, чем p - 1");

            Y = Pow(g, x, p);
            BigInteger f = F(p - 1) - 1;
            BigInteger m = GetHashMessage(plainText, p);
            BigInteger K = GetMutually(p - 1);
            BigInteger a = Pow(g, K, p);
            BigInteger b = ((m - a * x) * BigInteger.Pow(K, (int)f)) % (p - 1);
            if (b < 0) b = p + b - 1;
            return string.Format("Получателю отправляется:\r({0},{1},{2})", plainText, a, b);
        }
        /*
            Функция разбиения ключа 
        */
        private BigInteger[] ParseKey(string key)
        {
            string[] args = key.Split('\r');
            BigInteger[] keys = new BigInteger[args.Length];
            BigInteger number = 0;
            for (int i = 0; i < args.Length; i++)
            {
                if (!BigInteger.TryParse(args[i].Substring(args[i].IndexOf('=') + 1), out number))
                    throw new Error(Error.InvalidValueKey);
                keys[i] = number;
            }

            return keys;
        }

        public override string GenerateKey()
        {
            throw new NotImplementedException();
        }

        public override string KeyView()
        {
            string one = $"Открытый ключ:Y = {Y}\r";
            string two = $"Секретный ключ:X = {x}";
            return one + two;
        }
        /*
            Проверка числа на простоту. 
        */
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
        /*
            Вычисление функции Эйлера 
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
        /*
            Функция свертки. 
        */
        private BigInteger GetHashMessage(string plainText, BigInteger p)
        {
            var alf = Alphabet.GenerateAlphabet();
            int h = 0;
            foreach (char s in plainText)
            {
                int index = Alphabet.GetSymbol(alf, s);
                h = (int)((Math.Pow((h + index), 2)) % (int)p);
            }
            return h;
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
    }
}

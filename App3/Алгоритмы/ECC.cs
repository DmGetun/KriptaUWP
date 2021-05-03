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

        public override string DefaultKey => "a=5\rb=7\rp=47\rk=4\rCu=6\rG=(3,7)";

        public string Key { get; private set; }

        public override bool IsReplaceText => true;

        public int Cu { get; private set; }
        public Point Du { get; private set; }

        public override string Group => "Асимметричные шифры";

        public override string CheckKey(string key)
        {
            throw new NotImplementedException();
        }
        /*
            Метод формирования ключей.
            Получаем на вход строку с указанными параметрами,
            разбиваем её на строки с каждым параметром,
            вырезаем числовые значения и конвертируем в числа.
        */
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
            int X, Y;
            if (!int.TryParse(pt[0], out X))
                throw new Error(Error.InvalidValueKey);
            if (!int.TryParse(pt[1], out Y))
                throw new Error(Error.InvalidValueKey);
            numbers[numbers.Length - 2] = X;
            numbers[numbers.Length - 1] = Y;

            return numbers;
        }
        /*
            Функция расшифрования.
            Разбиваем ключ, разбиваем текст.
            Заносим параметры из ключа в переменные,
            создаем объект эллептической кривой с 
            переданными параметрами.
            Считаем точку Q,
            в цикле проходимся по всем номерам зашифрованных символов
            и возводим эти номера в степерь координаты X точки Q по модулю p,
            по полученному индексу получаем символ из алфавита.
        */
        public override string Decrypt(string cipherText, Config config)
        {
            int[] keys = ParseKey(config.Key);
            var alf = Alphabet.GenerateAlphabet();

            int[] values = ParseText(cipherText);
            int[] e = new int[values.Length - 2];
            for(int i = 2; i < values.Length;i++)
                e[i - 2] = values[i];

            BigInteger a = keys[0];
            BigInteger b = keys[1];
            BigInteger p = keys[2];
            BigInteger k = keys[3];
            Cu = keys[4];

            Elliptic elliptic = new Elliptic(a, b, p);
            //Point R = elliptic.CheckPoint(new Point(values[0], values[1]));
            Point R = new Point(values[0], values[1]);

            int f = (int)F(p) - 1;

            Point Q = elliptic.GetValue(Cu, R);
            StringBuilder str = new StringBuilder();
            foreach(int number in e)
            {
                BigInteger M = (number * BigInteger.Pow(Q.X,f)) % p;
                str.Append(alf[(int)M]);
            }

            return str.ToString();
        }
        /*
            Фукнция разбиения зашифрованного текста.
            Вырезаем координаты точки R,
            далее за ней идут зашифрованные значения символов,
            вырезаем их и добавляем в массив.
        */
        private int[] ParseText(string cipherText)
        {
            cipherText = cipherText.Substring(cipherText.IndexOf(":") + 1);
            cipherText = cipherText.Replace("(", "").Replace(")", "");
            string[] rez = cipherText.Split(",");
            int[] indexes = rez[2].Split(' ',StringSplitOptions.RemoveEmptyEntries).Select(i => int.Parse(i)).ToArray();
            int[] numbers = new int[rez.Length - 1 + indexes.Length];
            numbers[0] = int.Parse(rez[0]);
            numbers[1] = int.Parse(rez[1]);
            for(int i = 0;i < indexes.Length; i++)
            {
                numbers[i + 2] = indexes[i];
            }

            return numbers;
        }
        /*
            Функция шифрования.
            Получаем ключ и текст, разбиваем ключ по переменным.
            Создаем объект эллептической кривой с переданными параметрами,
            Вычисляем точку Du, R и P
            В цикле получаем индекс буквы в алфавите,
            возводим её в степень координаты X Точки P по модулю p,
            добавляем в массив зашифрованных обозначений.
            Из массива формируем строку.
        */
        public override string Encrypt(string plainText, Config config)
        {
            int[] keys = ParseKey(config.Key);
            var alf = Alphabet.GenerateAlphabet();
            Key = config.Key;

            BigInteger a = keys[0];
            BigInteger b = keys[1];
            BigInteger p = keys[2];
            int k = keys[3];
            Cu = keys[4];

            Elliptic elliptic = new Elliptic(a, b, p);
            Point G = elliptic.CheckPoint(new Point(keys[5], keys[6]));

            Du = elliptic.GetValue(Cu, G);

            Point R = elliptic.GetValue(k, G);
            Point P = elliptic.GetValue(k, Du);

            BigInteger[] mass = new BigInteger[plainText.Length];
            int i = 0;
            foreach(char s in plainText)
            {
                int index = Alphabet.GetSymbol(alf, s);
                mass[i++] = (index * P.X) % p;
            }
            StringBuilder str = new StringBuilder();
            foreach (int num in mass)
                str.Append(num + " ");
            return string.Format("Пользователю будет отправлено:(({0},{1}),{2})",R.X,R.Y,str);
        }

        public override string GenerateKey()
        {
            int a = 0, b = 0, p = 0, k = 0, Cu = 0;
            Point G = new Point(0, 0);
            Random rand = new Random();
            BigInteger value = 0;
            do
            {
                a = rand.Next(1, 100);
                b = rand.Next(1, 100);
                p = rand.Next(1, 100);
                value = (4 * BigInteger.Pow(a, 3) + 27 * BigInteger.Pow(b, 2)) % p;
            }
            while (value == 0);

            Elliptic elliptic = new Elliptic(a, b, p);
            k = rand.Next(1, 20);
            Cu = rand.Next(1, 20);
            do
            {
                int a_ = rand.Next(1, 20);
                int b_ = rand.Next(1, 20);
                G = new Point(a_, b_);
            } while (!elliptic.IsGoodPoint(G));
            return $"a={a}\rb={b}\rp={p}\rk={k}\rCu={Cu}\rG={G}";

        }

        public override string KeyView()
        {
            string one = string.Format("Закрытый ключ: {0}\r", Cu);
            string two = string.Format("Открытый ключ: {0}", Du.ToString());
            return one + two;
        }
        /*
            Метод вычисления функции Эйлера.
        */
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

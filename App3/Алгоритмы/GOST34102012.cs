using App3.Помошники;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using UWP.Помошники;

namespace UWP.Алгоритмы
{
    class GOST34102012 : Algorithm
    {
        private BigInteger p,q;
        private BigInteger hash;
        private BigInteger hash2;
        private BigInteger hash1;
        private int k;
        private byte[] byteHash;

        public override string Name => "ГОСТ Р 34.10-2012";
        public override string DefaultKey => "a=2\rb=7\rp=11\rXa=6\rG=(10,9)";
        public override bool IsReplaceText => true;
        public int Xa { get; private set; }
        public Point Yu { get; private set; }

        public override string Group => "ЦП ГОСТ Р";

        public override string CheckKey(string key)
        {
            throw new NotImplementedException();
        }
        /*
            Получаем сообщение и r,s
            Проверяем коэффициенты a,b и точку G
            Вычисляем q
            Вычисляем открытый ключ Yu
            Вычисляем хеш, находим коэффициенты u1 и u2, с помощью них вычисляем точку P
            делим координату X точки P по модулю q,
            если результат равен r, то подпись верна.
        */
        public override string Decrypt(string cipherText, Config config)
        {
            string text = cipherText.Split(',')[0];
            BigInteger[] numbers = ParseText(cipherText);
            BigInteger r = numbers[0];
            BigInteger s = numbers[1];
            int[] keys = ParseKey(config.Key);
            int a = keys[0];
            int b = keys[1];
            int p = keys[2];
            int Xa = keys[3];
            BigInteger x_ = new BigInteger(keys[4]);
            BigInteger y_ = new BigInteger(keys[5]);
            Elliptic elliptic = new Elliptic(a, b, p);
            Point G = elliptic.CheckPoint(new Point(x_, y_));
            q = elliptic.Calculate_Q(p);

            Yu = elliptic.GetValue(Xa, G);
            if (!(0 < r || s < q))
                return "Подпись не верна.";

            Stribog stribog = new Stribog();
            byteHash = stribog.GetHashMessage(text);
            hash = new BigInteger(stribog.GetHashMessage(text));
            if (hash < 0) hash = -hash;
            hash2 = hash;
            BigInteger f = F(q) - 1;

            BigInteger v = Pow(hash, f, q);

            BigInteger u1 = (s * v) % q;
            if (u1 < 0) u1 += q;
            BigInteger u2 = q + (-(r * v) % q);
            if (u2 < 0) u2 += q;

            Point P1 = elliptic.GetValue((int)u1, G);
            Point P2 = elliptic.GetValue((int)u2, Yu);

            Point P = elliptic.SumPoint(P1,P2);
            BigInteger res = P.X % q;
            if (res < 0) res += q;
            if (res == r)
                return $"{res}=={r}\rПодпись верна.\r Точка P = [{u1}]{G} + [{u2}]{Yu} = ({P.X},{P.Y})";
            return "Подпись не верна.";
        }
        /*
            Получаем на вход сообщение и коэффициенты кривой с точкой G,
            Преверяем коэффициенты a,b и точку G.
            Вычисляем q, 
            Вычисляем хеш,
            Генерируем случайное число k,
            Вычисляем r и s. Если r или s = 0, возвращаеся к генерации числа.
            Отправляем пользователю сообщение и (r,s)
        */
        public override string Encrypt(string plainText, Config config)
        {
            int[] keys = ParseKey(config.Key);
            var alf = Alphabet.GenerateAlphabet();

            BigInteger a = keys[0];
            BigInteger b = keys[1];
            BigInteger p = keys[2];
            Xa = keys[3];
            Elliptic elliptic = new Elliptic(a, b, p);
            Point G = elliptic.CheckPoint(new Point(keys[4], keys[5]));
            q = elliptic.Calculate_Q(p);

            Stribog stribog = new Stribog();

            hash = new BigInteger(stribog.GetHashMessage(plainText));
            if (hash < 0) hash = -hash;
            hash1 = hash;
            BigInteger e = hash % q;
            if (e == 0) e = 1;
            k = 3;
            Point P = new Point();
            BigInteger r = 0;
            BigInteger s = 0;
            do
            {
                k = new Random().Next(1, (int)q);
                P = elliptic.GetValue(k, G);
                r = P.X % q;
                if (r < 0) r += q;
                s = ((r * Xa) + (k * e)) % q;
                if (s < 0) s += q;
            }
            while (r == 0 || s == 0);

            return $"{plainText},({r},{s})";
        }

        public override string KeyView()
        {
            string one = $"Секретный ключ: Xa = {Xa}\r";
            string two = $"Открытый ключ: Yu = ({Yu.X},{Yu.Y})\r";
            string four = $"Порядок подгруппы группы точек: q = {q}\r";
            string five = $"Сгенерированное число k = {k}\r";
            /*string three = $"Хеш сообщения: {BitConverter.ToString(byteHash).Replace("-","")}";*/
            string three = $"Хеш сообщения: {hash1}";
            return one + two + four + five + three;
        }
        /*
            Разбиваем полученный текст от пользователя
            на сообщения и значения r и s
        */
        private BigInteger[] ParseText(string cipherText)
        {
            cipherText = cipherText.Substring(cipherText.IndexOf(','));
            cipherText = cipherText.Replace("(", "").Replace(")", "");
            string[] keys = cipherText.Split(',', StringSplitOptions.RemoveEmptyEntries);
            BigInteger number = 0;
            BigInteger[] numbers = new BigInteger[keys.Length];
            for (int i = 0; i < keys.Length; i++)
            {
                if (BigInteger.TryParse(keys[i], out number))
                    numbers[i] = number;
                else throw new Error(Error.InvalidValueKey);
            }
            return numbers;
        }
        /*
            Вычисление функции Эйлера 
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
            Возведение в степень по модулю 
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
            Разбиваем введенный пользователем ключ 
            на коэффициенты a,b,p,Xa,G
        */
        private int[] ParseKey(string key)
        {
            string[] keys = key.Split('\r');
            int[] numbers = new int[keys.Length + 1];
            int number = 0;
            for (int i = 0; i < keys.Length - 1; i++)
            {
                if (int.TryParse(keys[i].Substring(keys[i].IndexOf('=') + 1), out number))
                {
                    numbers[i] = number;
                }
                else throw new Error(Error.InvalidValueKey);
            }

            string temp = keys[keys.Length - 1].Replace("(", "").Replace(")", "");
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

        public override string GenerateKey()
        {
            int a = 0, b = 0, p = 0, k = 0;
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
            do
            {
                int a_ = rand.Next(1, 100);
                int b_ = rand.Next(1, 100);
                G = new Point(a_, b_);
            } while (!elliptic.IsGoodPoint(G));
            return $"a={a}\rb={b}\rp={p}\rXa={k}\rG={G}";
        }
    }
}

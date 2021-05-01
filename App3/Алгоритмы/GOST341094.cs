using App3.Помошники;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using UWP.Алгоритмы;
using UWP.Помошники;

namespace UWP.Алгоритмы
{
    class GOST341094 : Algorithm
    {
        private BigInteger p, q, x, a;

        public override string Name => "ГОСТ Р 34.10-94";

        public override string DefaultKey => "P=31\rQ=5\rx=4";

        public override bool IsReplaceText => true;

        public override string CheckKey(string key)
        {
            throw new NotImplementedException();
        }
        /*
            Получаем сообщение и r,s
            Проверяем коэффициенты a,b и точку G
            Вычисляем хеш сообщения,
            Вычисляем z1 и z2,
            на их основе вычисляем u.
            Если u = r, то подпись верна.
        */
        public override string Decrypt(string cipherText, Config config)
        {
            string text = cipherText.Split(',')[0];
            BigInteger[] numbers = ParseText(cipherText);
            BigInteger r = numbers[0];
            BigInteger s = numbers[1];
            BigInteger[] keys = ParseKey(config.Key);
            p = keys[0];
            q = keys[1];
            //a = Calculate_a(p, q);
            BigInteger y = Pow(a, x, p);
            if (y < 0) y += p;

            Stribog stribog = new Stribog();
            BigInteger h = new BigInteger(stribog.GetHashMessage(text)) % q;
            if (h < 0) h = -h;
            if ((h % q) == 0) h = 1;

            BigInteger v = Pow(h, q - 2, q);
            if (v < 0) v += q;
            BigInteger z1 = (s * v) % q;
            if (z1 < 0) z1 += q;

            BigInteger z2 = ((q - r) * v) % q;
            if (z2 < 0) z2 += q;
            BigInteger u1 = (Pow(a, z1, p) * Pow(y, z2, p)) % p;
            if (u1 < 0) u1 += p;
            BigInteger u = u1 % q;
            if (u < 0) u += q;
            if (u == r)
                return $"u = {u} === r = {r}.Подпись верна";
            return "Подпись не верна";
        }
        /*
            разбить текст от отправителя на сообщение и r,s 
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
            Вычисляем a,
            находим хеш сообщения.
            Генерируем число k,
            на его основе вычисляем r и s,
            Если r или s = 0, то возвращаемся к генерации k.
            Отправляем сообщение и r,s получателю
        */
        public override string Encrypt(string plainText, Config config)
        {
            BigInteger[] keys = ParseKey(config.Key);
            p = keys[0];
            q = keys[1];
            x = keys[2];
            CheckNumbers(p, q, x);

            a = Calculate_a(p, q);

            Stribog stribog = new Stribog();
            BigInteger h = new BigInteger(stribog.GetHashMessage(plainText)) % q;
            if (h < 0) h = -h;
            if ((h % q) == 0) h = 1;
            BigInteger r = 0;
            BigInteger rr = 0;
            BigInteger s = 0;
            do
            {
                BigInteger k = Calculate_k(q);
                rr = Pow(a, k, p);
                if (rr < 0) rr += p;
                r = rr % q;
                if (r < 0) r += q;
                s = (x * r + k * h) % q;
                if (s < 0) s += q;
            } while (r == 0 || s == 0);
            return $"{plainText},({r},{s})";
        }
        /*
            Проверка чисел 
        */
        private void CheckNumbers(BigInteger p, BigInteger q, BigInteger x)
        {
            if (!IsTheNumberSimple(p))
                throw new Error("Число p не простое");
            if (!IsTheNumberSimple(q))
                throw new Error("Число q не простое");
            if (x >= q)
                throw new Error("Число x должно быть меньше q");
            if ((p - 1) % q != 0)
                throw new Error("Число q не является сомножителем числа p -1");
        }
        /*
            Проверка на простоту числа 
        */
        private bool IsTheNumberSimple(BigInteger n)
        {
            if (n < 2)
                return false;

            if (n == 2)
                return true;

            for (BigInteger i = 2; i < n / 2; i++)
                if (n % i == 0)
                    return false;

            return true;
        }
        /*
            Генерируем k 
        */
        private BigInteger Calculate_k(BigInteger q)
        {
            return new Random().Next(1, (int)q);
        }
        /*
            Вычисляем a 
        */
        private BigInteger Calculate_a(BigInteger p, BigInteger q)
        {
            Random rand = new Random();
            BigInteger f;
            do
            {
                BigInteger d = rand.Next(2, (int)p - 1);
                f = Pow(d, (p - 1) / q, p);
            }
            while (f == 1);

            return f;
        }
        /*
            Возведение в степень по модулю m 
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
            Разбить ключ на коэффициенты 
        */
        private BigInteger[] ParseKey(string key)
        {
            string[] keys = key.Split('\r');
            BigInteger[] result = new BigInteger[keys.Length];
            for (int i = 0; i < keys.Length; i++)
            {
                BigInteger number = 0;
                if (BigInteger.TryParse(keys[i].Substring(keys[i].IndexOf('=') + 1), out number))
                    result[i] = number;
                else throw new Error(Error.InvalidValueKey);
            }
            return result;
        }

        public override string GenerateKey()
        {
            /*            Random rand = new Random();
                        int p = 0, q = 0, x = 4;
                        do
                        {
                            p = rand.Next(600, 2000);
                            if (!IsTheNumberSimple(p)) continue;
                            if (p - 1 <= 2) continue;
                            q = CalculateQ(p);
                        } while ((p - 1) % q != 0);
                        x = rand.Next(2, q);

                        return $"P={p}\rQ={q}\rx={x}";*/
            return "P=31\rQ=5\rx=4";
        }

        /*        private int CalculateQ(int p)
                {
                    List<int> som = new List<int>();
                    for(int i = 0; i < p - 1; i++)
                    {
                        if (IsTheNumberSimple(i))
                            som.Add(i);
                    }

                    som.Reverse();
                    foreach (int item in som)
                        if ((p - 1) % item == 0)
                            return item;
                    return 0;
                }*/

        public override string KeyView()
        {
            string one = $"Открытые ключи:p={p},q={q},a={a}\r";
            string two = $"Секретный ключ:x={x}";
            return one + two;
        }
    }
}

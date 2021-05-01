using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWP.Помошники;

namespace UWP.Алгоритмы
{
    class DiffieHellman : Algorithm
    {
        private int n,a,Ya,Yb;

        public override string Name => "Обмен ключами Диффи-Хеллман";

        public override string DefaultKey => "n=34\ra=15\rKa=10";

        public override bool IsReplaceText => true;

        public int K2 { get; private set; }

        public override string CheckKey(string key)
        {
            throw new NotImplementedException();
        }
        /*
            генерируем секретный ключ пользователя K в Kb,
            Считаем открытый ключ пользователя A в Ya, возведя a в степень Kb по модулю n
            Вычисляем общий секретный ключ K, возведя Yb в степень Kb по модулю n
        */
        public override string Decrypt(string cipherText, Config config)
        {
            return $"Открытый ключ 2 пользователя: {Yb}\rПолученный 2 пользоваталем ключ: {K2}";
        }
        /*
            Функция генерации общего ключа.
            Записываем входные значения ключа в n и a,
            Записываем секретный ключ пользователя A в Ka
            Вычисляем открытый ключ пользователя A в Ya
            Вычисляем общий секретный ключ K, возведя Ya в степень Ka по модулю n.
        */
        public override string Encrypt(string plainText, Config config)
        {
            int[] keys = ParseKey(config.Key);
            n = keys[0];
            a = keys[1];

            int Ka = keys[2];
            int Kb = new Random().Next(3, n - 1);

            Ya = Pow(a, Ka, n);
            if (Ya < 0) Ya += n;

            Yb = Pow(a, Kb, n);
            if (Yb < 0) Yb += n;

            int K1 = Pow(Yb, Ka, n);
            if (K1 < 0) K1 += n;

            K2 = Pow(Ya, Kb, n);
            if (K2 < 0) K2 += n;
            return $"Отправляем пользователю: {Ya}\rПолученный пользователем 1 ключ: {K1}";
        }

        public override string GenerateKey()
        {
            int n = 0, a = 0, Ka = 0;
            Random rand = new Random();
            n = rand.Next(50, 150);
            a = rand.Next(2, n);
            Ka = rand.Next(3, n - 1);
            return $"n={n}\ra={a}\rKa={Ka}";
        }

        public override string KeyView()
        {
            string one = $"Общие параметры: {a},{n}\r";
            string two = $"Открытые ключи: Ya={Ya},Yb={Yb}";
            return one + two;
        }
        /*
            Разбить ключ на параметры n,a,Ka 
        */
        private int[] ParseKey(string key)
        {
            string[] keys = key.Split('\r');
            int[] numbers = new int[keys.Length];
            for(int i = 0; i < numbers.Length; i++)
            {
                int number = 0;
                if (!int.TryParse(keys[i].Substring(keys[i].IndexOf('=') + 1), out number))
                    throw new Error(Error.InvalidValueKey);
                numbers[i] = number;
            }
            CheckNumbers(numbers);
            return numbers;
        }
        /*
            Функция возведения числа в степень 
        */
        private int Pow(int x, int p, int m)
        {
            int r = 1;
            x %= m;
            for (int i = 1; i <= p; i++)
            {
                r = (r * x) % m;
            }

            return r;
        }
        /*
            Проверка входных параметров на соответствие:
            1 < a < n
            Ka > 2
            Ka < n - 1
        */
        private void CheckNumbers(int[] numbers)
        {
            int n = numbers[0];
            int a = numbers[1];
            int Ka = numbers[2];
            if (1 > a) throw new Error("Параметр a должен быть больше единицы");
            if (a > n) throw new Error("Параметр a должен быть меньше n");
            if (Ka < 2) throw new Error("Параметр Ka должен быть больше 2");
            if (Ka > n - 1) throw new Error("Параметр Ka должен быть меньше, чем параметр n - 1");
        }
    }
}

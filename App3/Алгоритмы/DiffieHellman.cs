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

        public override string CheckKey(string key)
        {
            throw new NotImplementedException();
        }

        public override string Decrypt(string cipherText, Config config)
        {
            int Kb = new Random().Next(3, n-1);
            Yb = Pow(a, Kb, n);
            int K = Pow(Yb, Kb, n);
            return $"Открытый ключ 2 пользователя: {Yb}\rПолученный 2 пользоваталем ключ: {K}";
        }

        public override string Encrypt(string plainText, Config config)
        {
            int[] keys = ParseKey(config.Key);
            n = keys[0];
            a = keys[1];
            int Ka = keys[2];
            Ya = Pow(a, Ka, n);
            int K = Pow(Ya, Ka, n);
            return $"Отправляем пользователю: {Ya}\rПолученный пользователем 1 ключ: {K}";
        }

        public override string GenerateKey()
        {
            throw new NotImplementedException();
        }

        public override string KeyView()
        {
            string one = $"Общие параметры{a},{n}\r";
            string two = $"Открытые ключи:Ya={Ya},Yb={Yb}";
            return one + two;
        }

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

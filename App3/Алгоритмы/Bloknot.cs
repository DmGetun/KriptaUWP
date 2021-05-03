using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWP.Помошники;

namespace UWP.Алгоритмы
{
    class Bloknot : Algorithm
    {
        GenPCH pch;
        public override string Name => "Шифр Блокнот Шеннона";

        public override string DefaultKey => "a=557\rc=7229\rt=3";

        public override bool IsReplaceText => true;

        public override string Group => "Шифры гаммирования";

        public int[] Gamma { get; private set; }

        public string Key = string.Empty;

        /*
            Проверка ключа на соответствие требованиям генерации гаммы
         */
        public override string CheckKey(string key)
        {
            int a, c;
            int[] num = KeyParse(key);
            a = num[0];
            c = num[1];
            if (a % 2 == 0) throw new Error("Параметр a должен быть нечетным");
            if (c % 4 != 1) throw new Error("Параметр c должен давать остаток 1 по модулю 4");
            return key;
        }
        /*
            Расшифрование шифра блокнот Шеннона
            Получаем на вход зашифрованную строку и ключ,
            Проверяем ключ и разбиваем его на параметры a,c,t.
            Генерируем псевдослучайную последовательность,
            переводим буквы зашифрованного текста в их номера по алфавиту,
            Прибавляем 33 к номеру символа, вычитаем гамму и производим деление по модулю 33.
        */
        public override string Decrypt(string cipherText, Config config)
        {
            var alf = new string(Alphabet.GenerateFullAlphabet().Values.ToArray());
            string key = CheckKey(config.Key);
            int[] keys = KeyParse(key);
            int a = keys[0];
            int c = keys[1];
            int t = keys[2];
            var pch = new GenPCH(cipherText, a, c, t);
            int[] numbers = pch.Generate();
            List<int> numbersSymbols = new List<int>();
            foreach (char s in cipherText)
            {
                numbersSymbols.Add(alf.IndexOf(s));
            }

            StringBuilder str = new StringBuilder();

            for (int i = 0; i < numbersSymbols.Count; i++)
            {
                int index = (alf.Length + numbersSymbols[i] - numbers[i]) % alf.Length;
                str.Append(alf[index]);
            }

            return str.ToString();
        }

        /*
            Шифрование шифром блокнот Шеннона
            Получаем на вход строку и ключ,
            Проверяем ключ и разбиваем его на параметры a,c,t.
            Генерируем псевдослучайную последовательность,
            переводим буквы открытого текста в их номера по алфавиту
            Используем сложение номера буквы и сгенерированного числа по модулю 33
            для получения зашифрованного символа    
         */
        public override string Encrypt(string plainText, Config config)
        {
            var alf = new string(Alphabet.GenerateFullAlphabet().Values.ToArray());
            string key = CheckKey(config.Key);
            int[] keys = KeyParse(key);
            int a = keys[0];
            int c = keys[1];
            int t = keys[2];
            
            pch = new GenPCH(plainText,a,c,t);
            int[] numbers = pch.Generate();
            Gamma = numbers;
            List<int> numbersSymbols = new List<int>();
            foreach(char s in plainText)
            {
                numbersSymbols.Add(alf.IndexOf(s));
            }
            StringBuilder str = new StringBuilder();
            for(int i = 0; i < numbersSymbols.Count; i++)
            {
                int index = (numbersSymbols[i] + numbers[i]) % alf.Length;
                str.Append(alf[index]);
            }

            return str.ToString();
        }
        
        /*
            Функция автоматического генерирования ключа
         */
        public override string GenerateKey()
        {
            var rand = new Random();
            int a, c, t;
            do
                a = rand.Next(500, 1000);
            while (a % 2 == 0);

            while (true)
            {
                c = rand.Next(1000, 10000);
                if (c % 4 == 1) break;
            }

            t = rand.Next(5, 30);
            

            return $"a={a}\rc={c}\rt={t}";
        }

        public override string KeyView()
        {
            StringBuilder str = new StringBuilder();
            foreach(int num in Gamma)
            {
                str.Append(num + " ");
            }
            return str.ToString();
        }

        /*
            Функция получения коэффициентов a,c,t из строки ключа
         */
        private int[] KeyParse(string _key)
        {
            string[] keys = _key.Split('\r');
            int[] rez = new int[keys.Length];

            for (int i = 0; i < keys.Length; i++)
            {
                rez[i] = int.Parse(keys[i].Trim().Substring(keys[i].IndexOf('=') + 1));
            }

            return rez;
        }
    }
}

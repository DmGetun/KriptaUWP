using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWP.Помошники;

namespace UWP.Алгоритмы
{
    class VertSwapa : Algorithm
    {
        public override string Name => "Шифр вертикальной перестановки";

        public override string DefaultKey => "ОКТЯБРЬ";

        public string Key { get; private set; }

        public override bool IsReplaceText => true;

        /*
            расшифровка шифра вертикальной перестановки
        */

        public override string Decrypt(string cipherText, Config config)
        {
            string key = CheckKey(config.Key.ToUpper());
            int length = cipherText.Length;
            var size = CalculateSize(length, key.Length);
            int rows = size.Item1;
            int cols = size.Item2;

            char[,] table = new char[rows, cols];

            string[] arr = GetColumnsSymbols(cipherText, key); // восстанавливаем колонки

            var alf = Alphabet.GenerateAlphabet();
            var alf_f = alf.Values.ToArray<char>();
            StringBuilder str = new StringBuilder(key);
            int j = 0;
            foreach (char s in alf_f) // восстнавливаем исходную таблицу, записывая колонки в соответствии с индексом символов ключа.
            {
                while (str.ToString().IndexOf(s) != -1)
                {
                    int index = str.ToString().IndexOf(s);
                    str[index] = '/';
                    for (int i = 0; i < rows; i++)
                    {
                        table[i, index] = arr[j][i];
                    }
                    j++;
                }
            }
            string plainText = GetPlainText(table); // проходимся по таблице и формируем строку открытого текста.
            return plainText.Replace("/", "");
        }

        /*
            Функция выписывания таблицы. Заполняет по 2 строки таблицы за раз.
         */

        private string GetPlainText(char[,] table)
        {
            int rows = table.GetLength(0);
            int cols = table.GetLength(1);
            StringBuilder str = new StringBuilder();
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    str.Append(table[i, j]);
                }
                i++;
                if (i == rows) return str.ToString();
                for (int j = cols - 1, k = cols * i; j >= 0; j--, k++)
                {
                    str.Append(table[i, j]);
                }
            }
            return str.ToString();
        }

        /*
            Функция восстановления колонок из шифртекста.
            Берем по rows символов шифртекста и записываем в массив
         */

        private string[] GetColumnsSymbols(string cipherText, string key)
        {
            StringBuilder str = new StringBuilder(cipherText);
            int rows = CalculateSize(str.Length, key.Length).Item1;
            while (str.Length % rows != 0)
            {
                str.Append('/');
            }
            string[] rez = new string[str.Length / rows];
            string text = str.ToString();
            for (int i = 0, j = 0; i < text.Length; i += rows, j++)
            {
                rez[j] = text.Substring(i, rows);
            }
            return rez;
        }

        /*
            Расшифрование шифра вертикальной перестановки.
            Записываем исходный текст в таблицу,
            получаем номера букв ключа в алфавите.
            Выписываем символы в одной колонке таблицы,
            порядок выписывания колонок определяем числовым значением ключа.
        */

        public override string Encrypt(string plainText, Config config)
        {
            string key = CheckKey(config.Key.ToUpper());

            char[,] table = GetTable(plainText, key); // записываем исходный текст в таблицу

            var alf = Alphabet.GenerateAlphabet();
            var alf_f = alf.Values.ToArray<char>();
            List<int> indexes = new List<int>();
            foreach (char s in key) // получаем индексы символов в алфавите
            {
                indexes.Add(Alphabet.GetSymbol(alf, s));
            }
            int rows = table.GetLength(0);
            int cols = table.GetLength(1);

            StringBuilder str = new StringBuilder(key);
            StringBuilder encryptTable = new StringBuilder();

            foreach (char s in alf_f) // выписываем колонки в соответствиии с ключом.
            {
                while (str.ToString().IndexOf(s) != -1)
                {
                    int index = str.ToString().IndexOf(s);
                    str[index] = '/';
                    for (int i = 0; i < rows; i++)
                    {
                        encryptTable.Append(table[i, index]);
                    }
                }
            }
            return encryptTable.ToString();
        }

        // записываем исходный текст в таблицу
        private static char[,] GetTable(string plainText, string key)
        {
            int length = plainText.Length;
            Tuple<int, int> size = CalculateSize(length, key.Length);
            int rows = size.Item1;
            int cols = size.Item2;
            char[,] table = new char[rows, cols];
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++) // пишем текст в нечетную строку
                {
                    int index = i * cols + j;
                    if (index >= length)
                    {
                        table[i, j] = 'Ф';
                        continue;
                    }
                    char symbol = plainText[index];
                    table[i, j] = symbol;
                }
                i++;
                if (i == rows) return table;
                for (int j = cols - 1, k = cols * i; j >= 0; j--, k++) // пишем текст в четную строку в обратном порядке
                {
                    if (k >= length)
                    {
                        table[i, j] = 'Ф';
                        continue;
                    }
                    table[i, j] = plainText[k];
                }
            }
            return table;
        }

        private static Tuple<int, int> CalculateSize(int length, int keyLen)
        {
            int cols = keyLen;
            int rows = length / cols;
            if (rows * cols < length)
            {
                rows += 1;
            }
            return new Tuple<int, int>(rows, cols);
        }

        public override string CheckKey(string key)
        {
            if(key.Length == 0)
            {
                throw new Error(Error.InvalidKey);
            }
            foreach(char s in key)
            {
                if (!(s >= 'А' && s <= 'Я'))
                    throw new Error(Error.InvalidKey);
            }
            Key = key;
            return key;
        }

        public override string KeyView()
        {
            var alf = Alphabet.GenerateAlphabet().Values.ToArray();
            int i = 1;
            int[] numbers = new int[Key.Length];
            StringBuilder text = new StringBuilder(Key);
            StringBuilder str = new StringBuilder();
            foreach (char s in alf)
            {
                while (text.ToString().IndexOf(s) != -1)
                {
                    numbers[text.ToString().IndexOf(s)] = i;
                    text[text.ToString().IndexOf(s)] = '/';
                    i++;
                }
            }
            foreach (int num in numbers)
            {
                str.Append(num.ToString() + " ");
            }
            return str.ToString();
        }

        public override string GenerateKey()
        {
            return new Plaifer().GenerateKey();
        }
    }
}

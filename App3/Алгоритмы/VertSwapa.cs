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

        public override string Group => "Шифры блочной замены";

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

            int[] keys = GetKeyNumbers(key);
            cipherText = RestoreSpace(cipherText,keys,rows,cols);
            return cipherText;
        }
        /*
            Получить порядковые номера букв ключа по алфавиту. 
        */
        private int[] GetKeyNumbers(string key)
        {
            int[] numbers = new int[key.Length];
            var alf = Alphabet.GenerateAlphabet();
            var alf_f = new string(alf.Values.ToArray<char>());
            StringBuilder str = new StringBuilder(key);
            foreach (char s in alf_f)
            {
                while (str.ToString().IndexOf(s) != -1)
                {
                    int index = str.ToString().IndexOf(s);
                    str[index] = '/';
                    numbers[index] = alf_f.IndexOf(s);
                }
            }
            return numbers;
        }
        // Восстановление пробелов
        private string RestoreSpace(string input, int[] key, int rows,int cols)
        {
            int strLen = input.Length;
            char[,] table = new char[rows, cols];
            key = ReplaceKey(key);
            int spaceCount = rows * cols - strLen;
            int[] l = new int[key.Length];
            int fullLen = 0;

            // По длине ключа получить количество букв в каждом столбце 
            for (int i = 0; i < key.Length; i++)
                if (i < key.Length - spaceCount)
                    l[i] = rows;
                else
                    l[i] = rows - 1;

            for (int i = input.Length; i < rows * cols; i++)
                input += " ";

            List<int> indexKey = new List<int>(key);
            if(rows % 2 == 1) // пробелы находятся справа
            {
                Formtable(input, key, ref table, l, ref fullLen, indexKey);
            }
            else
            {
                Array.Reverse(l);
                Formtable(input, key, ref table, l, ref fullLen, indexKey);
            }

            return RestoreTable(table,key).Trim();
        }
        // Сформировать исходную таблицу с открытым текстом
        private void Formtable(string input, int[] key, ref char[,] table, int[] l, ref int fullLen, List<int> indexKey)
        {
            int step = 0;
            for (int i = 0; i < key.Length; i++)
            {
                    step = indexKey.IndexOf(i); // получаем исходный номер столбца
                    string row = input.Substring(fullLen, l[step]); // берем из строки буквы в количестве, полученном из l
                    fullLen += row.Length;
                    AddRow(ref table, row, step);
            }
        }
        // Выписать открытый текст из таблицы
        private string RestoreTable(char[,] table, int[] key)
        {
            string res = string.Empty;

            for (int i = 0; i < table.GetLength(0); i++)
            {
                for (int j = 0; j < table.GetLength(1); j++)    
                {
                    if (i % 2 == 0)
                        res += table[i, j];
                    else
                        res += table[i, table.GetLength(1) - j - 1];
                }
            }
                    
            return res;
        }
        /*
            Добавить строку в таблицу открытого текста 
        */
        private void AddRow(ref char[,] table, string row, int start)
        {
            for (int i = 0; i < table.GetLength(0); i++)
            {
                if (i >= row.Length)
                {
                    table[i, start] = ' ';
                    continue;
                }
                table[i, start] = row[i];
            }
        }
        // Получить порядок выписывания столбцов по ключу
        private int[] ReplaceKey(int[] key)
        {
            int[] t = new int[key.Length];
            List<int> a = new List<int>(key);
            for(int i = 0; i < key.Length; i++)
            {
                int min = a.Min();
                int index = a.IndexOf(min); // вернуть индекс минимального элемента
                t[index] = min - min + i; // вычислить, каким будет выписан столбец
                a[index] = int.MaxValue;
            }
            return t;
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
            foreach (char s in key) // получаем индексы символов ключа в алфавите
            {
                indexes.Add(Alphabet.GetSymbol(alf, s));
            }
            int rows = table.GetLength(0);

            StringBuilder str = new StringBuilder(key);

            var encryptTable = GetResultString(table, alf_f, rows, str);

            return encryptTable.ToString().Replace(" ", "");
        }

        // Получить итоговую строку по таблице и ключу
        private static StringBuilder GetResultString(char[,] table, char[] alf_f, int rows, StringBuilder str)
        {
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
            return encryptTable;
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
                        table[i, j] = ' ';
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
                        table[i, j] = ' ';
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

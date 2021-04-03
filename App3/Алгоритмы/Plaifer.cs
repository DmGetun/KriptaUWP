using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWP.Помошники;

namespace UWP.Алгоритмы
{
    class Plaifer : Algorithm
    {
        public override string Name => "Шифр Плейфера";

        public override string DefaultKey => "РЕСПУБЛИКА";

        public override bool IsReplaceText => true;

        public char[,] Key;

        /*
            расшифрование шифра Плейфера.
            Получаем ключ, генерируем на его основе матрицу.
            Разбиваем текст на биграммы.
            Для обоих символов биграммы находим их индексы.
            Получаем зашифрованные символы сдвигом текуших символов на -1
            или с помощью прямоугольника. 
         */

        public override string Decrypt(string cipherText, Config config)
        {
            string key = CheckKey(config.Key.ToUpper());
            char[,] alf = Alphabet.GenerateMatrixAlphabet(key);
            Key = alf;

            var lPlainText = Split(cipherText);
            StringBuilder plainText = new StringBuilder();

            foreach (string item in lPlainText)
            {
                var index = GetIndexSymbol(item[0], alf);
                var index2 = GetIndexSymbol(item[1], alf);

                int i_index_1 = index.Item1,
                    j_index_1 = index.Item2;
                int i_index_2 = index2.Item1,
                    j_index_2 = index2.Item2;

                plainText.Append(BigramSymbolsEncrypt(alf, i_index_1, i_index_2, j_index_1, j_index_2,-1));
            }
            return plainText.ToString();
        }

        /*
            шифрование шифра Плейфера.
            Получаем ключ, генерируем на его основе матрицу.
            Разбиваем текст на биграммы.
            Для обоих символов биграммы находим их индексы.
            Получаем зашифрованные символы сдвигом текуших символов на 1
            или с помощью прямоугольника. 
        */

        public override string Encrypt(string plainText, Config config)
        {
            string key = CheckKey(config.Key.ToUpper());
            char[,] alf = Alphabet.GenerateMatrixAlphabet(key);
            Key = alf;

            for (int i = 0; i < plainText.Length - 1; i++)
            {
                if (plainText[i] == plainText[i + 1])
                {
                    plainText = plainText.Substring(0, i + 1) + 'Ф' + plainText.Substring(i + 1);
                }
            }

            if (plainText.Length % 2 != 0)
            {
                plainText += 'Ф';
            }
            plainText = CheckText(plainText);
            var lPlainText = Split(plainText);
            StringBuilder cipherText = new StringBuilder();

            foreach (string item in lPlainText)
            {
                var index = GetIndexSymbol(item[0], alf);
                var index2 = GetIndexSymbol(item[1], alf);

                int i_index_1 = index.Item1,
                    j_index_1 = index.Item2;
                int i_index_2 = index2.Item1,
                    j_index_2 = index2.Item2;

                cipherText.Append(BigramSymbolsEncrypt(alf, i_index_1, i_index_2, j_index_1, j_index_2,1));
            }

            return cipherText.ToString();
        }

        private string CheckText(string plainText)
        {
            return plainText.Replace('Ъ', 'Ь').Replace('Й', 'И').Replace('Ё','Е');
        }

        /*
            Получаем индексы символов.
            Если символы находятся в одной строке, то увеличиваем индекс колонки на 1 или -1,
            если при этом выходим за границы - меняем индекс на противоположный.
            
            Аналогично для символов в одной колонке, только меняем индекс строки.
            Если символы в разных колонках и строках, то соединяем их прямоугольником
            и берем символы с противоположных углов прямоугольника.
            
        */

        private string BigramSymbolsEncrypt(char[,] alf, int i1, int i2, int j1, int j2,int step)
        {
            StringBuilder rez = new StringBuilder();
            int rows = alf.GetLength(0);
            int cols = alf.GetLength(1);

            if (i1 == i2)
            {
                j1 += step;
                if (j1 < 0) j1 = cols - 1; // индекс колонки меньше допустимого, меняем на индекс правой колонки.
                if (j1 >= cols) j1 = 0; // индекс колонки больше допустимого, меняем на индекс левой колонки.
                j2 += step;
                if (j2 < 0) j2 = cols - 1;
                if (j2 >= cols) j2 = 0;
                rez.Append(new char[] { alf[i1, j1], alf[i2, j2] });
            }

            else if (j1 == j2)
            {
                i1 += step;
                if (i1 < 0) i1 = rows - 1; // индекс строки меньше допустимого, меняем на индекс нижней строки.
                if (i1 >= rows) i1 = 0; // индекс строки больше допустимого, меняем на индекс верхней строки.
                i2 += step;
                if (i2 < 0) i2 = rows - 1;
                if (i2 >= rows) i2 = 0;
                rez.Append(new char[] { alf[i1, j1], alf[i2, j2] });
            }

            else
            {
                rez.Append(new char[] { alf[i1, j2], alf[i2, j1] });
            }


            return rez.ToString();
        }

        private List<string> Split(string str)
        {
            List<string> list = new List<string>();
            int i = 0;
            while (i < str.Length - 1)
            {
                list.Add(str.Substring(i, 2));
                i += 2;
            }
            return list;
        }

        private Tuple<int, int> GetIndexSymbol(char symbol, char[,] alf)
        {
            for (int i = 0; i < alf.GetLength(0); i++)
            {
                for (int j = 0; j < alf.GetLength(1); j++)
                {
                    if (symbol == alf[i, j])
                    {
                        return new Tuple<int, int>(i, j);
                    }
                }
            }
            return null;
        }

        public override string CheckKey(string key)
        {

            foreach (char s in key)
            {
                if(!(s >= 'А' && s <= 'Я'))
                {
                    throw new Error(Error.InvalidKey);
                }
            }

            HashSet<char> hash = new HashSet<char>(key);
            return new string(hash.ToArray());
        }

        public override string KeyView()
        {
            int rows = Key.GetLength(0);
            int cols = Key.GetLength(1);
            string str = String.Empty;

            for(int i = 0; i < rows; i++)
            {
                for(int j = 0; j < cols; j++)
                {
                    str += Key[i, j].ToString().PadRight(3);
                }
                str += '\r';
            }

            return str;
        }

        public override string GenerateKey()
        {
            var rand = new Random();
            var alf = Alphabet.GenerateAlphabet().Values.ToArray();
            int len = rand.Next(5, 10);
            StringBuilder key = new StringBuilder();

            for(int i = 0; i < len; i++)
            {
                int index = rand.Next(0, 32);
                key.Append(alf[index]);
            }

            return key.ToString();
        }
    }
}

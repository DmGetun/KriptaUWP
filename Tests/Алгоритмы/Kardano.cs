using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWP.Помошники;

namespace UWP.Алгоритмы
{
    class Kardano : Algorithm
    {
        public override string Name => "Решетка Кардано";

        public override string DefaultKey => Algorithm.NonKey;

        public override bool IsReplaceText => true;

        public string Key;

        public override string CheckKey(string key)
        {
            return null;
        }

        /*
            Расшифрование решетки Кардано.
            Генерируем решетку, вращаем её вверх и влево, вверх и влево.
            Записываем зашифрованные символы в решетку.
            Идем по положениям решетки и ищем открытые ячейки,
            выписываем символы из них.
        */

        public override string Decrypt(string cipherText, Config config)
        {
            var alf = Alphabet.GenerateKardano(cipherText);
            var alfUp = SwapLeft(SwapUp(alf));
            var alfLeft = SwapUp(alf);
            var alfRight = SwapLeft(alf);

            List<int[][]> reshetka = new List<int[][]>();
            reshetka.Add(alf);
            reshetka.Add(alfUp);
            reshetka.Add(alfLeft);
            reshetka.Add(alfRight);

            int rows = alf.Length;
            int cols = alf[0].Length;
            char[][] encryptedText = new char[rows][];

            for (int i = 0; i < rows; i++)
            {
                encryptedText[i] = new char[cols];
            }

            int index = 0;
            for(int i = 0; i < rows; i++)
            {
                for(int j = 0; j < cols; j++)
                {
                    encryptedText[i][j] = cipherText[index++];
                }
            }

            StringBuilder rez = new StringBuilder();
            foreach (int[][] k in reshetka)
            {
                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        if (k[i][j] == 1) continue;

                        rez.Append(encryptedText[i][j]);
                    }
                }
            }

            return rez.ToString();
        }

        /*
            Зашифрование решетки Кардано.
            Генерируем решетку, вращаем её вверх и влево, вверх и влево.
            Идем по положениям решетки и записываем символы исходного текста в открытые ячейки.
            После окончания текста заполняем оставшиеся пропуски нулями.
         */

        public override string Encrypt(string plainText, Config config)
        {
            var alf = Alphabet.GenerateKardano(plainText);
            var alfUp = SwapLeft(SwapUp(alf));
            var alfLeft = SwapUp(alf);
            var alfRight = SwapLeft(alf);

            List<int[][]> reshetka = new List<int[][]>();
            reshetka.Add(alf);
            reshetka.Add(alfUp);
            reshetka.Add(alfLeft);
            reshetka.Add(alfRight);

            int rows = alf.Length;
            int cols = alf[0].Length;
            char[][] encryptedText = new char[rows][];
            for(int i = 0; i < rows; i++)
            {
                encryptedText[i] = new char[cols];
            }
            int index = 0;
            foreach (int[][] k in reshetka) // цикл по положениям решетки
            {
                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        if (k[i][j] == 1) continue; // если ячейка закрыта, то пропускаем.

                        if(index >= plainText.Length)
                        {
                            encryptedText = EndEncrypt(encryptedText);
                            break;
                        }

                        encryptedText[i][j] = plainText[index++];
                    }
                }
            }

            StringBuilder text = new StringBuilder();

            for(int i = 0; i < rows; i++)
            {
                for(int j = 0; j < cols; j++)
                {
                    if (encryptedText[i][j] == '\0') continue;
                    text.Append(encryptedText[i][j]);
                }
            }
            return text.ToString();            
        }
        // после окончания символов текста заполняем пропуски в решетке случайными символами
        private char[][] EndEncrypt(char[][] encryptedText)
        {
            int rows = encryptedText.Length;
            int cols = encryptedText[0].Length;
            var alf = Alphabet.GenerateAlphabet().Values.ToArray<char>();

            Random rand = new Random();
            for(int i = 0;i < rows; i++)
            {
                for(int j = 0; j < cols; j++)
                {
                    if(encryptedText[i][j] == '\0')
                    {
                        encryptedText[i][j] = alf[rand.Next(0,31)];
                    }
                }
            }

            return encryptedText;
        }

        // поворот решетки влево
        private static int[][] SwapLeft(int[][] alf)
        {
            int rows = alf.Length;
            int cols = alf[0].Length;
            int[][] rez = new int[rows][];

            for (int i = 0; i < rows; i++)
            {
                rez[i] = new int[cols];
                for (int j = 0; j < cols; j++)
                {
                    rez[i][j] = alf[i][cols - j - 1];
                }
            }
            return rez;
        }

        // поворот решетки вправо
        private static int[][] SwapUp(int[][] alf)
        {
            int rows = alf.Length;
            int cols = alf[0].Length;
            int[][] rez = new int[rows][];

            for(int i = 0; i < rows; i++)
            {
                rez[i] = new int[cols];
                rez[i] = alf[rows - i - 1];
            }
            return rez;
        }

        public override string KeyView()
        {
            return null;
        }

        public override string GenerateKey()
        {
            return null;
        }
    }
}

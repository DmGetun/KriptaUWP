using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWP.Помошники;

namespace UWP.Алгоритмы
{
    class Matrics : Algorithm
    {
        Matrix matrix;

        public override string Name => "Матричный шифр";

        public override string DefaultKey => "1,4,8\r3,7,2\r6,9,5";

        public override bool IsReplaceText => true;

        public override string Group => "Шифры блочной замены";

        private string Key;

        /*
            Расшифрование Матричный шифр.
            Получаем ключевую матрицу,
            находим обратную ей матрицу.
            Разбиваем строку из зашифрованных чисел по 3 числа,
            используем умножение матрицы на вектор для
            получения исходных номеров букв.
        */

        public override string Decrypt(string cipherText, Config config)
        {
            var alf_f = Alphabet.GenerateAlphabet();
            string _key = CheckKey(config.Key);
            int[,] matric = GetKey(_key);

            matrix = new Matrix(matric);
            double[,] key = matrix.GetReverseMatric(); // вычисляем обратную матрицу
            if (matrix.flag)
                throw new Error(Error.MatrixValue);

            int[] numbers = cipherText.Split(' ').
                            Where(x => !string.IsNullOrWhiteSpace(x)).
                            Select(x => int.Parse(x)).ToArray(); // перевод чисел в тип int

            var listSymbols = GetListNumbers(numbers); // разбиваем шифртекст по 3 цифры
            int[] plainSymbols = GetDecryptSymbols(listSymbols, key); // умножение матрицы на вектор
            StringBuilder plain = new StringBuilder();
            foreach (int symbol in plainSymbols)
            {
                plain.Append(alf_f[symbol]); // добавляем символ по его номеру.
            }
            return plain.ToString();
        }
        /*
            Умножаем матрицу на вектор и получаем дешифрованные числовые эквиваленты символов   
        */
        private int[] GetDecryptSymbols(List<int[]> vs, double[,] matric)
        {
            List<int[]> a = new List<int[]>();
            foreach (int[] row in vs) // умножение матрицы на вектор
            {
                double[] sum = new double[3];
                for (int i = 0; i < 3; i++)
                {
                    sum[i] = matric[i, 0] * row[0] + matric[i, 1] * row[1] + matric[i, 2] * row[2];
                }
                a.Add(sum.Select(x => Convert.ToInt32(x)).ToArray());
            }
            int[] rez = new int[a.Count * 3];
            int k = 0;
            foreach (int[] elem in a) // полученные числа записываем в массив
            {
                for (int i = 0; i < elem.Length; i++)
                {
                    rez[k++] = elem[i];
                }
            }
            return rez;
        }
        /*
            Шифрование Матричный шифр.
            Получаем на вход клучевую матрицу.
            Переводим буквы открытого текста в их порядковые номера в алфавите.
            Разбиваем по 3 числа.
            Используем формулу умножения матрицы на вектор для получения зашифрованных номеров.
         */
        public override string Encrypt(string plainText, Config config)
        {
            string key = CheckKey(config.Key);
            int[,] matric = GetKey(key);

            while (plainText.Length % 3 != 0)
            {
                plainText += 'Ф';
            }

            var alf = Alphabet.GenerateAlphabet();
            int[] indexesSymbols = new int[plainText.Length];
            for (int i = 0; i < plainText.Length; i++)
            {
                indexesSymbols[i] = Alphabet.GetSymbol(alf, plainText[i]); // переводим букву в цифру
            }

            List<int[]> vs = GetListNumbers(indexesSymbols); // разбиваем исходный текст по 3 цифры

            string rez = GetEncryptSymbols(vs, matric); // используем формулу умножения матрицы на вектор 
            return rez;
        }

        private int[,] GetKey(string key)
        {
            int[,] rez = new int[3, 3];
            string[] arr = key.Split('\r');
            int j = 0;
            foreach(string str in arr)
            {
                string[] temp = str.Split(',');
                for(int i = 0; i < temp.Length; i++)
                {
                    rez[j, i] = int.Parse(temp[i].Trim());
                }
                j++;
            }

            return rez;
        }

        /*
            Разбиваем числовые эквиваленты символов по 3 числа. 
        */
        private static List<int[]> GetListNumbers(int[] indexesSymbols)
        {
            List<int[]> vs = new List<int[]>();
            for (int i = 0; i < indexesSymbols.Length - 2; i = i + 3)
            {
                int a = indexesSymbols[i];
                int b = indexesSymbols[i + 1];
                int c = indexesSymbols[i + 2];
                vs.Add(new int[] { a, b, c });
            }
            return vs;
        }

        /*
          Умножаем матрицу на вектор и получаем зашифрованные числовые эквиваленты символов   
        */
        private string GetEncryptSymbols(List<int[]> vs, int[,] matric)
        {
            List<int[]> a = new List<int[]>();
            foreach (int[] row in vs)
            {
                int[] sum = new int[3];
                for (int i = 0; i < 3; i++)
                {
                    sum[i] = matric[i, 0] * row[0] + matric[i, 1] * row[1] + matric[i, 2] * row[2]; // умножение матрицы на вектор
                }
                a.Add(sum);
            }
            StringBuilder rez = new StringBuilder();

            foreach (int[] elem in a)
            {
                for (int i = 0; i < elem.Length; i++)
                {
                    string temp = elem[i].ToString() + ' ';
                    rez.Append(temp);
                }
            }
            return rez.ToString();
        }

        /*
            Проверка матрицы на соответствие 
        */

        public override string CheckKey(string key)
        {
            string[] arr = key.Split('\r');
            int[,] rez = new int[3, 3];
            int num;
            int j = 0;
            foreach (string str in arr)
            {
                
                string[] temp = str.Split(',');
                if (temp.Length != 3) return Error.MatrixSize;
                for (int i = 0; i < temp.Length; i++)
                {
                    if (!int.TryParse(temp[i].Trim(),out num))
                    {
                        throw new Error(Error.MatrixSize);
                    }
                    rez[j, i] = int.Parse(temp[i].Trim());
                }
                j++;
            }
            if (j != 3) throw new Error(Error.MatrixSize);

            int opred = new Matrix(rez).GetMajorOpredelitel();
            if (opred == 0)
                throw new Error(Error.MatrixValue);
            Key = key;
            return key;
        }

        public override string KeyView()
        {
            return Key;
        }

        public override string GenerateKey()
        {
            StringBuilder str = new StringBuilder();
            int rows = 3; int cols = 3;
            var rand = new Random();

            for(int i = 0; i < rows; i++)
            {
                for(int j = 0; j < cols; j++)
                {
                    str.Append(rand.Next(4,50).ToString() + ",");
                }
                str.Remove(str.Length - 1, 1);
                str.Append('\r');
            }

            return str.Remove(str.Length - 1,1).ToString();
        }
    }
}

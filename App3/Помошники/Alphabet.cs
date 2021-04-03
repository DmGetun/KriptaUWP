using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UWP.Помошники
{
    class Alphabet
    {
        private static int SymbolsCount = 32;
        public static int SYMBOLS_COUNT = 32;
        static List<Case> cases;
        static StringBuilder StartText;
        enum Case
        {
            Upper,
            Lower
        }
        public static Dictionary<int, char> GenerateAlphabet()
        {

            int start = Convert.ToInt32('А');
            int stop = Convert.ToInt32('Я');
            int k = 1;
            var alf = new Dictionary<int, char>();

            for (int i = start; i <= stop; i++)
            {
                alf[k++] = Convert.ToChar(i);
            }

            return alf;
        }

        public static Dictionary<int, char> GenerateFullAlphabet()
        {
            var alf = new Dictionary<int, char>();
            int start = Convert.ToInt32('А');
            int stop = Convert.ToInt32('Я');
            int k = 1;
            int i = start;
            for(;i < start + 6; i++)
            {
                alf[k++] = Convert.ToChar(i);
            }
            alf[k++] = Convert.ToChar('Ё');
            for (;i <= stop; i++)
            {
                alf[k++] = Convert.ToChar(i);
            }
            return alf;
        }

        public static int CheckSymbol(char symbol)
        {
            int index = Convert.ToInt32(symbol);

            if (index > Convert.ToInt32('а') && index < Convert.ToInt32('я'))
            {
                return 0;
            }
            else return 1;
        }

        public static string SetSymbolsCase(string text)
        {
            cases = new List<Case>();
            foreach (char s in text)
            {
                if (!(s >= 'А' && s <= 'Я') && !(s >= 'а' && s <= 'я'))
                {
                    if (!(s == 'Ё' || s == 'ё') && !(s == ' ') && !(s == '.') && !(s == ','))
                    {
                        return Error.WrongSymbol;
                    }
                }
                if (char.IsUpper(s))
                {
                    cases.Add(Case.Upper);
                }
                else
                {
                    cases.Add(Case.Lower);
                }
            }
            return null;
        }

        public static string GetSymbolsCase(string text)
        {
            StringBuilder str = new StringBuilder();
            StringBuilder newText = new StringBuilder(text);
            int i = 0;
            if (cases.Count == 0) return text.ToUpper();
            try
            {
                foreach (char s in text)
                {
                    if(i == cases.Count)
                    {
                        str.Append(text.Substring(i));
                        return str.ToString();
                    }
                    Case c = cases[i];
                    switch (c)
                    {
                        case Case.Upper:
                            str.Append(char.ToUpper(newText[i++]));
                            break;
                        case Case.Lower:
                            str.Append(char.ToLower(newText[i++]));
                            break;
                    }
                }
                cases.Clear();
                return str.ToString();
            }
            catch
            {
                cases.Clear();
                return Error.WrongSymbol;
            }
        }

        public static string[] GenerateKeyAlphabet(string key, Dictionary<int, string> alf)
        {
            var keyAlf = new string[key.Length];
            for (int i = 0; i < key.Length; i++)
            {
                foreach (string s in alf.Values)
                {
                    if (s.ToUpper()[0] == key.ToUpper()[i])
                    {
                        keyAlf[i] = s;
                    }
                }
            }
            return keyAlf;
        }

        internal static char GetSymbolPolibii(Dictionary<char, string> alf, string s)
        {
            var a = alf.Keys.ToArray();
            for (int i = 1; i <= 6; i++)
            {
                for (int j = 1; j <= 6; j++)
                {
                    string index = i.ToString() + j.ToString();
                    if (index.Equals(s))
                    {
                        return a[(i * 6 - (6 - j) - 1)];
                    }
                }
            }

            return ' ';
        }

        public static string CheckText(string text)
        {
            text = text.Replace('ё', 'е');
            text = text.Replace('Ё', 'Е');

            return text;
        }

        public static int GetSymbol(Dictionary<int, char> alf, char symbol)
        {
            for (int i = 1; i < 33; i++)
            {
                if (symbol == alf[i])
                {
                    return i;
                }
            }
            return 0;
        }

        public static Dictionary<char, string> PolibiiAlphabet()
        {
            var alf = new Dictionary<char, string>();
            int k = Convert.ToInt32('А');
            string index;
            for (int i = 1; i <= 6; i++)
            {
                for (int j = 1; j <= 6; j++)
                {
                    index = i.ToString() + j.ToString();
                    alf[Convert.ToChar(k++)] = index;
                    if (index == "63")
                    {
                        break;
                    }
                }
            }
            return alf;
        }

        private static Dictionary<int, char> GenForTritemy()
        {
            var alf = new Dictionary<int, char>();
            int start = Convert.ToInt32('А');
            int stop = Convert.ToInt32('Я');
            int index = 0;
            for (int i = start; i <= stop; i++)
            {
                alf[index++] = Convert.ToChar(i);
            }
            return alf;
        }

        public static int[][] GenerateKardano(string s)
        {
            string[] start = { "1011111111",
                               "0111010011",
                               "1011101110",
                               "1110111011",
                               "1011111111",
                               "1101100110" };

            Tuple<int, int> size = GetSize(s);
            int rows = size.Item1;
            int cols = size.Item2;

            int[][] temp = new int[rows][];
            
            for(int i = 0; i < rows; i++)
            {
                temp[i] = new int[cols];
            }

            for (int i = 0; i < rows; i++)
                for(int j = 0; j < cols; j++)
                    temp[i][j] = (int)char.GetNumericValue(start[i % 6][j % 10]);
            

            int[][] rez = new int[rows][];
            
            for(int i = 0;i < rows; i++)
            {
                rez[i] = new int[cols];
                for(int j = 0; j < cols; j++)
                {
                    rez[i][j] = temp[i % rows][j % cols];
                }
            }

            return rez;

        }

        private static Tuple<int, int> GetSize(string text)
        {
            int len = text.Length;
            int rows = 6;
            int cols = 10;

            while (true)
            {
                if (len <= rows * cols)
                {
                    break;
                }
                rows += rows;
                cols += cols;
            }
            return new Tuple<int, int>(rows, cols);
        }

        public static Dictionary<int, string> AlphabetTritemy()
        {
            var alf = new Dictionary<int, string>();
            var alf_s = GenForTritemy();
            int index = 1;
            int start = 0;
            int stop = 31;
            StringBuilder str = new StringBuilder();
            do
            {
                for (int i = start; i < start + SymbolsCount; i++)
                {
                    str.Append(alf_s[i % 32]);
                }
                alf[index++] = str.ToString();
                str.Clear();
                start++;
            } while (start <= stop);

            return alf;
        }

        public static char[,] GenerateMatrixAlphabet(string key)
        {
            int ROWS = 5;
            int COLS = 6;
            char[,] matrix = new char[ROWS, COLS];
            string _alf = new string(GenerateAlphabet().Values.ToArray<char>());
            StringBuilder alf_ = new StringBuilder(_alf);
            
            int index = _alf.IndexOf('Ы');

            alf_[index] = 'Ь';
            alf_[index + 1] = 'Ы';

            string alf = alf_.ToString();
            
            alf = alf.Remove(alf.IndexOf('Ъ'), 1).Remove(alf.IndexOf('Й'), 1);
            key = key + alf;
            StringBuilder str = new StringBuilder();
            str.Append(key);
            char[] set = new HashSet<char>(key).ToArray<char>();

            for (int i = 0; i < ROWS; i++)
            {
                for (int j = 0; j < COLS; j++)
                {
                    matrix[i, j] = set[i * COLS + j];
                }
            }

            return matrix;
        }
    }
}

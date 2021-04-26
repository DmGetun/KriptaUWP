using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWP.Помошники;

namespace UWP.Алгоритмы
{
    class Atbash : Algorithm
    {
        public override string Name => "Шифр Атбаш";

        public override string DefaultKey => Algorithm.NonKey;

        public override bool IsReplaceText => true;

        /*
            Шифрование шифром Атбаш. Получаем номер буквы в алфавите, 
            прибавляем единицу т.к. отсчет в массиве начинается с нуля,
            Alphabet.SYMBOLS_COUNT - количество букв в алфавите.
            По формуле находим индекс новой буквы.
        */
        public override string Encrypt(string plainText, Config config)
        {
            StringBuilder str = new StringBuilder();
            var alf = Alphabet.GenerateAlphabet(); // Здесь храним словарь с позицией букв, начиная нумерацию с единицы
            string alf_f = new string(alf.Values.ToArray<char>());
            foreach (char s in plainText)
            {
                int number = alf_f.IndexOf(s) + 1;
                int indexCipherSymbol = alf_f.Length - number + 1;
                str.Append(alf[indexCipherSymbol]);
            }
            return str.ToString();
        }

        /*
            Расшифрование шифром Атбаш. Получаем номер буквы в перевернутом алфавите, 
            прибавляем единицу т.к. отсчет в массиве начинается с нуля,
            Alphabet.SYMBOLS_COUNT - количество букв в алфавите.
            По формуле находим индекс новой буквы.
        */

        public override string Decrypt(string cipherText, Config config)
        {
            StringBuilder str = new StringBuilder();

            var alf = Alphabet.GenerateAlphabet();
            string alf_f = new string(alf.Values.ToArray<char>());
            alf_f = new string(alf_f.ToCharArray().Reverse().ToArray());
            foreach (char s in cipherText)
            {
                int number = alf_f.IndexOf(s) + 1;
                int indexPlainSymbol = alf_f.Length - number;
                str.Append(alf_f[indexPlainSymbol]);
            }
            return str.ToString();
        }

        public override string CheckKey(string key)
        {
             return null;
        }

        public override string KeyView()
        {
            return "";
        }

        public override string GenerateKey()
        {
            return null;        
        }
    }
}

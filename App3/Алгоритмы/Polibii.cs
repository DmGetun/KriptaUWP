using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWP.Помошники;

namespace UWP.Алгоритмы
{
    class Polibii : Algorithm
    {
        public override string Name => "Шифр Полибия";

        public override string DefaultKey => Algorithm.NonKey;

        public override bool IsReplaceText => true;

        public override string Group => "Шифры однозначной замены";

        public override string CheckKey(string key)
        {
            throw new NotImplementedException();
        }

        /*
            Функция расшифрования.
            Генерируем словарь, состоящий из буквы и её индексов в таблице 6 на 6.
            Для числа шифртекста получаем букву, беря её индекс в таблице
        */
        public override string Decrypt(string cipherText, Config config)
        {
            var alf = Alphabet.PolibiiAlphabet();
            StringBuilder str = new StringBuilder();
            string text = Alphabet.CheckText(cipherText);
            string[] text_new = text.Split(' ');
            foreach (string s in text_new)
            {
                char number = Alphabet.GetSymbolPolibii(alf, s);
                str.Append(number);
            }
            return str.ToString();
        }
        /*
            Функция шифрования.
            Генерируем словарь, состоящий из буквы и её индексов в таблице 6 на 6.
            Для буквы текста получаем число, соответствующее номеру строки и столбца в таблице.
        */
        public override string Encrypt(string plainText, Config config)
        {
            var alf = Alphabet.PolibiiAlphabet();
            StringBuilder str = new StringBuilder();
            string text = Alphabet.CheckText(plainText);
            foreach (char s in text)
            {
                string number = alf[s];
                str.Append(number + ' ');
            }
            str = str.Remove(str.Length - 1, 1);
            return str.ToString();
        }

        public override string GenerateKey()
        {
            return null;
        }

        public override string KeyView()
        {
            var alf = Alphabet.PolibiiAlphabet();
            int i = 0;
            StringBuilder str = new StringBuilder();
            foreach(char key in alf.Keys)
            {
                str.Append($"{key}:{alf[key]} ");
                i++;
                if (i % 6 == 0) str.Append("\r");
            }
            return str.ToString();
        }
    }
}

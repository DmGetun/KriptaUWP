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

        public override string CheckKey(string key)
        {
            throw new NotImplementedException();
        }

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
            return null;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWP.Помошники;

namespace UWP.Алгоритмы
{
    class Cezar : Algorithm
    {
        public override string Name => "Шифр Цезаря";

        public override string DefaultKey => "3";

        public override bool IsReplaceText => true;

        public override string Decrypt(string cipherText, Config config)
        {
            StringBuilder str = new StringBuilder();

            string error = CheckKey(config.Key);
            if (error != null) return error; 

            int step = int.Parse(config.Key);
            var alf = Alphabet.GenerateAlphabet();
            string text = Alphabet.CheckText(cipherText);
            foreach (char s in text)
            {
                int number = Alphabet.CheckSymbol(s);
                int position = Alphabet.GetSymbol(alf, s);
                int index = (position - step) % 32;
                if (index <= 0)
                {
                    index = 32 + index;
                }
                str.Append(alf[index]);
            }
            return str.ToString();
        }

        public override string CheckKey(string key)
        {
            int num;
            if (string.IsNullOrEmpty(key))
            {
                throw new Error(Error.MissingKey);
            }
            if (!int.TryParse(key,out num))
            {
                throw new Error(Error.StepNumber);
            }
            if (int.Parse(key) < 0 || int.Parse(key) >= 32)
            {
                throw new Error(Error.LimitKeyCezar);
            }

            return null;
        }

        public override string Encrypt(string plainText, Config config)
        {
            StringBuilder str = new StringBuilder();

            string error = CheckKey(config.Key);
            if (error != null) return error;
            int step = int.Parse(config.Key);
            var alf = Alphabet.GenerateAlphabet();
            string text = Alphabet.CheckText(plainText);
            foreach (char s in text)
            {
                int number = Alphabet.CheckSymbol(s);
                int position = Alphabet.GetSymbol(alf, s);
                int index = (position + step) % 32;
                if (index <= 0)
                {
                    index = 32 + index;
                }
                str.Append(alf[index]);
            }
            return str.ToString();
        }

        public override string KeyView()
        {
            return null;
        }

        public override string GenerateKey()
        {
            return new Random().Next(0, 32).ToString();
        }
    }
}

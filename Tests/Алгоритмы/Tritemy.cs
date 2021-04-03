using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWP.Помошники;

namespace UWP.Алгоритмы
{
    class Tritemy : Algorithm
    {
        public override string Name => "Шифр Тритемия";

        public override string DefaultKey => Algorithm.NonKey;

        public override bool IsReplaceText => true;

        public override string CheckKey(string key)
        {
            return null;
        }

        /*
        * расшифровка.
        * Получаем на вход текст, идем по каждому символу,
        * в таблице Тритемия ищем строку по текущему номеру символа,
        * в ней находим наш зашифрованный символ, по его номеру
        * получаем исходный символ.
        */
        public override string Decrypt(string cipherText, Config config)
        {
            var alf = Alphabet.AlphabetTritemy(); // таблица Тритемия

            var alf_f = alf[1];

            StringBuilder plainText = new StringBuilder();
            for (int i = 0; i < cipherText.Length; i++)
            {
                char symbol = cipherText[i];
                int numberString = i % Alphabet.SYMBOLS_COUNT + 1; // номер строки исходя из ключа
                int index = alf[numberString].IndexOf(symbol);
                char plainSymbol = alf_f[index];
                plainText.Append(plainSymbol);
            }
            return plainText.ToString();
        }
        /*
         * Зашифровка.
         * Получаем на вход текст, идем по каждому символу,
         * в таблице Тритемия находим номер символа в первой строке,
         * получаем номер строки исходя из позиции символа в тексте,
         * получаем зашифрованный символ по номеру строки и позиции открытого символа.
         */
        public override string Encrypt(string plainText, Config config)
        {
            var alf = Alphabet.AlphabetTritemy();
            var alf_f = alf[1];

            StringBuilder cipherText = new StringBuilder();
            for (int i = 0; i < plainText.Length; i++)
            {
                char symbol = plainText[i];
                int index = alf_f.IndexOf(symbol);
                int numberString = i % Alphabet.SYMBOLS_COUNT + 1; // номер строки исходя из ключа
                char cipherSymbol = alf[numberString][index]; // зашифрованный символ по номеру строки и позиции символа
                cipherText.Append(cipherSymbol);
            }
            return cipherText.ToString();
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

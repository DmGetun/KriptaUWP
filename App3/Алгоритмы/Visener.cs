using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWP.Помошники;

namespace UWP.Алгоритмы
{
    class Visener : Algorithm
    {
        public override string Name => "Шифр Виженера";

        public override string DefaultKey => "А";

        public override bool IsReplaceText => true;

        public override string CheckKey(string key)
        {
            if (key.Length != 1) return Error.KeySingleCharacter;
            return null;
        }

        /*
            расшифровка Белазо
            Находим номер строки исходя из символа ключа.
            Получаем номер зашифрованного символа в этой строке.
            По номеру зашифрованного символа находим открытый символ в обычном алфавите.
        */
        public override string Decrypt(string cipherText, Config config)
        {
            var alf = Alphabet.AlphabetTritemy();

            var alf_f = alf[1];
            string error = CheckKey(config.Key.ToUpper());
            if (error != null) return error;

            char plainKeySymbol = Convert.ToChar(config.Key.ToUpper());

            StringBuilder plainText = new StringBuilder();
            for (int i = 0; i < cipherText.Length; i++)
            {
                char symbol = cipherText[i];
                int indexKeySymbol = alf_f.IndexOf(plainKeySymbol); // Номер символа ключа (номер строки)
                int indexCipherSymbol = alf[indexKeySymbol + 1].IndexOf(symbol); // номер зашифрованного символа в строке ключа
                char plainSymbol = alf_f[indexCipherSymbol];
                plainText.Append(plainSymbol);
                plainKeySymbol = plainSymbol; // используем найденный открытый символ как следующий символ ключа.
            }
            return plainText.ToString();
        }

        /*
            Зашифровка Белазо
            Формируем Таблицу исходя из символов открытого текста.
            Таблица представляет из себя последовательно идущие строки,
            где каждая следующая строка находится по первому символу текущего символа ключа.
            Получаем номер открытого символа в обычном алфавите.
            Получаем номер строки исходя из номера символа в тексте.
            Получаем зашифрованный символ по номеру строки и номеру открытого символа.
         */

        public override string Encrypt(string plainText, Config config)
        {
            var alf = Alphabet.AlphabetTritemy();
            var alf_f = alf[1];
            string error = CheckKey(config.Key.ToUpper());
            if (error != null) return error;
            string newSymbol = config.Key.ToUpper();
            string key = newSymbol + plainText;
            var keyAlf = Alphabet.GenerateKeyAlphabet(key, alf);

            StringBuilder cipherText = new StringBuilder();
            for (int i = 0; i < plainText.Length; i++)
            {
                char symbol = plainText[i];
                int index = alf_f.IndexOf(symbol);
                int numberString = i % key.Length; // номер строки исходя из ключа
                char cipherSymbol = keyAlf[numberString][index];
                cipherText.Append(cipherSymbol);
            }
            return cipherText.ToString();
        }

        public override string GenerateKey()
        {
            char[] alf = Alphabet.GenerateAlphabet().Values.ToArray();

            return alf[new Random().Next(0, 32)].ToString();
        }

        public override string KeyView()
        {
            return null;
        }
    }
}

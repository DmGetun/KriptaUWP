using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWP.Помошники;

namespace UWP.Алгоритмы
{
    class Belazo : Algorithm
    {
        public override string Name => "Шифр Белазо";

        public override string DefaultKey => "ЗОНД";

        public override bool IsReplaceText => true;

        private string Key;

        public override string CheckKey(string key)
        {
            foreach(char s in key)
            {
                if(!(s >= 'А' && s <= 'Я') && !(s >='а' && s <= 'я'))
                {
                    throw new Error(Error.InvalidKey);
                }
            }
            Key = key;
            return null;
        }

        /*
        Расшифрование шифром Белазо
        Выбираем из таблицы Тритемия только те строки, 
        у которых первые символы совпадают с символами ключа.
        Вычисляем номер строки в таблице по номеру буквы в строке.
        Находим номер зашифрованного символа в строке.
        По этому номеру получаем исходный символ.
        */
        public override string Decrypt(string cipherText, Config config)
        {
            var alf = Alphabet.AlphabetTritemy();
            string key = config.Key;

            string error = CheckKey(key);

            var keyAlf = Alphabet.GenerateKeyAlphabet(key, alf);
            string alf_f = alf[1];

            StringBuilder plainText = new StringBuilder();
            for (int i = 0; i < cipherText.Length; i++)
            {
                char symbol = cipherText[i];
                int numberString = i % key.Length; // Номер строки в ключе
                int index = keyAlf[numberString].IndexOf(symbol);
                char plainSymbol = alf_f[index];
                plainText.Append(plainSymbol);
            }

            return plainText.ToString();
        }

        /*
            Шифрование шифром Белазо
            Выбираем из таблицы Тритемия только те строки, 
            у которых первые символы совпадают с символами ключа.
            Вычисляем номер открытого символа в первой строке таблицы.
            Находим номер строки, в которой будем искать зашифрованный символ.
            По номеру строки и номеру открытого символа получаем зашифрованный символ.
         */

        public override string Encrypt(string plainText, Config config)
        {
            var alf = Alphabet.AlphabetTritemy();
            string key = config.Key;

            string error = CheckKey(key);

            var keyAlf = Alphabet.GenerateKeyAlphabet(key, alf);
            var alf_f = keyAlf[1];

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
            return new Plaifer().GenerateKey();
        }

        public override string KeyView()
        {
            return Key;
        }
    }
}

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

        public override string Group => "Шифры однозначной замены";
        /*
            Функция расшифрования.
            Получить индекс буквы в алфавите,
            отнять от индекса текущий шаг и взять модуль от разности.
            По полученному числу получить букву из алфавита.
        */
        public override string Decrypt(string cipherText, Config config)
        {
            StringBuilder str = new StringBuilder();

            CheckKey(config.Key);

            int step = int.Parse(config.Key);
            var alf = Alphabet.GenerateAlphabet();
            string text = Alphabet.CheckText(cipherText);
            foreach (char s in text)
            {
                int position = Alphabet.GetSymbol(alf, s);
                int index = (position - step) % alf.Count;
                if (index <= 0)
                    index += alf.Count;
                str.Append(alf[index]);
            }
            return str.ToString();
        }
        /*
            Функция шифрования.
            Получить индекс буквы в алфавите,
            прибавить к индексу текущий шаг и взять модуль от суммы.
            По полученному числу получить букву из алфавита.
        */
        public override string Encrypt(string plainText, Config config)
        {
            StringBuilder str = new StringBuilder();

            CheckKey(config.Key);
            int step = int.Parse(config.Key);
            var alf = Alphabet.GenerateAlphabet();
            string text = Alphabet.CheckText(plainText);
            foreach (char s in text)
            {
                int position = Alphabet.GetSymbol(alf, s);
                int index = (position + step) % alf.Count;
                if (index <= 0)
                    index += alf.Count;
                
                str.Append(alf[index]);
            }
            return str.ToString();
        }
        /*
            Проверка ключа.
            Если ключа нет, ключ не число или ключ меньше 0 или больше 32,
            то ошибка.
        */
        public override string CheckKey(string key)
        {
            int num;
            if (string.IsNullOrEmpty(key))
            {
                throw new Error(Error.MissingKey);
            }
            if (!int.TryParse(key, out num))
            {
                throw new Error(Error.StepNumber);
            }
            if (int.Parse(key) < 0 || int.Parse(key) >= 32)
            {
                throw new Error(Error.LimitKeyCezar);
            }

            return null;
        }

        public override string KeyView()
        {
            return "";
        }

        public override string GenerateKey()
        {
            return new Random().Next(0, 32).ToString();
        }
    }
}

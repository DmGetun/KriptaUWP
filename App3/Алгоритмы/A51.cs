using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWP.Помошники;

namespace UWP.Алгоритмы
{
    class A51 : Algorithm
    {
        public override string Name => "Шифр A5/1";

        public override string DefaultKey => "1kkk";

        public override bool IsReplaceText => false;

        public override string Group => "Поточные шифры";

        BitArray R1 = new BitArray(19);
        BitArray R2 = new BitArray(22);
        BitArray R3 = new BitArray(23);

        private int bitCount = 228;
        private static byte[] Key;
        byte[] encrypt = null;
        bool flagD = false;

        enum Registers
        {
            first,
            second,
            third
        };
        /*
            Функция проверки ключа.
            Проверяем ключ на длину в 8 байт,
            выводим сообщение в случае несоответствия.
        */
        public override string CheckKey(string key)
        {
            byte[] k = Encoding.Unicode.GetBytes(key);
            if (k.Length != 8)
            {
                throw new Error(Error.KeyLength64bit);
            }
            return key;
        }
        /*
            Расшифрование A5/1
            Функция аналогична функции шифрования.
         */
        public override string Decrypt(string cipherText, Config config)
        {
            flagD = true;
            return Encrypt(cipherText, config);
        }
        /*
            Фукнция шифрования A5/1
            Получаем ключ и текст, проверяем ключ.
            Конвертируем в массив байтов, высчитываем количество фреймов.
            Для каждого фрейма генерируем гамму, складываем биты текста с битами гаммы по модулю 2,
            получившийся массив бит конвертируем в строку.
        */
        public override string Encrypt(string plainText, Config config)
        {
            string k = CheckKey(config.Key);
            byte[] _key = Encoding.Unicode.GetBytes(k);
            Key = _key;
            byte[] message = Encoding.Unicode.GetBytes(plainText);
            if (flagD == true && encrypt != null)
            {
                message = encrypt;
                encrypt = null;
                flagD = false;
            }

            BitArray msg = new BitArray(message);
            BitArray key = new BitArray(_key);

            int frameCount = msg.Length / bitCount;
            if (msg.Length % bitCount != 0)
            {
                frameCount++;
            }

            int[] frame = new int[1] { 0x134 };
            BitArray res = new BitArray(msg.Length);
            for (int i = 0; i < frameCount; i++)
            {
                frame[0] = i;
                Initialize(key, frame);
                BitArray stream = GetStream();
                
                for (int j = 0; j < bitCount; j++)
                {
                    if (i * bitCount + j == msg.Count) break;
                    int index = i * bitCount + j;
                    bool value = stream[j] ^ msg[i * bitCount + j];
                    res.Set(index, value);
                }
            }
            flagD = false;
            return GetString(res);
        }

        /*
            Функция конвертации массива бит в строку.
            Массив бит конвретируем в массив байт,
            массив байт конвертируем в строку.
        */
        private string GetString(BitArray arr)
        {
            byte[] ret = new byte[(arr.Length - 1) / 8 + 1];
            arr.CopyTo(ret, 0);
            encrypt = ret;
            return Encoding.Unicode.GetString(ret);
        }

        /*
            Функция инициализации ключа.
            Заполняем все регистры нулями,
            делаем 64 такта со сдвигами регистров и сложением нулевого бита регистра с битом ключа,
            делаем 22 такта со сдвигами регистров и сложением нулевого бита регистра с битом пакета,
            делаем 100 тактов со сдвигами регистров на основе мажоритарной функции.       
        */
        private void Initialize(BitArray key, int[] frame)
        {
            for (int i = 0; i < 19; i++)
                R1[i] = false;
            for (int i = 0; i < 22; i++)
                R2[i] = false;
            for (int i = 0; i < 23; i++)
                R3[i] = false;

            BitArray frameBits = new BitArray(frame);
            for (int i = 0; i < 64; i++)
            {
                ClockAll();
                R1[0] ^= key[i];
                R2[0] ^= key[i];
                R3[0] ^= key[i];
            }

            for (int i = 0; i < 22; i++)
            {
                ClockAll();
                R1[0] ^= frameBits[i];
                R2[0] ^= frameBits[i];
                R3[0] ^= frameBits[i];
            }

            for (int i = 0; i < 100; i++)
            {
                Clock();
            }
        }
        /*
            Функция получения гаммы.
            Выполняем сдвиг регистров на основе мажоритарной функции,
            Выходные биты каждого регистра складываем по модулю 2.
         */
        private BitArray GetStream()
        {
            BitArray part = new BitArray(bitCount);
            for (int i = 0; i < bitCount; i++)
            {
                Clock();
                part[i] = R1[18] ^ R2[21] ^ R3[22];
            }
            return part;
        }

        /*
            Функция сдвига регистров на основе мажоритарной функции.
            Вычисляем мажоритарную фукнцию,
            сдвигаем только те регистры, бит синхронизации которых равен мажоритарной функции
        */
        private void Clock()
        {
            bool majority = Majority();
            if (R1[8] == majority)
                ClockFull(R1, Registers.first);

            if (R2[10] == majority)
                ClockFull(R2, Registers.second);

            if (R3[10] == majority)
                ClockFull(R3, Registers.third);
        }
        /*
            Функция сдвига всех регистров без учета мажоритарной функции. 
        */
        private void ClockAll()
        {
            R1 = ClockFull(R1, Registers.first);
            R2 = ClockFull(R2, Registers.second);
            R3 = ClockFull(R3, Registers.third);
        }
        /*
            Функция сдвига регистра.
            Получаем временный бит, который равен сложению по модулю 2 определенных битов регистра,
            Сдвигаем регистр вправо на 1, на нулевую позицию регистра ставим временный бит.
        */
        private BitArray ClockFull(BitArray RegOne, Registers register)
        {
            bool temp = false;
            switch (register)
            {
                case Registers.first:
                    temp = RegOne[13] ^ RegOne[16] ^ RegOne[17] ^ RegOne[18];
                    break;
                case Registers.second:
                    temp = RegOne[20] ^ RegOne[21];
                    break;
                case Registers.third:
                    temp = RegOne[7] ^ RegOne[20] ^ RegOne[21] ^ RegOne[22];
                    break;
            }
            RegOne = RegOne.LeftShift(1);
            RegOne[0] = temp;
            return RegOne;
        }

        // вычисление мажоритарной функции
        private bool Majority()
        {
            return (R1[8] & R2[10]) | (R1[8] & R3[10]) | (R2[10] & R3[10]);
        }
        /*
            Показать двоичный вид ключа  
        */
        public override string KeyView()
        {
            BitArray arr = new BitArray(Key);
            StringBuilder rez = new StringBuilder();
            foreach(bool b in arr)
            {
                int bit = b ? 1 : 0;
                rez.Append(bit);
            }
            return rez.ToString();
        }
        /*
            Сгенерировать ключ длиной 64 бита 
        */
        public override string GenerateKey()
        {
            BitArray arr = new BitArray(64);
            var rand = new Random();
            for (int i = 0; i < arr.Length; i++)
            {
                bool value = false;
                if (rand.Next(0, 2) == 1)
                    value = true;
                arr.Set(i, value);
            }
            return GetString(arr);
        }

    }
}

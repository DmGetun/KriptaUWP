using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWP.Помошники;

namespace UWP.Алгоритмы
{
    class A52 : Algorithm
    {
        public override string Name => "Шифр A5/2";

        public override string DefaultKey => "1kkk";

        public override bool IsReplaceText => false;

        public override string Group => "Поточные шифры";

        private byte[] Key;

        BitArray R1 = new BitArray(19);
        BitArray R2 = new BitArray(22);
        BitArray R3 = new BitArray(23);
        BitArray R4 = new BitArray(17);

        private int bitCount = 114;

        private byte[] encrypt = null;
        private bool flagD = false;

        enum Registers
        {
            first,
            second,
            third,
            fourth
        };
        /*
            Функция проверки ключа на длину в 8 байт 
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
            Расшифрование A5/2.
            Функция аналогична функции шифрования
        */
        public override string Decrypt(string cipherText, Config config)
        {
            flagD = true;
            return Encrypt(cipherText, config);
        }

        /*
            Получаем ключ и текст, проверяем ключ.
            Превращаем текст и ключ в массив байт,
            Считаем количество фреймов длиной 114 бит,
            В цикле по количеству фреймов инициализируем ключ,
            получаем поток гаммы,
            суммируем гамму и открытый текст по модулю 2.
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

            int[] frame = new int[1] { 0x21 };
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
            return GetString(res);
        }

        //превращаем массив бит в строку.
        private string GetString(BitArray arr)
        {
            byte[] bytes = new byte[Convert.ToInt32(Math.Ceiling(arr.Count / 8.0))];
            arr.CopyTo(bytes, 0);
            if(flagD == false) encrypt = bytes;
            return Encoding.Unicode.GetString(bytes);
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
            for (int i = 0; i < 17; i++)
                R4[i] = false;

            BitArray frameBits = new BitArray(frame);
            for (int i = 0; i < 64; i++)
            {
                ClockAll();
                R1[0] ^= key[i];
                R2[0] ^= key[i];
                R3[0] ^= key[i];
                R4[0] ^= key[i];
            }

            for (int i = 0; i < 22; i++)
            {
                ClockAll();
                R1[0] ^= frameBits[i];
                R2[0] ^= frameBits[i];
                R3[0] ^= frameBits[i];
                R4[0] ^= frameBits[i];
            }

            for (int i = 0; i < 100; i++)
            {
                Clock();
            }
        }
        /*
            Функция получения гаммы.
            Выполняем сдвиг регистров на основе мажоритарной функции,
            Выходные биты каждого регистра и значение мажоритарной функции
            от трех определенных битов каждого регистра складываем по модулю 2.
        */
        private BitArray GetStream()
        {
            BitArray part = new BitArray(bitCount);
            for (int i = 0; i < bitCount; i++)
            {
                Clock();
                bool bit1 = MajorityForBits(R1[12], R1[14], R1[15]);
                bool bit2 = MajorityForBits(R2[9], R2[13], R2[16]);
                bool bit3 = MajorityForBits(R3[13], R3[16], R3[18]);

                part[i] = bit1 ^ R1[18] ^ bit2 ^ R2[21] ^ bit3 ^ R3[22];
            }
            return part;
        }
        /*
            Функция сдвига регистров на основе мажоритарной функции.
            Вычисляем мажоритарную фукнцию,
            сдвигаем 3 регистра по условию равенства одного из битов R4 мажоритарному,
            сдвигаем 4 регистр.
        */
        private void Clock()
        {
            bool majority = Majority();
            if (R4[10] == majority)
                ClockFull(R1, Registers.first);

            if (R4[3] == majority)
                ClockFull(R2, Registers.second);

            if (R4[7] == majority)
                ClockFull(R3, Registers.third);

            ClockFull(R4, Registers.fourth);
        }

        //Функция сдвига всех регистров без учета мажоритарной функции. 
        private void ClockAll()
        {
            ClockFull(R1, Registers.first);
            ClockFull(R2, Registers.second);
            ClockFull(R3, Registers.third);
            ClockFull(R4, Registers.fourth);
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
                case Registers.fourth:
                    temp = RegOne[11] ^ RegOne[16];
                    break;
            }
            RegOne = RegOne.LeftShift(1);
            RegOne[0] = temp;
            return RegOne;
        }

        // вычисление мажоритарной функции трех битов одного регистра
        private bool MajorityForBits(bool b1, bool b2, bool b3)
        {
            return (b1 & b2) | (b1 & b3) | (b2 & b3);
        }

        // вычисление мажоритарной функции
        private bool Majority()
        {
            return (R4[3] & R4[7]) | (R4[3] & R4[10]) | (R4[7] & R4[10]);
        }
        /*
            Показать двоичный вид ключа  
        */
        public override string KeyView()
        {
            BitArray arr = new BitArray(Key);
            StringBuilder rez = new StringBuilder();
            foreach (bool b in arr)
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
            for(int i = 0; i < arr.Length; i++)
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

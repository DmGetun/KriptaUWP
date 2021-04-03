using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using UWP.Помошники;

namespace UWP.Алгоритмы
{
    class GammaGost28147 : Algorithm
    {
        byte[][] table = new byte[8][] {
        new byte[] {12, 4, 6, 2, 10, 5, 11, 9, 14, 8, 13, 7, 0, 3, 15, 1 },
        new byte[] {6, 8, 2, 3, 9, 10, 5, 12, 1, 14, 4, 7, 11, 13, 0, 15 },
        new byte[] {11, 3, 5, 8, 2, 15, 10, 13, 14, 1, 7, 4, 12, 9, 6, 0 },
        new byte[] {12, 8, 2, 1, 13, 4, 15, 6, 7, 0, 10, 5, 3, 14, 9, 11 },
        new byte[] {7, 15, 5, 10, 8, 1, 6, 13, 0, 9, 3, 14, 11, 4, 2, 12 },
        new byte[] {5, 13, 15, 6, 9, 2, 12, 10, 11, 7, 8, 1, 4, 3, 14, 0 },
        new byte[] {8, 14, 2, 5, 6, 9, 1, 12, 15, 4, 11, 0, 13, 10, 3, 7 },
        new byte[] {1, 7, 14, 13, 0, 5, 8, 3, 4, 15, 10, 6, 9, 12, 11, 2 } };
        public override string Name => "Гаммирование ГОСТ 28147-89";

        public override string DefaultKey => "1234567890ABCDEF";

        public override bool IsReplaceText => false;

        public byte[] EncryptBlocks { get; private set; }
        bool flagD = false;

        private string Key;

        private readonly int BLOCK_SIZE = 8;

        private const uint C2 = 0x_1010101;
        private const uint C1 = 0x_1010104;
        private const uint s1 = 0x_1010104;
        private const uint s2 = 0x_1010104;

        private uint N1, N2, N3, N4;

        /*
            Проверка ключа на длину в 256 бит 
        */
        public override string CheckKey(string key)
        {
            byte[] arr = Encoding.Unicode.GetBytes(key);
            if (arr.Length * 8 != 256)
            {
                throw new Error(Error.KeyLength32Byte);
            }
            return key;
        }

        /*
            Функция расшифрования. 
            Аналогична функции шифрования.
        */

        public override string Decrypt(string cipherText, Config config)
        {
            flagD = true;
            string key = CheckKey(config.Key);
            Key = key;
            byte[] text = Encoding.Unicode.GetBytes(cipherText);
            if (flagD == true && EncryptBlocks != null)
            {
                text = EncryptBlocks;
                EncryptBlocks = null;
                flagD = false;
            }
            byte[][] allBlocks = GiveAllBlocks(text);
            uint[] keys = GenerateKeys(key);
            N1 = s1;
            N2 = s2;
            byte[] block = new byte[BLOCK_SIZE];
            byte[] plainBlocks = new byte[text.Length];
            int len = text.Length / 8;
            if (text.Length % 8 != 0) len++;
            for (int i = 0; i < len; i++)
            {
                var syncro = GenerateSynhro(keys);
                block = XOR(syncro, allBlocks[i]);
                for (int j = 0; j < block.Length; j++)
                    plainBlocks[8 * i + j] = block[j];
            }

            flagD = false;
            return Encoding.Unicode.GetString(plainBlocks);
        }

        /*
            Разбить текст на блоки.
            Если можем записать полный блок, то пишем полный.
            Иначе последний блок оставляем не полным.
        */
        private byte[][] GiveAllBlocks(byte[] text)
        {
            int len = text.Length / 8 + 1;
            int j = 0,i = 0;
            byte[][] res = new byte[len][];
            for(; j < len; j++)
            {
                if(j * 8 + 8 < text.Length)
                {
                    res[j] = new byte[8];
                    for(; i < j * 8 + 8; i++)
                    {
                        res[j][i % 8] = text[i];
                    }
                }
                else
                {
                    res[j] = new byte[text.Length - (j * 8)];
                    for(int l = 0;l < text.Length - (j * 8); l++)
                    {
                        res[j][l] = text[i++];
                    }
                }
            }
            return res;
        }
        /*
            Операция XOR над массивом байт.
            Складываем каждый байт входный массивом по модулю 2.
        */
        private byte[] XOR(byte[] syncro, byte[] block)
        {
            byte[] buffer = new byte[block.Length];
            for(int i = 0; i < block.Length; i++)
            {
                buffer[i] = (byte)(syncro[i] ^ block[i]);
            }
            return buffer;
        }

        /*
            Генерация гаммы.
            Записываем значения из N1,N2 в массив байт,
            полученный массив зашифровываем в режиме простой замены.
            Полученный массив переписываем в N3,N4
            Суммируем N3 и C2 по модулю 2^32
            Суммируем N4 и C1 по модулю 2^32 - 1
            Записываем N3 в N1, N4 в N2
            Переводим N1 и N2 в массив байт.
            Полученный массив шифруем в режиме простой замены
        */

        private byte[] GenerateSynhro(uint[] keys)
        {
            byte[] block = new byte[8];
            Array.Copy(BitConverter.GetBytes(N1), 0, block, 0, 4);
            Array.Copy(BitConverter.GetBytes(N2), 0, block, 4, 4);
            byte[] result = SimpleReplacement(block, keys);
            N3 = BitConverter.ToUInt32(result, 0);
            N4 = BitConverter.ToUInt32(result, 4);
            N3 = (uint)((N3 + C2) % Math.Pow(2, 32));
            N4 = (uint)((N4 + C1) % (Math.Pow(2, 32) - 1));
            N1 = N3;
            N2 = N4;
            Array.Copy(BitConverter.GetBytes(N1), 0, block, 0, 4);
            Array.Copy(BitConverter.GetBytes(N2), 0, block, 4, 4);
            return SimpleReplacement(block, keys);
        }

        /*
            Функция шифрования.
            Проверяем ключ, разбиваем текст на блоки.
            Генерируем ключи (С 1 по 24 записываем K0...K7, с 25 по 32 записываем K7...K0)
            Записываем синхропосылки в N1,N2
            Генерируем гамму, складываем её по модулю 2 с блоком открытого текста
            Добавляем к массиву зашифрованных байт
        */

        public override string Encrypt(string plainText, Config config)
        {
            string key = CheckKey(config.Key);
            Key = key;
            byte[] text = Encoding.Unicode.GetBytes(plainText);
            byte[][] allBlocks = GiveAllBlocks(text);
            uint[] keys = GenerateKeys(key);
            N1 = s1;
            N2 = s2;
            byte[] block = new byte[BLOCK_SIZE];
            int len = text.Length / 8;
            if (text.Length % 8 != 0) len++;
            byte[] cipherBlocks = new byte[text.Length];
            for (int i = 0; i < len; i++)
            {
                var syncro = GenerateSynhro(keys);
                block = XOR(syncro, allBlocks[i]);
                for (int j = 0; j < block.Length; j++)
                    cipherBlocks[8 * i + j] = block[j];
            }
            

            EncryptBlocks = cipherBlocks;

            return Encoding.Unicode.GetString(cipherBlocks);
        }

        /*
            Режим простой замены.
            Разбиваем 64 битный массив на два 32 битных.
            Суммируем N1 и ключ по модулю 2^32 в сумматоре CM1
            Заменяем значение из CM1 по таблице замен
            Сдвигаем результат циклическим сдвигом влево на 11
            Полученное значение складываем с N2 по модулю 2 в сумматоре CM2
            Записываем N1 в N2, CM2 в N1.
            На последней итерации пишем CM2 N2
        */
        private byte[] SimpleReplacement(byte[] block, uint[] keys)
        {
            byte[] N1 = new byte[block.Length / 2];
            byte[] N2 = new byte[block.Length / 2];
            Array.Copy(block, 0, N1, 0, 4);
            Array.Copy(block, 4, N2, 0, 4);

            for (int i = 0; i < 32; i++)
            {
                uint CM1 = (uint)((BitConverter.ToUInt32(N1, 0) + keys[i]) % Math.Pow(2, 32));
                byte[] K = Replfacement(CM1);
                uint R = BitConverter.ToUInt32(K, 0);
                R = (R << 11) | (R >> 21);
                uint CM2 = R ^ BitConverter.ToUInt32(N2, 0);
                if (i < 31)
                {
                    N2 = N1;
                    N1 = BitConverter.GetBytes(CM2);
                }
                else
                    N2 = BitConverter.GetBytes(CM2);
            }

            var output = new byte[8];

            for (int i = 0; i < 4; i++)
            {
                output[i] = N1[i];
                output[4 + i] = N2[i];
            }

            return output;
        }

        /*
            Замена 32 битного блока по таблице
            Каждый раз сдвигаем блок на 4 вправо,
            полученный число используем как индекс в таблице замен.
        */
        private byte[] Replfacement(uint cM1)
        {

            uint result = 0;
            for (int i = 0; i < 8; i++)
            {
                var temp = (byte)((cM1 >> (4 * i)) & 0x0f);
                temp = table[i][temp];
                result |= (UInt32)temp << (4 * i);
            }
            return BitConverter.GetBytes(result);
        }

        private uint[] GenerateKeys(string key)
        {
            byte[] keys = Encoding.Unicode.GetBytes(key);
            uint[] result = new uint[keys.Length];
            for (int i = 0; i < 24; i++)
            {
                int index = (i * 4) % 32;
                result[i] = BitConverter.ToUInt32(keys, index);
            }

            for (int i = 24; i < 32; i++)
            {
                result[i] = BitConverter.ToUInt32(keys, 28 - (i * 4) % 32);
            }

            return result;
        }

        public override string GenerateKey()
        {
            byte[] rez = new byte[32];

            for (int i = 0; i < 32; i++)
            {
                new Random().NextBytes(rez);
            }

            return Encoding.Unicode.GetString(rez);
        }

        public override string KeyView()
        {
            byte[] key = Encoding.Unicode.GetBytes(Key);
            return string.Join(" ", key.Select(x => Convert.ToString(x, 2)));
        }
    }
}

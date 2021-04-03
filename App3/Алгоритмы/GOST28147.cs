using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWP.Помошники;

namespace UWP.Алгоритмы
{
    class GOST28147 : Algorithm
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
        public override string Name => "ГОСТ 28147-89";

        public override string DefaultKey => "1234567890ABCDEF";

        public override bool IsReplaceText => false;

        public byte[] EncryptBlocks { get; private set; }
        public byte[] CurrentImito { get; private set; }
        public byte[] ImitoBeforeDecrypt { get; private set; }
        public byte L { get; private set; }

        bool flagD = false;

        private string Key;

        private const uint s1 = 0x_1010104;
        private const uint s2 = 0x_1010104;

        private uint N1, N2;

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
            Аналогична функции шифрования 
            за исключением удаление имитовставки в открытом тексте.
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
            text = DeleteImito(text);
            byte[][] allBlocks = GiveAllBlocks(text);
            uint[] keys = GenerateKeys(key);
            N1 = s1;
            N2 = s2;
            byte[] plainBlocks = new byte[text.Length];
            int len = text.Length / 8;
            if (text.Length % 8 != 0) len++;
            for (int i = 0; i < len; i++)
            {
                var block = GenerateSynhro(allBlocks[i],keys,true);
                for (int j = 0; j < block.Length; j++)
                    plainBlocks[8 * i + j] = block[j];
            }
            CheckImito(plainBlocks,keys);
            flagD = false;
            return Encoding.Unicode.GetString(plainBlocks);
        }
        /*
            Проверка имитовставки.
            Разбиваем открытый текст на блоки и высчитываем значение имитовставки.
            Если имитовставка не совпадает с той, которая пришла с открытым текстом
            выводим ошибку.
        */
        private void CheckImito(byte[] plainBlocks, uint[] keys)
        {
            byte[][] allBlocks = GiveAllBlocks(plainBlocks);
            var imito = GiveImito(allBlocks, keys);

            for(int i = imito.Length - L - 1,j = 0;i < ImitoBeforeDecrypt.Length; i++,j++)
            {
                if(imito[i] != ImitoBeforeDecrypt[j])
                    throw new Error("Имитовставка не совпадает.");
            }
        }
        /*
            Функция удаления имитовставки.
            Получаем значение последнего байта зашифрованного массива,
            которое показывает длину имитовставки.
            Удаляем из текста указанное количество байт с конца.
        */
        private byte[] DeleteImito(byte[] text)
        {
            L = text[text.Length - 1];
            byte[] buffer = new byte[text.Length - L - 1];
            ImitoBeforeDecrypt = new byte[L];
            for(int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = text[i];
            }
            for(int i = text.Length - L - 1,j=0;i < text.Length - 1; i++,j++)
            {
                ImitoBeforeDecrypt[j] = text[i];
            }
            return buffer;
        }
        /*
            Функция разбиения массива байт на блоки по 64 бита.
            Все блоки за исключением последнего заполняются полностью из массива,
            последний может быть не полным.
        */
        private byte[][] GiveAllBlocks(byte[] text)
        {
            int len = text.Length / 8 + 1;
            int j = 0, i = 0;
            byte[][] res = new byte[len][];
            for (; j < len; j++)
            {
                if (j * 8 + 8 < text.Length)
                {
                    res[j] = new byte[8];
                    for (; i < j * 8 + 8; i++)
                    {
                        res[j][i % 8] = text[i];
                    }
                }
                else
                {
                    res[j] = new byte[text.Length - (j * 8)];
                    for (int l = 0; l < text.Length - (j * 8); l++)
                    {
                        res[j][l] = text[i++];
                    }
                }
            }
            return res;
        }
        /*
            Сложение по модулю 2 массивов байт 
        */
        private byte[] XOR(byte[] syncro, byte[] block)
        {
            byte[] buffer = new byte[block.Length];
            for (int i = 0; i < block.Length; i++)
            {
                buffer[i] = (byte)(syncro[i] ^ block[i]);
            }
            return buffer;
        }
        /*
            Функция выработки гаммы.
            Получаем массив байт, шифруем в режиме простой замены
            в 32 раунда, полученный блок складываем по модулю 2 
            с блоком открытого текста.
        */
        private byte[] GenerateSynhro(byte[] vs, uint[] keys,bool flagDecrypt)
        {
            byte[] block = new byte[8];
            Array.Copy(BitConverter.GetBytes(N1), 0, block, 0, 4);
            Array.Copy(BitConverter.GetBytes(N2), 0, block, 4, 4);
            byte[] result = SimpleReplacement(block, keys, 32);

            block = result;

            var gamma = XOR(block, vs);
            if (gamma.Length != 8) return gamma;
            if (!flagDecrypt)
            {
                N1 = BitConverter.ToUInt32(gamma, 0);
                N2 = BitConverter.ToUInt32(gamma, 4);
            }
            else
            {
                N1 = BitConverter.ToUInt32(vs, 0);
                N2 = BitConverter.ToUInt32(vs, 4);
            }
            return gamma;
        }
        /*
            Функция шифрования.
            Получаем на вход текст и ключ,
            проверяем ключ.
            Переводим текст и ключ в массив байт.
            Формируем 32 ключа из 8 исходных,
            генерируем имитовставку. Записываем синхропосылку в N1,N2.
            В цикле по количеству блоком каждый блок 
            складываем с гаммой по модулю 2,результат пишем в результирующий массив
            Добавляем имитовставку, переводим массив байт в текст.
        */
        public override string Encrypt(string plainText, Config config)
        {
            string key = CheckKey(config.Key);
            Key = key;
            byte[] text = Encoding.Unicode.GetBytes(plainText);
            byte[][] allBlocks = GiveAllBlocks(text);
            uint[] keys = GenerateKeys(key);
            byte[] imitoV = GiveImito(allBlocks, keys);
            N1 = s1;
            N2 = s2;
            int len = text.Length / 8;
            if (text.Length % 8 != 0) len++;
            byte[] cipherBlocks = new byte[text.Length];
            for (int i = 0; i < len; i++)
            {
                var block = GenerateSynhro(allBlocks[i],keys,false);
                for (int j = 0; j < block.Length; j++)
                    cipherBlocks[8 * i + j] = block[j];
            }

            cipherBlocks = AddImito(cipherBlocks);
            EncryptBlocks = cipherBlocks;

            return Encoding.Unicode.GetString(cipherBlocks);
        }
        /*
            Функция добавления имитовставки.
            Переписываем весь зашифрованный массив во временный,
            доабвляем к нему имитовставку и в последнюю позицию
            добавляем число, означающее длину имитовставки.
        */
        private byte[] AddImito(byte[] cipherBlocks)
        {
            byte L = 5;
            byte[] result = new byte[cipherBlocks.Length + L + 1];
            int i = 0;
            for(; i < cipherBlocks.Length; i++)
            {
                result[i] = cipherBlocks[i];
            }
            for(int j = CurrentImito.Length - L - 1;i < cipherBlocks.Length + L; i++,j++)
            {
                result[i] = CurrentImito[j];
            }
            result[result.Length - 1] = L;
            return result;
        }
        /*
            Функция вычисления имитовставки.
            В цикле по кждому блоку шифруем открытый блок
            в режиме простой замены в 16 раундов,
            складываем по модулю 2 с предыдущим блоком.
        */
        private byte[] GiveImito(byte[][] allBlocks, uint[] keys)
        {
            CurrentImito = null;
            for(int i = 0; i < allBlocks.GetLength(0); i++)
            {
                byte[] buffer = new byte[8];
                if (allBlocks[i].Length != 8)
                {
                    int j = 0;
                    for (; j < allBlocks[i].Length; j++)
                    {
                        buffer[j] = allBlocks[i][j];
                    }
                    for (; j < buffer.Length; j++)
                    {
                        buffer[j] = 0;
                    }
                }
                else
                    buffer = allBlocks[i];
                var temp = SimpleReplacement(buffer, keys, 16);
                if(i > 0)
                    CurrentImito = XOR(CurrentImito,temp);
                else
                {
                    CurrentImito = temp;
                }
            }

            return CurrentImito;
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
        private byte[] SimpleReplacement(byte[] block, uint[] keys,int round)
        {
            byte[] N1 = new byte[block.Length / 2];
            byte[] N2 = new byte[block.Length / 2];
            Array.Copy(block, 0, N1, 0, 4);
            Array.Copy(block, 4, N2, 0, 4);

            for (int i = 0; i < round; i++)
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

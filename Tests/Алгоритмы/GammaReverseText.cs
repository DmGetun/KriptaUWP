using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWP.Алгоритмы;
using UWP.Помошники;

namespace Tests.Алгоритмы
{
    class GammaReverseText : Algorithm
    {
        private readonly byte[][] table =
{
            //            0     1     2     3     4     5     6     7     8     9     A     B     C     D     E     F
            new byte[] { 0x0C, 0x04, 0x06, 0x02, 0x0A, 0x05, 0x0B, 0x09, 0x0E, 0x08, 0x0D, 0x07, 0x00, 0x03, 0x0F, 0x01 },
            new byte[] { 0x06, 0x08, 0x02, 0x03, 0x09, 0x0A, 0x05, 0x0C, 0x01, 0x0E, 0x04, 0x07, 0x0B, 0x0D, 0x00, 0x0F },
            new byte[] { 0x0B, 0x03, 0x05, 0x08, 0x02, 0x0F, 0x0A, 0x0D, 0x0E, 0x01, 0x07, 0x04, 0x0C, 0x09, 0x06, 0x00 },
            new byte[] { 0x0C, 0x08, 0x02, 0x01, 0x0D, 0x04, 0x0F, 0x06, 0x07, 0x00, 0x0A, 0x05, 0x03, 0x0E, 0x09, 0x0B },
            new byte[] { 0x07, 0x0F, 0x05, 0x0A, 0x08, 0x01, 0x06, 0x0D, 0x00, 0x09, 0x03, 0x0E, 0x0B, 0x04, 0x02, 0x0C },
            new byte[] { 0x05, 0x0D, 0x0F, 0x06, 0x09, 0x02, 0x0C, 0x0A, 0x0B, 0x07, 0x08, 0x01, 0x04, 0x03, 0x0E, 0x00 },
            new byte[] { 0x08, 0x0E, 0x02, 0x05, 0x06, 0x09, 0x01, 0x0C, 0x0F, 0x04, 0x0B, 0x00, 0x0D, 0x0A, 0x03, 0x07 },
            new byte[] { 0x01, 0x07, 0x0E, 0x0D, 0x00, 0x05, 0x08, 0x03, 0x04, 0x0F, 0x0A, 0x06, 0x09, 0x0C, 0x0B, 0x02 }
        };
        public override string Name => "Простая замена ГОСТ 28147";

        public override string DefaultKey => "1234567890ABCDEF";

        public override bool IsReplaceText => false;

        private uint c1 = 0x_1010101;
        private uint c2 = 0x_1010104;
        byte[] s;

        public byte[] EncryptBlocks { get; private set; }
        bool flagD = false;

        private string Key;
        private uint[] _subKeys;
        private readonly int BLOCK_SIZE = 8;

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
            Функция расшифрования по входному тексту.
            Конвертируем текст и ключ в массивы байт,
            Разбиваем текст на блоки по 64 бита и передаем в функцию дешифрования блока
            После дешифрования удаляем дополнение.

        */
        public override string Decrypt(string cipherText, Config config)
        {
            return null;
        }

        /*
            Функция шифрования по входному тексту.
            Проверяем ключ, переводим текст и ключ в массивы байт.
            Дополняем блок до кратности 64.    
        */
        public override string Encrypt(string plainText, Config config)
        {
            return null;
        }

        private byte[] XOR(byte[] block, byte[] data)
        {
            byte[] res = new byte[8];
            for (int i = 0; i < 8; i++)
                res[i] = (byte)(block[i] ^ data[i]);

            return res;
        }

        /*
   Вычисляем раундовые ключи.
   Разбиваем входной массив байт длины 32 на 8 частей длиной 4 байта.
*/
        private void CalculateSubKeys(byte[] key)
        {
            byte[] keyR = new byte[key.Length];
            uint[] subKeys = new uint[8];
            Array.Copy(key, keyR, key.Length);
            Array.Reverse(keyR);
            for (int i = 0; i < 8; i++)
            {
                subKeys[i] = BitConverter.ToUInt32(keyR, i * 4);
            }
            Array.Reverse(subKeys);
            _subKeys = subKeys;
        }
        /*
            Функция расшифрования блока и проверки на тестовых векторах.
            Вычисляем раундовые ключи,
            вычисляем XOR левой части блока и функции G правой части блока.
            Меняем местами части.
            В последней итерации производим те же вычисления, только не меняем части местами.
        */
        public byte[] Decrypt(byte[] data, byte[] key, byte[] synhro)
        {
            byte[][] blocks = GiveBlocks(data);
            byte[] result = new byte[data.Length];
            s = synhro;
            for (int i = 0; i < blocks.GetLength(0); i++)
            {
                byte[] block = blocks[i];
                byte[] left = CalcSyn();
                CalculateSubKeys(key);
                byte[] gamma = FullEnc(left);
                byte[] resBlock = XOR(gamma, block);
                Inc(resBlock);
                Array.Copy(resBlock, 0, result, i * 8, 8);
            }
            return result;
        }
        /*
            Функция расшифрования блока и проверки на тестовых векторах.
            Вычисляем раундовые ключи,
            вычисляем XOR левой части блока и функции G правой части блока.
            Меняем местами части.
            В последней итерации производим те же вычисления, только не меняем части местами.
            Код идентичен функции расшифрования за исключением прямого порядка ключей.
        */
        public byte[] Encrypt(byte[] data, byte[] key, byte[] synhro)
        {
            byte[][] blocks = GiveBlocks(data);
            byte[] result = new byte[data.Length];
            s = synhro;
            for (int i = 0; i < blocks.GetLength(0); i++)
            {
                byte[] block = blocks[i];
                byte[] left = CalcSyn();
                Console.WriteLine($"Шифрование Входной блок {i + 1} :{BitConverter.ToString(left)}");
                CalculateSubKeys(key);
                byte[] gamma = FullEnc(left);
                Console.WriteLine($"Шифрование Выходной блок {i + 1} :{BitConverter.ToString(gamma)}");
                byte[] resBlock = XOR(gamma, block);
                Inc(resBlock);
                Array.Copy(resBlock, 0, result, i * 8, 8);
            }
            return result;
        }

        private byte[][] GiveBlocks(byte[] data)
        {
            int blockSize = 8;
            byte[][] res = new byte[data.Length / 8][];
            for (int i = 0; i < res.Length; i++)
            {
                res[i] = new byte[8];
                for (int j = 0; j < blockSize; j++)
                    res[i][j] = data[i * blockSize + j];
            }
            return res;
        }

        private byte[] CalcSyn()
        {
            return s[..8];
        }

        private void Inc(byte[] gamma)
        {
            byte[] temp = new byte[16];
            Array.Copy(s, 8, temp, 0, 8);
            Array.Copy(gamma, 0, temp, 8, 8);
            Array.Copy(temp, 0, s, 0, 16);
        }

        private byte[] FullEnc(byte[] data)
        {
            byte[] dataR = new byte[data.Length];
            Array.Copy(data, dataR, data.Length);
            Array.Reverse(dataR);

            uint a0 = BitConverter.ToUInt32(dataR, 0); // левые 32 бита
            uint a1 = BitConverter.ToUInt32(dataR, 4); // правые 32 бита

            byte[] result = new byte[8];
            for (int i = 0; i < 31; i++)
            {
                int keyIndex = (i < 24) ? i % 8 : 7 - (i % 8); // с 1 по 24 ключи повторяются, с 25 по 32 записаны в обратном порядке
                uint round = a1 ^ funcG(a0, _subKeys[keyIndex]); // XOR левой части и преобразованной по раундовому ключу правой части

                a1 = a0;
                a0 = round; // меняем местами
            }

            a1 ^= funcG(a0, _subKeys[0]); // XOR левой и правой части без смены мест.

            Array.Copy(BitConverter.GetBytes(a0), 0, result, 0, 4);
            Array.Copy(BitConverter.GetBytes(a1), 0, result, 4, 4);

            Array.Reverse(result);
            return result;
        }

        /*
        Сложение чисел по модулю 2^32,
        Замена результата по таблице замен, циклический сдвиг влево на 11 бит
        */
        private uint funcG(uint a, uint k)
        {
            uint c = a + k;
            uint tmp = funcT(c); // замена 32 битной последовательности по таблице замен
            return (tmp << 11) | (tmp >> 21);
        }
        /*
            Функция замены 4-битных последовательностей по таблице замен.
            Сдвигаем 32 битную последовательность на n * 4 бит и берем 4 младших бита, 
            затем по значению этих бит получаем значение по таблице замены.
        */
        private uint funcT(uint a)
        {
            uint res = 0;

            for (int i = 0; i < 8; i++)
            {
                res ^= (uint)(table[i][a >> (4 * i) & 0b_1111] << (4 * i));
            }

            return res;
        }
        /*
            Функция генерации рандомного ключа 
        */
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

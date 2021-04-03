using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWP.Помошники;

namespace UWP.Алгоритмы
{
    class Magma : Algorithm
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
        public override string Name => "Простая замена ГОСТ 28147";

        public override string DefaultKey => "1234567890ABCDEF";

        public override bool IsReplaceText => false;

        public byte[] EncryptBlocks { get; private set; }
        bool flagD = false;

        private string Key;

        private readonly int BLOCK_SIZE = 8;

        public override string CheckKey(string key)
        {
            byte[] arr = Encoding.Unicode.GetBytes(key);
            if (arr.Length * 8 != 256)
            {
                throw new Error(Error.KeyLength32Byte);
            }
            return key;
        }

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

            uint[] keys = GenerateKeys(key).Reverse().ToArray();
            byte[] block = new byte[BLOCK_SIZE];
            byte[] cipherBlocks = new byte[text.Length];
            Parallel.For(0, text.Length / 8, i =>
            {
                Array.Copy(text, 8 * i, block, 0, 8);
                block = SimpleReplacement(block, keys);
                Array.Copy(block, 0, cipherBlocks, 8 * i, 8);
            });

            byte[] plain = ClearBlocks(cipherBlocks);
            flagD = false;
            return Encoding.Unicode.GetString(plain);
        }

        private byte[] ClearBlocks(byte[] cipherBlocks)
        {
            byte[] result = new byte[1];
            for (int i = cipherBlocks.Length - 1; i > 0; i--)
            {
                if (cipherBlocks[i] == 128)
                {
                    result = new byte[i];
                    Array.Copy(cipherBlocks, 0, result, 0, result.Length);
                    break;
                }
            }
            return result;
        }

        public override string Encrypt(string plainText, Config config)
        {
            string key = CheckKey(config.Key);
            Key = key;
            byte[] text = Encoding.Unicode.GetBytes(plainText);
            text = DopBlock(text);
            uint[] keys = GenerateKeys(key);
            byte[] block = new byte[BLOCK_SIZE];
            byte[] cipherBlocks = new byte[text.Length];
            Parallel.For(0, text.Length / 8, i =>
            {
                Array.Copy(text, 8 * i, block, 0, 8);
                Array.Copy(SimpleReplacement(block, keys), 0, cipherBlocks, 8 * i, 8);
            });

            EncryptBlocks = cipherBlocks;

            return Encoding.Unicode.GetString(cipherBlocks);
        }

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

        private byte[] DopBlock(byte[] text)
        {
            byte te = 0x_80;
            int len = text.Length;
            int countBlock = len / BLOCK_SIZE;
            if (len % BLOCK_SIZE != 0)
                countBlock++;
            if (len % BLOCK_SIZE == 0)
                countBlock++;
            byte[] result = new byte[countBlock * BLOCK_SIZE];
            int i = 0;
            for (; i < countBlock * BLOCK_SIZE; i++)
            {
                if (i == len)
                {
                    result[i++] = 0x_80;
                    break;
                }
                result[i] = text[i];
            }
            for (; i < countBlock * BLOCK_SIZE; i++)
            {
                result[i] = 0x_00;
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

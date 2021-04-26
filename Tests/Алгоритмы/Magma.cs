using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using UWP.Помошники;

namespace UWP.Алгоритмы
{
    class Magma : Algorithm
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

        public byte[] EncryptBlocks { get; private set; }
        bool flagD = false;

        private string Key;
        private byte[] bfs;
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

        public byte[] Encrypt(byte[] text, byte[] key)
        {
            bfs = text; 
            string t = Encoding.Unicode.GetString(text);
            string k = Encoding.Unicode.GetString(key);
            Config config = new Config { Key = k };
            return Encoding.Unicode.GetBytes(Encrypt(t, config));
        }
        public override string Encrypt(string plainText, Config config)
        {
            
            string key = CheckKey(config.Key);
            Key = key;
            //byte[] text = Encoding.Unicode.GetBytes(plainText);
            byte[] text = bfs;
            //text = DopBlock(text);
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
            Console.WriteLine(BitConverter.ToString(N1));
            Console.WriteLine(BitConverter.ToString(N2));
            byte[] temp = N2;
            N2 = N1;
            E(ref N1, ref temp,keys);

            var output = new byte[8];

            for (int i = 0; i < 4; i++)
            {
                output[i] = N1[i];
                output[4 + i] = N2[i];
            }

            return output;
        }

        private void E(ref byte[] N1, ref byte[] N2, uint[] keys)
        {
            for (int i = 0; i < 32; i++)
            {
                byte[] CM1 = BitConverter.GetBytes(BitConverter.ToUInt32(N1, 0) + keys[i]);
                byte[] K = Replfacement(CM1);
                uint R = BitConverter.ToUInt32(K, 0);
                R = (R << 11) | (R >> 21);
                byte[] CM2 = XOR(BitConverter.GetBytes(R), N2);
                if (i < 31)
                {
                    N2 = N1;
                    N1 = CM2;
                }
                else
                    N2 = CM2;

                Console.WriteLine($"{i}(a1,a0) = {BitConverter.ToString(N2).Replace("-", "").ToLower()} , {BitConverter.ToString(N1).Replace("-", "").ToLower()}");
            }
        }

        private byte[] XOR(byte[] k, byte[] a)
        {
            byte[] result = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                result[i] = (byte)(k[i] ^ a[i]);
            }
            return result;
        }

        private byte[] Replfacement(byte[] cM1)
        {

            byte[] result = new byte[4];
            cM1 = ToFourBitsArray(cM1);
            int step = 0;
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    byte temp = table[step++][((cM1[i] >> (4 * j)) & 0b_1111)];
                    result[i] = (byte)(result[i] << (4 * j) | temp);
                }
                
            }
            return result;
        }

        public static byte[] ToFourBitsArray(byte[] arr)
        {
            byte[] res = new byte[8];
            for (int i = 0; i < 4; i++)
            {
                res[2 * i] = (byte)(arr[i] % 16);
                res[2 * i + 1] = (byte)(arr[i] / 16);
            }
            return res;
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

            for (int i = 0; i < 32; i++)
                Console.WriteLine($"{i + 1}: " + BitConverter.ToString(BitConverter.GetBytes(result[i])));

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

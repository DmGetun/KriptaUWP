using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Алгоритмы
{
    class MagmaClass
    {
        private const int BLOCK_SIZE = 8;
        private const int KEY_LENGTH = 32;

        public int BlockSize
        {
            get
            {
                return BLOCK_SIZE;
            }
        }

        public void SetKey(byte[] key)
        {
            _subKeys = GetSubKeys(key);
        }

        public int KeyLength
        {
            get
            {
                return KEY_LENGTH;
            }
        }

        public string Name
        {
            get
            {
                return "GOST/Magma (256-Bit Key)";
            }
        }

        /*
         * Magma cipher implementation
         */

        /// <summary>
        /// Substitution Table
        /// id-tc26-gost-28147-param-Z
        /// </summary>
        private readonly byte[][] _sBox =
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

        private uint[] _subKeys;
        private byte[] s;
        private bool flagD = false;

        public byte[] Decrypt(byte[] data, byte[] key, byte[] synhro, string mode)
        {
            if (mode == "simple")
            {
                flagD = true;
                return SimpleCipher(data, key);
            }
            else if (mode == "gamma")
                return GammaCipher(data, key, synhro);
            else if (mode == "gammaOut")
                return GammaOut(data, key, synhro);
            else if (mode == "gammaText")
                return GammaText(data, key, synhro);

            return new byte[0];
        }

        public byte[] Encrypt(byte[] data, byte[] key,byte[] synhro,string mode)
        {
            if (mode == "simple")
                return SimpleCipher(data, key);
            else if (mode == "gamma")
                return GammaCipher(data, key, synhro);
            else if (mode == "gammaOut")
                return GammaOut(data, key, synhro);
            else if (mode == "gammaText")
                return GammaText(data, key, synhro);

            return new byte[0];
        }

        private byte[] GammaText(byte[] data, byte[] key, byte[] synhro)
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
                byte[] gamma = SimpleCipher(left,key);
                Console.WriteLine($"Шифрование Выходной блок {i + 1} :{BitConverter.ToString(gamma)}");
                byte[] resBlock = XOR(gamma, block);
                IncOut(resBlock);
                Array.Copy(resBlock, 0, result, i * 8, 8);
            }
            return result;
        }

        private byte[] GammaOut(byte[] data, byte[] key, byte[] synhro)
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
                byte[] gamma = SimpleCipher(left,key);
                Console.WriteLine($"Шифрование Выходной блок {i + 1} :{BitConverter.ToString(gamma)}");
                IncOut(gamma);
                Array.Copy(XOR(gamma, block), 0, result, i * 8, 8);
            }
            return result;
        }

        private void IncOut(byte[] gamma)
        {
            byte[] temp = new byte[16];
            Array.Copy(s, 8, temp, 0, 8);
            Array.Copy(gamma, 0, temp, 8, 8);
            Array.Copy(temp, 0, s, 0, 16);
        }

        private byte[] CalcSyn()
        {
            return s[..8];
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

        private byte[] GammaCipher(byte[] data, byte[] key,byte[] synhro)
        {
            s = synhro;
            CalculateSubKeys(key);
            byte[] gamma = SimpleCipher(s, key);
            IncGamma();
            return XOR(gamma, data);
        }

        private byte[] XOR(byte[] block, byte[] data)
        {
            byte[] res = new byte[8];
            for (int i = 0; i < 8; i++)
                res[i] = (byte)(block[i] ^ data[i]);

            return res;
        }

        private void IncGamma()
        {
            ulong t = BitConverter.ToUInt64(s, 0);
            t += 1;
            s = BitConverter.GetBytes(t);
        }

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

        private byte[] SimpleCipher(byte[] data, byte[] key)
        {
            SetKey(key);
            byte[] dataR = new byte[data.Length];
            Array.Copy(data, dataR, data.Length);
            Array.Reverse(dataR);
            uint a0 = BitConverter.ToUInt32(dataR, 0);
            uint a1 = BitConverter.ToUInt32(dataR, 4);

            byte[] result = new byte[8];

            for (int i = 0; i < 31; i++)
            {
                int keyIndex = 0;
                if(flagD == false) keyIndex = (i < 24) ? i % 8 : 7 - (i % 8);
                if(flagD == true) keyIndex = (i < 8) ? i % 8 : 7 - (i % 8);
                uint round = a1 ^ funcG(a0, _subKeys[keyIndex]);

                a1 = a0;
                a0 = round;
            }

            a1 ^= funcG(a0, _subKeys[0]);

            Array.Copy(BitConverter.GetBytes(a0), 0, result, 0, 4);
            Array.Copy(BitConverter.GetBytes(a1), 0, result, 4, 4);

            Array.Reverse(result);
            return result;
        }

        private uint funcG(uint a, uint k)
        {
            uint c = a + k;
            uint tmp = funcT(c);
            return (tmp << 11) | (tmp >> 21);
        }

        private uint funcT(uint a)
        {
            uint res = 0;

            for(int i = 0; i < 8; i++)
            {
                res ^= (uint)(_sBox[i][a >> (4 * i) & 0b_1111] << (4 * i));
            }

            return res;
        }

        private uint[] GetSubKeys(byte[] key)
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
            return subKeys;
        }
    }
}

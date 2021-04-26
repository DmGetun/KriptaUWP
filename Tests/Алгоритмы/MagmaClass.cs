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

        public byte[] Decrypt(byte[] data, byte[] key)
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
                int keyIndex = (i < 8) ? i % 8 : 7 - (i % 8) ;
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

        public byte[] Encrypt(byte[] data, byte[] key)
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
                int keyIndex = (i < 24) ? i % 8 : 7 - (i % 8);
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

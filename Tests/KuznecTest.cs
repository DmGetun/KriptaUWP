using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWP.Помошники;

namespace Tests
{
    class KuznecTest
    {
        private const int BLOCK_SIZE = 16;
        private const int KEY_LENGTH = 32;

        private const int SUB_LENGTH = KEY_LENGTH / 2;

        public int BlockSize
        {
            get
            {
                return BLOCK_SIZE;
            }
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
                return "GOST/Kuznyechik (256-Bit Key)";
            }
        }
        private byte[][] _subKeys;

        private byte[] _pi = Tables._pi;
        private byte[] _reversepi = Tables._reversePi;

        private readonly byte[] _lFactors = {
            0x94, 0x20, 0x85, 0x10, 0xC2, 0xC0, 0x01, 0xFB,
            0x01, 0xC0, 0xC2, 0x10, 0x85, 0x20, 0x94, 0x01
            };

        public byte[] Encrypt(byte[] data, byte[] key)
        {
            this.SetKey(key);
            byte[] block = new byte[BLOCK_SIZE];
            byte[] temp = new byte[BLOCK_SIZE];

            Array.Copy(data, block, BLOCK_SIZE);

            for (int i = 0; i < 9; i++)
            {
                LSX(ref temp, ref _subKeys[i], ref block);
                Array.Copy(temp, block, BLOCK_SIZE);
            }

            X(ref block, ref _subKeys[9]);

            return block;
        }

        public byte[] Decrypt(byte[] data, byte[] key)
        {
            this.SetKey(key);
            byte[] block = new byte[BLOCK_SIZE];
            byte[] temp = new byte[BLOCK_SIZE];

            Array.Copy(data, block, BLOCK_SIZE);

            Console.WriteLine(BitConverter.ToString(block));

            for (int i = 9; i > 0; i--)
            {
                ReverseLSX(ref temp, ref _subKeys[i], ref block);
                Array.Copy(temp, block, BLOCK_SIZE);
            }

            X(ref block, ref _subKeys[0]);

            return block;
        }

        public void SetKey(byte[] key)
        {

            /*
             * Initialize SubKeys array
             */

            _subKeys = new byte[10][];
            for (int i = 0; i < 10; i++)
            {
                _subKeys[i] = new byte[SUB_LENGTH];
            }

            byte[] x = new byte[SUB_LENGTH];
            byte[] y = new byte[SUB_LENGTH];

            byte[] c = new byte[SUB_LENGTH];

            /*
             * SubKey[1] = k[255..128]
             * SubKey[2] = k[127..0]
             */

            for (int i = 0; i < SUB_LENGTH; i++)
            {
                _subKeys[0][i] = x[i] = key[i];
                _subKeys[1][i] = y[i] = key[i + 16];
            }

            for (int k = 1; k < 5; k++)
            {

                for (int j = 1; j <= 8; j++)
                {
                    C(ref c, 8 * (k - 1) + j);
                    F(ref c, ref x, ref y);
                }

                Array.Copy(x, _subKeys[2 * k], SUB_LENGTH);
                Array.Copy(y, _subKeys[2 * k + 1], SUB_LENGTH);

            }

        }
        

        private static byte kuz_mul_gf256_slow(byte a, byte b)
        {
            byte p = 0;
            byte counter;
            byte hi_bit_set;
            for (counter = 0; counter < 8 && a != 0 && b != 0; counter++)
            {
                if ((b & 1) != 0)
                    p ^= a;
                hi_bit_set = (byte)(a & 0x80);
                a <<= 1;
                if (hi_bit_set != 0)
                    a ^= 0xc3; /* x^8 + x^7 + x^6 + x + 1 */
                b >>= 1;
            }
            return p;
        }



        private void S(ref byte[] data)
        {
            data = data.Select(i => _pi[i]).ToArray();
        }

        private void ReverseS(ref byte[] data)
        {
            data = data.Select(i => _reversepi[i]).ToArray();
        }

        private void X(ref byte[] result, ref byte[] data)
        {
            for (int i = 0; i < result.Length; i++)
            {
                result[i] ^= data[i];
            }
        }

        private byte l(ref byte[] data)
        {
            byte x = data[15];
            for (int i = 14; i >= 0; i--)
            {
                x ^= kuz_mul_gf256_slow(data[i],_lFactors[i]);
            }
            return x;
        }

        private void R(ref byte[] data)
        {
            byte z = l(ref data);
            for (int i = 15; i > 0; i--)
            {
                data[i] = data[i - 1];
            }
            data[0] = z;
        }

        private void ReverseR(ref byte[] data)
        {
            byte z = data[0];
            byte[] temp = new byte[16];
             
            for(int i = 15;i > 0; i--)
            {
                temp[i - 1] = data[i];
            }

            temp[15] = z;
            byte r = l(ref temp);
            temp[15] = r;
            Array.Copy(temp,0,data,0,16);
        }

        private void L(ref byte[] data)
        {
            for (int i = 0; i < 16; i++)
            {
                R(ref data);
            }
        }

        private void ReverseL(ref byte[] data)
        {
            for (int i = 0; i < 16; i++)
            {
                ReverseR(ref data);
            }
        }

        private void F(ref byte[] k, ref byte[] a1, ref byte[] a0)
        {
            byte[] temp = new byte[SUB_LENGTH];

            LSX(ref temp, ref k, ref a1);
            X(ref temp, ref a0);

            Array.Copy(a1, a0, SUB_LENGTH);
            Array.Copy(temp, a1, SUB_LENGTH);

        }

        private void LSX(ref byte[] result, ref byte[] k, ref byte[] a)
        {
            Array.Copy(k, result, BLOCK_SIZE);
            X(ref result, ref a);
            S(ref result);
            L(ref result);
        }

        private void ReverseLSX(ref byte[] result, ref byte[] k, ref byte[] a)
        {
            Array.Copy(k, result, BLOCK_SIZE);
            X(ref result, ref a);
            ReverseL(ref result);
            ReverseS(ref result);
            Console.WriteLine(BitConverter.ToString(result));
        }

        private void C(ref byte[] c, int i)
        {
            Array.Clear(c, 0, SUB_LENGTH);
            c[15] = (byte)i;
            L(ref c);
        }
    }
}

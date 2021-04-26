using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWP.Помошники;

namespace Tests.Алгоритмы
{
    class KuznecNew
    {
        private const int BLOCK_SIZE = 16;
        private const int KEY_LENGTH = 32;

        private const int SUB_LENGTH = KEY_LENGTH / 2;

        private byte[][] _gf_mul = init_gf256_mul_table();

        private byte[] _pi;
        private byte[] reverse_pi;
        private byte[] resultArray;
        private readonly byte[] _lFactors = {
            0x94, 0x20, 0x85, 0x10, 0xC2, 0xC0, 0x01, 0xFB,
            0x01, 0xC0, 0xC2, 0x10, 0x85, 0x20, 0x94, 0x01
        };

        public byte[] Encrypt(byte[] plainText, byte[] key)
        {
            string text = Encoding.Unicode.GetString(plainText);
            string _key = BitConverter.ToString(key);
            Config config = new Config { Key = _key };
            string res = Encrypt(text, config);
            byte[] final = Encoding.Unicode.GetBytes(res);
            return final;
        }

        public string Encrypt(string plainText, Config config)
        {
            _pi = Tables._pi;
            reverse_pi = Tables._reversePi;
            string keyString = config.Key;
            byte[] keys = ParseHex(keyString);
            string temp1 = BitConverter.ToString(keys);
            byte[] text = Encoding.Unicode.GetBytes(plainText);
            string temp2 = BitConverter.ToString(text);
            //text = DopBlock(text);
            byte[] block = new byte[BLOCK_SIZE];
            byte[] result = new byte[text.Length];
            //GOST_Kuz_Expand_Key(keys);

            Parallel.For(0, text.Length / BLOCK_SIZE, i =>
            {
                Array.Copy(text, i * BLOCK_SIZE, block, 0, BLOCK_SIZE);
                byte[] temp = new byte[16];
                GOST_Kuz_Encript(ref block, ref temp);
                Array.Copy(temp, 0, result, i * BLOCK_SIZE, BLOCK_SIZE);
            });
            resultArray = result;
            var temp = BitConverter.ToString(resultArray);
            return Encoding.Unicode.GetString(result);
        }

        public void GOST_Kuz_Encript(ref byte[] blk, ref byte[] out_blk)
        {
            blk.CopyTo(out_blk, 0);
            for (int i = 0; i < 9; i++)
            {
                //X(ref iter_key[i], ref out_blk, ref out_blk);
                //S(ref out_blk, ref out_blk);
                //L(ref out_blk, ref out_blk);
            }
            //X(ref out_blk, ref iter_key[9], ref out_blk);
        }

        private byte[] ParseHex(string keyString)
        {
            string[] arr = keyString.Split('-');
            byte[] res = new byte[arr.Length];
            int i = 0;
            foreach (string s in arr)
                res[i++] = Convert.ToByte(s, 16);
            return res;
        }

        private static byte[][] init_gf256_mul_table()
        {
            byte[][] mul_table = new byte[256][];
            for (int x = 0; x < 256; x++)
            {
                mul_table[x] = new byte[256];
                for (int y = 0; y < 256; y++)
                {
                    mul_table[x][y] = kuz_mul_gf256_slow((byte)x, (byte)y);
                }
            }
            return mul_table;
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
            _pi = Tables._pi;
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = _pi[data[i]];
            }
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
                x ^= _gf_mul[data[i]][_lFactors[i]];
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

        private void L(ref byte[] data)
        {
            for (int i = 0; i < 16; i++)
            {
                R(ref data);
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

        private void C(ref byte[] c, int i)
        {
            Array.Clear(c, 0, SUB_LENGTH);
            c[15] = (byte)i;
            L(ref c);
        }
    }
}

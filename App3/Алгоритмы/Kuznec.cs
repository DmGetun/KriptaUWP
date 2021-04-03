using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWP.Помошники;

namespace UWP.Алгоритмы
{
    class Kuznec : Algorithm
    {
        private byte[] pi, reverse_pi;

        private const int SUB_LENGTH = 16;

        private const int BLOCK_SIZE = 16;

        private const int KEY_LENGTH = 32;

        private byte[][] _subKeys;

        private readonly byte[] _lFactors = {
            0x94, 0x20, 0x85, 0x10, 0xC2, 0xC0, 0x01, 0xFB,
            0x01, 0xC0, 0xC2, 0x10, 0x85, 0x20, 0x94, 0x01
        };

        public override string Name => "Кузнечик";

        public override string DefaultKey => "8899aabbccddeeff0011223344556677fedcba98765432100123456789abcdef";

        public override bool IsReplaceText => false;

        public override string CheckKey(string key)
        {
            throw new NotImplementedException();
        }

        public override string Decrypt(string cipherText, Config config)
        {
            throw new NotImplementedException();
        }

        public override string Encrypt(string plainText, Config config)
        {
            pi = Tables._pi;
            reverse_pi = Tables._reversePi;
            string keyString = "8899aabbccddeeff0011223344556677fedcba98765432100123456789abcdef";
            string[] text = SplitText(plainText);
            string[] outputBlocks = new string[text.Length];
            string result;
            byte[] keysArray = Encoding.Unicode.GetBytes(keyString);
            SetKey(keysArray);
/*            for (int i = 0; i < inputText.Length; i++)
            {
                outputBlocks[i] = ByteToHexConverter(Encrypt(HexToByte.HexToByteConvert(inputText[i])));
            }*/
            return "";
        }

        private string[] SplitText(string inputText)
        {
            //Chuck size is 32 for 128-bit text
            int chunkSize = 32;
            int inputStringSize = inputText.Length;
            double doubleVarChunkCount = (double)inputStringSize / (double)chunkSize;
            int chunkCount = (int)Math.Ceiling(doubleVarChunkCount);
            if (inputStringSize < chunkCount * chunkSize)
            {
                string zeroChar = "0";
                for (int j = inputStringSize; j < chunkCount * chunkSize; j++)
                {
                    inputText += zeroChar;
                }
            }

            string[] block = new string[chunkCount];
            for (int i = 0; i < chunkCount; i++)
            {
                block[i] = inputText.Substring(i * chunkSize, chunkSize);
                //Console.WriteLine(block[i]);
            }


            return block;
        }

        public void SetKey(byte[] key)
        {

            /*
             * Инициализация массива итерационных ключей
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

            Console.WriteLine("\nРазвертывание ключа:");
            for (int j = 0; j < _subKeys.Length; j++)
            {
                Console.WriteLine("K" + (j + 1) + ": " + ByteToHexConverter(_subKeys[j]).ToUpper());
            }
        }

        public static string ByteToHexConverter(byte[] bytes)
        {
            char[] c = new char[bytes.Length * 2];

            byte b;

            for (int bx = 0, cx = 0; bx < bytes.Length; ++bx, ++cx)
            {
                b = ((byte)(bytes[bx] >> 4));
                c[cx] = (char)(b > 9 ? b + 0x37 + 0x20 : b + 0x30);

                b = ((byte)(bytes[bx] & 0x0F));
                c[++cx] = (char)(b > 9 ? b + 0x37 + 0x20 : b + 0x30);
            }

            return new string(c);
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

        private void X(ref byte[] result, ref byte[] data)
        {
            for (int i = 0; i < result.Length; i++)
            {
                result[i] ^= data[i];
            }
        }

        private void S(ref byte[] data)
        {
            for(int i = 0; i < data.Length; i++)
            {
                data[i] = pi[data[i]];
            }
        }

        private void ReverseS(ref byte[] data)
        {
            for(int i = 0; i < data.Length; i++)
            {
                data[i] = reverse_pi[data[i]];
            }
        }

        private byte GF_mul(byte a, byte b)
        {
            byte p = 0;
            byte counter;
            byte hi_bit_set;
            for (counter = 0; counter < 8; counter++)
            {
                if ((b & 1) != 0)
                    p ^= a;
                hi_bit_set = (byte)(a & 0x80);
                a <<= 1;
                if (hi_bit_set != 0)
                    a ^= 0xC3; /* x^8 + x^7 + x^6 + x + 1 */
                b >>= 1;
            }
            return p;
        }

        private void R(ref byte[] data)
        {
            byte z = data[15];
            for(int i = 14;i >= 0; i--)
            {
                z ^= GF_mul(data[i], _lFactors[i]);
            }

            for(int i = 15; i > 0; i--)
            {
                data[i] = data[i - 1];
            }
            data[0] = z;
        }

        private void reverseR(ref byte[] data)
        {
            byte z = data[0];

            for (int i = 0; i < 15; i++)
            {
                data[i] = data[i + 1];
                z ^= GF_mul(data[i], _lFactors[i]);
            }
            data[15] = z;
        }

        private void L(ref byte[] data)
        {
            for (int i = 0; i < 16; i++)
            {
                R(ref data);
            }
        }
        private void reverseL(ref byte[] data)
        {
            for (int i = 0; i < 16; i++)
            {
                reverseR(ref data);
            }
        }

        private void C(ref byte[] c, int i)
        {
            Array.Clear(c, 0, SUB_LENGTH);
            c[15] = (byte)i;
            L(ref c);
        }

        public override string GenerateKey()
        {
            throw new NotImplementedException();
        }

        public override string KeyView()
        {
            return "";
        }
    }
}

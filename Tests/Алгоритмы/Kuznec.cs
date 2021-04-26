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
        private static byte[] pi, reverse_pi;

        private const int SUB_Length = 16;

        private const int BLOCK_SIZE = 16;

        private const int KEY_Length = 32;

        private byte[][] _subKeys;
        private byte[] resultArray;

        public override string Name => "Кузнечик";

        public override string DefaultKey => "8899aabbccddeeff0011223344556677fedcba98765432100123456789abcdef";

        public override bool IsReplaceText => false;

        // массив для хранения констант
        static byte[][] iter_C = new byte[32][]; // 16
        // массив для хранения ключей
        static byte[][] iter_key = new byte[10][]; // 64

        public override string CheckKey(string key)
        {
            throw new NotImplementedException();
        }

        private static byte[] l_vec = {
             1, (byte) 148, 32, (byte) 133, 16, (byte) 194, (byte) 192, 1,
             (byte) 251, 1, (byte) 192, (byte) 194, 16, (byte) 133, 32, (byte) 148
        };

        public override string Decrypt(string cipherText, Config config)
        {
            pi = Tables._pi;
            reverse_pi = Tables._reversePi;
            string keyString = config.Key;
            byte[] keys = Encoding.Unicode.GetBytes(keyString);
            byte[] text = Encoding.Unicode.GetBytes(cipherText);
            text = resultArray;
            byte[] block = new byte[BLOCK_SIZE];
            byte[] result = new byte[text.Length];
            GOST_Kuz_Expand_Key(keys);

            Parallel.For(0, text.Length / BLOCK_SIZE, i =>
            {
                Array.Copy(text, i * BLOCK_SIZE, block, 0, BLOCK_SIZE);
                byte[] temp = new byte[16];
                GOST_Kuz_Decript(ref block, ref temp);
                Array.Copy(temp, 0, result, i * BLOCK_SIZE, BLOCK_SIZE);
            });
            result = ClearBlocks(result);
            return Encoding.Unicode.GetString(result);
        }

        public byte[] Decrypt(byte[] cipherText, byte[] key)
        {
            string text = Encoding.Unicode.GetString(cipherText);
            string _key = Encoding.Unicode.GetString(key);
            Config config = new Config { Key = _key };
            string res = Decrypt(text, config);
            byte[] final = Encoding.Unicode.GetBytes(res);
            return final;
        }

        public byte[] Encrypt(byte[] plainText, byte[] key)
        {
            string text = Encoding.Unicode.GetString(plainText);
            string _key = BitConverter.ToString(key);
            Config config = new Config { Key = _key };
            string res = Encrypt(text, config);
            byte[] final = Encoding.Unicode.GetBytes(res);
            return final;
        }

        public override string Encrypt(string plainText, Config config)
        {
            pi = Tables._pi;
            reverse_pi = Tables._reversePi;
            string keyString = config.Key;
            byte[] keys = ParseHex(keyString);
            string temp1 = BitConverter.ToString(keys);
            byte[] text = Encoding.Unicode.GetBytes(plainText);
            string temp2 = BitConverter.ToString(text);
            text = DopBlock(text);
            byte[] block = new byte[BLOCK_SIZE];
            byte[] result = new byte[text.Length];
            GOST_Kuz_Expand_Key(keys);

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

        private byte[] ParseHex(string keyString)
        {
            string[] arr = keyString.Split('-');
            byte[] res = new byte[arr.Length];
            int i = 0;
            foreach (string s in arr)
                res[i++] = Convert.ToByte(s, 16);
            return res;
        }

        // функция X
        static private void GOST_Kuz_X(ref byte[] a,ref byte[] b, ref byte[] c)
        {
            for (int i = 0; i < BLOCK_SIZE; i++)
                c[i] = (byte)(a[i] ^ b[i]);
        }

        // Функция S
        static private void GOST_Kuz_S(ref byte[] in_data, ref byte[] out_data)
        {
            for (int i = 0; i < BLOCK_SIZE; i++)
                out_data[i] = pi[in_data[i]];        
        }

        // умножение в поле Галуа
        static private byte GOST_Kuz_GF_mul(byte a, byte b)
        {
            byte c = 0;
            byte hi_bit;
            int i;
            for (i = 0; i < 8; i++)
            {
                if ((b & 1) == 1)
                    c ^= a;
                hi_bit = (byte)(a & 0x80);
                a <<= 1;
                if (hi_bit == 0)
                    a ^= 0xc3; //полином  x^8+x^7+x^6+x+1
                b >>= 1;
            }
            return c;
        }

        // функция R сдвигает данные и реализует уравнение, представленное для расчета L-функции
        static private void GOST_Kuz_R(ref byte[] state)
        {
            byte a_15 = 0;
            byte[] temp = new byte[16];
            for (int i = 15; i >= 0; i--)
            {
                if (i == 0)
                    temp[15] = state[i];
                else
                    temp[i - 1] = state[i];
                a_15 ^= GOST_Kuz_GF_mul(state[i], l_vec[i]);
            }
            temp[15] = a_15;
            temp.CopyTo(state,0);
        }

        static private void GOST_Kuz_reverse_R(ref byte[] state)
        {
            byte a_0;
            a_0 = state[15];
            byte[] temp = new byte[16];
            for (int i = 1; i < 16; i++)
            {
                temp[i] = state[i - 1];
                a_0 ^= GOST_Kuz_GF_mul(temp[i], l_vec[i]);
            }
            temp[0] = a_0;
            temp.CopyTo(state, 0);
        }

        static private void GOST_Kuz_L(ref byte[] in_data, ref byte[] out_data)
        {
            byte[] temp = in_data;
            for (int i = 0; i < 16; i++)
            {
                GOST_Kuz_R(ref temp);
            }
            temp.CopyTo(out_data, 0);
        }

        // функция S^(-1)
        static private void GOST_Kuz_reverse_S(ref byte[] in_data, ref byte[] out_data)
        {
            for (int i = 0; i < BLOCK_SIZE; i++)
                out_data[i] = reverse_pi[in_data[i]];         
        }
        static private void GOST_Kuz_reverse_L(ref byte[] in_data, ref byte[] out_data)
        {
            byte[] temp = new byte[16];
            in_data.CopyTo(temp, 0);
            for (int i = 0; i < 16; i++)
                GOST_Kuz_reverse_R(ref temp);
            temp.CopyTo(out_data, 0);
        }
        // функция расчета констант
        static private void GOST_Kuz_Get_C()
        {
            byte[][] iter_num = new byte[32][];
            for (int i = 0; i < 32; i++)
            {
                iter_num[i] = new byte[16];
                iter_num[i][0] = (byte)(i + 1);
            }
            for (int i = 0; i < 32; i++)
            {
                GOST_Kuz_L(ref iter_num[i],ref iter_C[i]);
            }
        }
        // функция, выполняющая преобразования ячейки Фейстеля
        static private void GOST_Kuz_F(ref byte[] in_key_1, ref byte[] in_key_2,ref byte[] out_key_1,ref byte[] out_key_2, ref byte[] iter_const)
        {
            byte[] temp = new byte[16];
            in_key_1.CopyTo(out_key_2, 0);
            GOST_Kuz_X(ref in_key_1, ref iter_const,ref temp);
            GOST_Kuz_S(ref temp,ref temp);
            GOST_Kuz_L(ref temp,ref temp);
            GOST_Kuz_X(ref temp, ref in_key_2, ref out_key_1);
        }
        // функция расчета раундовых ключей
        public void GOST_Kuz_Expand_Key(byte[] keys)
        {
            int len = keys.Length / 2;
            byte[] key_1 = new byte[len], key_2 = new byte[len];
            Array.Copy(keys, 0, key_1, 0, len);
            Array.Copy(keys, len, key_2, 0, len);

            for (int i = 0; i < iter_C.Length; i++)
                iter_C[i] = new byte[16];

            for (int i = 0; i < iter_key.Length; i++)
                iter_key[i] = new byte[16];


            byte[] iter1 = new byte[16];
            byte[] iter2 = new byte[16];

            byte[] iter3 = new byte[16];
            byte[] iter4 = new byte[16];
            GOST_Kuz_Get_C();

            key_1.CopyTo(iter_key[0],0);
            key_2.CopyTo(iter_key[1],0);

            key_1.CopyTo(iter1, 0);
            key_2.CopyTo(iter2, 0);
            for (int i = 0; i < 4; i++)
            {
                GOST_Kuz_F(ref iter1, ref iter2, ref iter3, ref iter4, ref iter_C[0 + 8 * i]);
                GOST_Kuz_F(ref iter3, ref iter4, ref iter1, ref iter2, ref iter_C[1 + 8 * i]);
                GOST_Kuz_F(ref iter1, ref iter2, ref iter3, ref iter4, ref iter_C[2 + 8 * i]);
                GOST_Kuz_F(ref iter3, ref iter4, ref iter1, ref iter2, ref iter_C[3 + 8 * i]);
                GOST_Kuz_F(ref iter1, ref iter2, ref iter3, ref iter4, ref iter_C[4 + 8 * i]);
                GOST_Kuz_F(ref iter3, ref iter4, ref iter1, ref iter2, ref iter_C[5 + 8 * i]);
                GOST_Kuz_F(ref iter1, ref iter2, ref iter3, ref iter4, ref iter_C[6 + 8 * i]);
                GOST_Kuz_F(ref iter3, ref iter4, ref iter1, ref iter2, ref iter_C[7 + 8 * i]);

                iter_key[2 * i + 2] = iter1;
                Console.WriteLine(BitConverter.ToString(iter_key[2 * i + 2]));
                iter_key[2 * i + 3] = iter2;
                Console.WriteLine(BitConverter.ToString(iter_key[2 * i + 3]));
            }
        }
        // функция шифрования блока
        public void GOST_Kuz_Encript(ref byte[] blk, ref byte[] out_blk)
        {
            blk.CopyTo(out_blk, 0);
            for (int i = 0; i < 9; i++)
            {
                GOST_Kuz_X(ref iter_key[i], ref out_blk,ref out_blk);
                GOST_Kuz_S(ref out_blk, ref out_blk);
                GOST_Kuz_L(ref out_blk, ref out_blk);
            }
            GOST_Kuz_X(ref out_blk, ref iter_key[9], ref out_blk);
        }
        //функция расшифрования блока
        public void GOST_Kuz_Decript(ref byte[] blk, ref byte[] out_blk)
        {
            blk.CopyTo(out_blk, 0);
            GOST_Kuz_X(ref out_blk, ref iter_key[9], ref out_blk);
            for (int i = 8; i >= 0; i--)
            {
                GOST_Kuz_reverse_L(ref out_blk, ref out_blk);
                GOST_Kuz_reverse_S(ref out_blk, ref out_blk);
                GOST_Kuz_X(ref iter_key[i], ref out_blk, ref out_blk);
            }
        }


        /*
        Операция удаления нулевых байт.
        Начиная с конца массива байт зашифрованного текста,
        удаляем нули, пока не встретим единицу.
        Как встречаем - удаляем и завершаем процедуру.
        */
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
        /*
            Дополнение блока.
            Умножаем количество блоков на длину блока, чтобы получить итоговое количество байт.
            В цикле по каждому байту идем до количества байт.
            Если байты закончились, а итоговое количество байт достигнуто не было,
            вставляем единицу и после неё заполняем нулями оставшуюься часть блока.
            В случае, если текст будет полным - добавим новый блок, состоящий из 1 единици и все остальное - нули.
        */
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
            throw new NotImplementedException();
        }

        public override string KeyView()
        {
            return "";
        }
    }
}

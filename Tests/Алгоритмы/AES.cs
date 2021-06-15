using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWP.Помошники;

namespace UWP.Алгоритмы
{
    class AES : Algorithm
    {
        public override string Name => "Шифр AES";

        public override string DefaultKey => "12345678";

        public override bool IsReplaceText => false;

        private int Nb = 4;
        private int Nk = 4;
        private int Nr;

        private readonly int BLOCK_SIZE = 16;

        private static byte[] Sbox, InvSbox;
        private static byte[][] Rcon;
        byte[][] w;
        byte[] Key;
        private bool flagD = false;
        private byte[] encrypt;

        /*
            Функция проверки ключа и настройки параметров.
            Проверяем длину ключа на соответствие 128,192,256 бит.
            В зависимости от длины выставляем параметр Nk
        */
        public override string CheckKey(string key)
        {
            int len = Encoding.Unicode.GetBytes(key).Length;
            if (len == 16) Nk = 4;
            else if (len == 24) Nk = 6;
            else if (len == 32) Nk = 8;
            else throw new Error(Error.AesLengthKey);
            Key = Encoding.Unicode.GetBytes(key);
            Nr = Nk + 6;
            return key;
        }
        /*
            Функция расшифрования.
            Аналогична функции шифрования за исключением
            процесса расшифрования блока.
        */
        public override string Decrypt(string cipherText, Config config)
        {
            flagD = true;
            string _key = CheckKey(config.Key);
            string plain = "69c4e0d86a7b0430d8cdb78070b4c55a";
            string key_ = "000102030405060708090a0b0c0d0e0f";

            byte[] text = Split(plain, 2);
            byte[] key = Split(key_, 2);

            Sbox = Tables.AES_Sbox;
            InvSbox = Tables.AES_InvSbox;
            Rcon = Tables.Rcon;

            byte[][] keys = GenerateSubKeys(key);
            w = GenerateWordKeys(keys);

            byte[] plainText = new byte[text.Length];
            Parallel.For(0, text.Length / BLOCK_SIZE, i =>
            {
                byte[] block = new byte[BLOCK_SIZE];
                Array.Copy(text, i * BLOCK_SIZE, block, 0, BLOCK_SIZE);
                Array.Copy(DecryptBlock(block), 0, plainText, i * block.Length, block.Length);
            });
            return BitConverter.ToString(plainText);
        }
        /*
            Фукнция расшифрования блока.    
        */
        private byte[] DecryptBlock(byte[] inp)
        {
            byte[] temp = new byte[inp.Length];

            byte[][] state = new byte[4][];
            for (int i = 0; i < 4; i++)
            {
                state[i] = new byte[4];
            }
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    state[j][i] = inp[4 * i + j];
                }
            }
            Console.Write($"round[0].input: ");
            PrintState(state);

            state = AddRoundKey(state, w, Nr);
            Console.Write($"round[0].k_sch: ");
            PrintState(state);

            for (int i = Nr - 1; i >= 1; i--)
            {
                state = InvShiftRows(state);
                Console.Write($"round[{i}].s_row: ");
                PrintState(state);

                state = InvSubBytes(state);
                Console.Write($"round[{i}].s_box: ");
                PrintState(state);

                state = AddRoundKey(state, w, i);
                Console.Write($"round[{i}].add: ");
                PrintState(state);

                state = InvMixColumns(state);
                Console.Write($"round[{i}].Mix: ");
                PrintState(state);
            }
            state = InvSubBytes(state);
            Console.Write($"round[10].s_box: ");
            PrintState(state);

            state = InvShiftRows(state);
            Console.Write($"round[10].s_row: ");
            PrintState(state);

            state = AddRoundKey(state, w, 0);
            Console.Write($"round[10].add: ");
            PrintState(state);

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    temp[i * 4 + j] = state[j][i];
                }
            }
            return temp;
        }

        public byte[] Encrypt(byte[] plainText, byte[] key)
        {
            Config config = new Config { Key = Encoding.Unicode.GetString(key) };
            string text = Encoding.Unicode.GetString(plainText);

            return Split(Encrypt(text, config).Replace("-",""),2);
        }

        public byte[] Decrypt(byte[] plainText, byte[] key)
        {
            Config config = new Config { Key = Encoding.Unicode.GetString(key) };
            string text = Encoding.Unicode.GetString(plainText);

            return Split(Decrypt(text, config).Replace("-", ""), 2);
        }
        public override string Encrypt(string plainText, Config config)
        {
            string plain = "00112233445566778899aabbccddeeff";
            string key_ = "000102030405060708090a0b0c0d0e0f";

            byte[] key = Split(key_,2);
            Nk = 4;
            Nr = 10;
            byte[] text = Split(plain,2);
            Sbox = Tables.AES_Sbox;
            InvSbox = Tables.AES_InvSbox;
            Rcon = Tables.Rcon;

            byte[][] keys = GenerateSubKeys(key);
            w = GenerateWordKeys(keys);

            byte[] cipherText = new byte[text.Length];
            Parallel.For(0, text.Length / BLOCK_SIZE, i =>
            {
                byte[] block = new byte[BLOCK_SIZE];
                Array.Copy(text, i * BLOCK_SIZE, block, 0, BLOCK_SIZE);
                Array.Copy(EncryptBlock(block), 0, cipherText, i * block.Length, block.Length);
            });
            flagD = false;
            encrypt = cipherText;
            return BitConverter.ToString(cipherText);
        }

        private byte[] EncryptBlock(byte[] inp)
        {
            byte[] temp = new byte[inp.Length];

            byte[][] state = new byte[4][];
            for (int i = 0; i < 4; i++)
                state[i] = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    state[j][i] = inp[4 * i + j];
                }
            }
            Console.Write($"round[0].input: ");
            PrintState(state);

            state = AddRoundKey(state, w, 0);
            Console.Write($"round[0].k_sch: ");
            PrintState(state);

            for (int i = 1; i < Nr; i++)
            {
                state = SubBytes(state);
                Console.Write($"round[{i}].subBytes: ");
                PrintState(state);

                state = ShiftRows(state);
                Console.Write($"round[{i}].ShiftRows: ");
                PrintState(state);

                state = MixColumns(state);
                Console.Write($"round[{i}].MixColumns: ");
                PrintState(state);

                state = AddRoundKey(state, w, i);
                Console.Write($"round[{i}].AddRoundKey: ");
                PrintState(state);
            }
            Console.Write("round[10].SubBytes: ");
            state = SubBytes(state);
            PrintState(state);

            Console.Write("round[10].ShiftRows: ");
            state = ShiftRows(state);
            PrintState(state);

            Console.Write("round[10].AddRoundKey: ");
            state = AddRoundKey(state, w, Nr);
            PrintState(state);

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    temp[i * 4 + j] = state[j][i];
                }
            }

            return temp;
        }

        private static void PrintKeys(byte[] keys)
        {
            Console.WriteLine(BitConverter.ToString(keys));
        }

        private void PrintState(byte[][] state)
        {
            byte[] temp = new byte[state.Length * 4];
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    temp[4 * i + j] = state[j][i];

            Console.WriteLine(BitConverter.ToString(temp));
        }

        private byte[] SubWord(byte[] state)
        {
            return state.Select(i => Sbox[i]).ToArray();
        }

        private byte[][] SubBytes(byte[][] state)
        {
            byte[][] temp = new byte[4][];
            for (int row = 0; row < 4; row++)
            {
                temp[row] = new byte[Nb];
                for (int col = 0; col < Nb; col++)
                    temp[row][col] = Sbox[state[row][col]];
            }

            return temp;
        }

        private byte[][] InvSubBytes(byte[][] state)
        {
            byte[][] temp = new byte[4][];
            for (int row = 0; row < 4; row++)
            {
                temp[row] = new byte[Nb];
                for (int col = 0; col < Nb; col++)
                    temp[row][col] = InvSbox[state[row][col]];
            }

            return temp;
        }

        private byte[][] ShiftRows(byte[][] state)
        {

            byte[][] t = new byte[4][];

            for (int r = 0; r < 4; r++)
            {
                byte[] row = state[r];
                row = row.Skip(r).Concat(row.Take(r)).ToArray();
                t[r] = row;
            }
            return t;
        }

        private byte[][] InvShiftRows(byte[][] state)
        {
            byte[][] t = new byte[4][];
            
            for (int r = 0; r < 4; r++)
            {
                byte[] row = state[r];
                row = row.Skip(4 - r).Concat(row.Take(r + 4)).ToArray();
                t[r] = row;
            }
            return t;
        }

        private byte[][] MulKef = new byte[][]
        {
            new byte[] {0x_02,0x_03,0x_01,0x_01},
            new byte[] {0x_01,0x_02,0x_03,0x_01},
            new byte[] {0x_01,0x_01,0x_02,0x_03},
            new byte[] {0x_03,0x_01,0x_01,0x_02}
        };

        private byte[][] InvMixColumns(byte[][] s)
        {
            int[] sp = new int[4];
            byte b02 = (byte)0x0e, b03 = (byte)0x0b, b04 = (byte)0x0d, b05 = (byte)0x09;
            for (int c = 0; c < 4; c++)
            {
                sp[0] = FFMul(b02, s[0][c]) ^ FFMul(b03, s[1][c]) ^ FFMul(b04, s[2][c]) ^ FFMul(b05, s[3][c]);
                sp[1] = FFMul(b05, s[0][c]) ^ FFMul(b02, s[1][c]) ^ FFMul(b03, s[2][c]) ^ FFMul(b04, s[3][c]);
                sp[2] = FFMul(b04, s[0][c]) ^ FFMul(b05, s[1][c]) ^ FFMul(b02, s[2][c]) ^ FFMul(b03, s[3][c]);
                sp[3] = FFMul(b03, s[0][c]) ^ FFMul(b04, s[1][c]) ^ FFMul(b05, s[2][c]) ^ FFMul(b02, s[3][c]);
                for (int i = 0; i < 4; i++)
                    s[i][c] = (byte)(sp[i]);
            }
            return s;
        }

        private byte[][] MixColumns(byte[][] s)
        {
            byte[][] res = new byte[4][];
            byte[] sp = new byte[4];
            for (int c = 0; c < 4; c++)
            {
                sp[0] = (byte)(FFMul(MulKef[0][0], s[0][c]) ^ FFMul(MulKef[0][1], s[1][c]) ^ FFMul(MulKef[0][2], s[2][c]) ^ FFMul(MulKef[0][3], s[3][c]));
                sp[1] = (byte)(FFMul(MulKef[1][0], s[0][c]) ^ FFMul(MulKef[1][1], s[1][c]) ^ FFMul(MulKef[1][2], s[2][c]) ^ FFMul(MulKef[1][3], s[3][c]));
                sp[2] = (byte)(FFMul(MulKef[2][0], s[0][c]) ^ FFMul(MulKef[2][1], s[1][c]) ^ FFMul(MulKef[2][2], s[2][c]) ^ FFMul(MulKef[2][3], s[3][c]));
                sp[3] = (byte)(FFMul(MulKef[3][0], s[0][c]) ^ FFMul(MulKef[3][1], s[1][c]) ^ FFMul(MulKef[3][2], s[2][c]) ^ FFMul(MulKef[3][3], s[3][c]));
                for (int i = 0; i < 4; i++)
                    s[i][c] = sp[i];
            }
            return s;
        }

        public byte FFMul(byte a, byte b)
        {
            byte aa = a, bb = b, r = 0, t;
            while (aa != 0)
            {
                if ((aa & 1) != 0)
                    r = (byte)(r ^ bb);
                t = (byte)(bb & 0x80);
                bb = (byte)(bb << 1);
                if (t != 0)
                    bb = (byte)(bb ^ 0x1b);
                aa = (byte)((aa & 0xff) >> 1);
            }
            return r;
        }

        private byte[][] GenerateWordKeys(byte[][] keys)
        {
            byte[][] w = new byte[44][];
            for(int i = 0; i < 4; i++)
            {
                byte[] temp = new byte[4];
                for(int j = 0; j < 4; j++)
                {
                    temp[j] = keys[j][i];
                }
                w[i] = temp;
            }
            
            for(int i = 4; i < 44; i++)
            {
                byte[] temp = new byte[4];
                if(i % 4 == 0)
                {
                    
                    w[i] = XOR(SubWord(RotWord(w[i - 1])), Rcon[i / 4]);
                    w[i] = XOR(w[i], w[i - 4]);
                }
                else
                {
                    w[i] = XOR(w[i - 4], w[i - 1]);
                }
            }
            return w;
        }

        private byte[] XOR(byte[] vs1, byte[] vs2)
        {
            byte[] res = new byte[4];
            for(int i = 0; i < 4; i++)
            {
                res[i] = (byte)(vs1[i] ^ vs2[i]);
            }
            return res;
        }

        private static byte[] RotWord(byte[] input)
        {
            PrintKeys(input.Skip(1).Concat(input.Take(1)).ToArray());
            return input.Skip(1).Concat(input.Take(1)).ToArray();
        }

        private byte[][] GenerateSubKeys(byte[] key)
        {
            byte[][] rez = new byte[4][];
            for (int i = 0; i < 4; i++)
                rez[i] = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    rez[j][i] = key[4 * i + j];
                }
            }
            return rez;
        }

        private byte[][] AddRoundKey(byte[][] state, byte[][] w, int round)
        {
            byte[][] tmp = new byte[4][];
            for (int i = 0; i < Nb; i++)
                tmp[i] = new byte[4];

            for (int c = 0; c < Nb; c++)
            {
                for (int l = 0; l < 4; l++)
                    tmp[l][c] = (byte)(state[l][c] ^ w[round * Nb + c][l]);
            }

            return tmp;
        }

        public override string GenerateKey()
        {
            throw new NotImplementedException();
        }

        public override string KeyView()
        {
            BitArray arr = new BitArray(Key);
            StringBuilder rez = new StringBuilder();
            foreach (bool b in arr)
            {
                int bit = b ? 1 : 0;
                rez.Append(bit);
            }
            return rez.ToString();
        }

        private static byte[] Split(string plaintext, int v)
        {
            string[] rez = new string[plaintext.Length / 2];
            int j = 0;
            for (int i = 0; i < plaintext.Length; i += 2)
                rez[j++] = plaintext.Substring(i, 2);

            return rez.Select(i => Convert.ToByte(i, 16)).ToArray();
        }
    }
}

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

        public override string Group => "Комбинационные шифры";

        private int Nb = 4;
        private int Nk = 4;
        private int Nr;

        private readonly int BLOCK_SIZE = 16;

        private static byte[] Sbox = Tables.AES_Sbox, InvSbox = Tables.AES_InvSbox;
        private static byte[][] Rcon = Tables.Rcon;
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
            string key_ = CheckKey(config.Key);

            byte[] text = Encoding.Unicode.GetBytes(cipherText);
            byte[] key = Encoding.Unicode.GetBytes(key_);
            if (flagD == true && encrypt != null)
            {
                text = encrypt;
                encrypt = null;
                flagD = false;
            }
            byte[][] keys = GenerateSubKeys(key);
            w = GenerateWordKeys(keys);

            byte[] plainText = new byte[text.Length];
            Parallel.For(0, text.Length / BLOCK_SIZE, i =>
            {
                byte[] block = new byte[BLOCK_SIZE];
                Array.Copy(text, i * BLOCK_SIZE, block, 0, BLOCK_SIZE);
                Array.Copy(DecryptBlock(block), 0, plainText, i * block.Length, block.Length);
            });
            plainText = ClearBlocks(plainText);
            return Encoding.Unicode.GetString(plainText);
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
                    state[j][i] = inp[4 * i + j]; // записываем входной блок в таблицу по столбцам
                }
            }

            state = AddRoundKey(state, w, Nr); // XOR с раундовым ключом

            for (int i = Nr - 1; i >= 1; i--)
            {
                state = InvShiftRows(state); // циклический сдвиг строк таблицы
                state = InvSubBytes(state); // табличная замена значений таблицы
                state = AddRoundKey(state, w, i); // XOR с раундовым ключом
                state = InvMixColumns(state); // Умножение столбца таблицы на полином
            }
            state = InvSubBytes(state);
            state = InvShiftRows(state);
            state = AddRoundKey(state, w, 0);

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    temp[i * 4 + j] = state[j][i]; // запись из таблицы в массив
                }
            }
            return temp;
        }
        public override string Encrypt(string plainText, Config config)
        {
            string key_ = CheckKey(config.Key);
            byte[] key = Encoding.Unicode.GetBytes(key_);
            byte[] text = Encoding.Unicode.GetBytes(plainText);
            text = DopBlock(text);

            byte[][] keys = GenerateSubKeys(key); // записать ключ по столбцам
            w = GenerateWordKeys(keys); // вычислить расширенный ключ

            byte[] cipherText = new byte[text.Length];
            // шифрование входных блоков
            Parallel.For(0, text.Length / BLOCK_SIZE, i =>
            {
                byte[] block = new byte[BLOCK_SIZE];
                Array.Copy(text, i * BLOCK_SIZE, block, 0, BLOCK_SIZE);
                Array.Copy(EncryptBlock(block), 0, cipherText, i * block.Length, block.Length);
            });
            flagD = false;
            encrypt = cipherText;
            return Encoding.Unicode.GetString(cipherText);
        }
        // функция шифрования блока
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
                    state[j][i] = inp[4 * i + j]; // записать входной блок в таблицу
                }
            }

            state = AddRoundKey(state, w, 0); // XOR с раундовым ключом

            for (int i = 1; i < Nr; i++)
            {
                state = SubBytes(state); // табличная замена значений таблицы
                state = ShiftRows(state); // циклический сдвиг строк таблицы
                state = MixColumns(state); // Умножение столбца таблицы на полином
                state = AddRoundKey(state, w, i);
            }
            state = SubBytes(state);
            state = ShiftRows(state);
            state = AddRoundKey(state, w, Nr); // XOR с раундовым ключом

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    temp[i * 4 + j] = state[j][i];
                }
            }

            return temp;
        }
        // замена 4-байтного слова по таблице
        private byte[] SubWord(byte[] state)
        {
            return state.Select(i => Sbox[i]).ToArray(); 
        }
        // замена блока по таблице
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
        // обратная замена блока по таблице
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
        /*
            Циклический сдвиг строк таблицы.
            берем байты, начиная с индекса номера текущей строки, соединяем
            с байтами, взятыми из начала строки до индекса текущей строки.
        */
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
        /*
            Циклический обратный сдвиг строк таблицы.
            Алгоритм похож на предыдущий сдвиг, только двигаем в другую сторону.
        */
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
        // коэффициенты для умножения в поле Галуа
        private byte[][] MulKef = new byte[][]
        {
            new byte[] {0x_02,0x_03,0x_01,0x_01},
            new byte[] {0x_01,0x_02,0x_03,0x_01},
            new byte[] {0x_01,0x_01,0x_02,0x_03},
            new byte[] {0x_03,0x_01,0x_01,0x_02}
        };
        /*
            Умножение столбца таблицы на полином.
            Берем столбец таблицы, каждое значение из этого столбца умножаем на коэффициент в поле Галуа,
            XOR'им полученные результаты и записываем как новое значение данного столбца.
            Данную операцию проделываем на каждом значении столбца.
        */
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
        /*
            Умножение столбца таблицы на полином.
            Берем столбец таблицы, каждое значение из этого столбца умножаем на коэффициент в поле Галуа,
            XOR'им полученные результаты и записываем как новое значение данного столбца.
            Данную операцию проделываем на каждом значении столбца.
        */
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

        // умножение в поле Галуа.
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
            for (int i = 0; i < 4; i++)
            {
                byte[] temp = new byte[4];
                for (int j = 0; j < 4; j++)
                {
                    temp[j] = keys[j][i]; // записываем входной массив в первые 4 слова
                }
                w[i] = temp;
            }

            for (int i = 4; i < 44; i++)
            {
                byte[] temp = new byte[4];
                if (i % 4 == 0) // если слово является первым в столбце
                {

                    w[i] = XOR(SubWord(RotWord(w[i - 1])), Rcon[i / 4]);
                    w[i] = XOR(w[i], w[i - 4]);
                }
                else // для остальных 3 слов
                {
                    w[i] = XOR(w[i - 4], w[i - 1]);
                }
            }
            return w;
        }
        // XOR двух массивов байт
        private byte[] XOR(byte[] vs1, byte[] vs2)
        {
            byte[] res = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                res[i] = (byte)(vs1[i] ^ vs2[i]);
            }
            return res;
        }
        // Циклический сдвиг строки на 1.
        private static byte[] RotWord(byte[] input)
        {
            return input.Skip(1).Concat(input.Take(1)).ToArray();
        }
        // Запись исходного ключа в таблицу по столбцам
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
        // XOR таблицы с расширенным ключом
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
            byte[] key = new byte[128 / 8];
            new Random().NextBytes(key);
            return Encoding.Unicode.GetString(key);
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
    }
}

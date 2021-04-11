﻿using System;
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
        byte[,] w;
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
            byte[] key = Encoding.Unicode.GetBytes(_key);
            byte[] text = Encoding.Unicode.GetBytes(cipherText);
            if (flagD == true && encrypt != null)
            {
                text = encrypt;
                encrypt = null;
                flagD = false;
            }

            Sbox = Tables.AES_Sbox;
            InvSbox = Tables.AES_InvSbox;
            Rcon = Tables.Rcon;

            byte[,] keys = GenerateSubKeys(key);
            w = GenerateWordKeys(keys);
            byte[][] blocks = GiveAllBlocks(text);

            byte[] plainText = new byte[blocks.Length * 16];
            for (int i = 0; i < blocks.Length; i++)
            {
                byte[] block = DecryptBlock(blocks[i]);
                Array.Copy(block, 0, plainText, i * block.Length, block.Length);
            }
            plainText = ClearBlocks(plainText);
            return Encoding.Unicode.GetString(plainText);
        }
        /*
            Фукнция расшифрования блока.
            Переписываем входные байты с транспонированием в state.
            Производим сложение с раундовым ключом,
            в цикле производим обратные преобразования,
            1 - сдвиг байт
            2 - подстановка байт
            3 - сложение с раундовым ключом
            4 - перемешивание байт в столбцах
            в финале производим преобразования без перемешивания байт в столбцах
        */
        private byte[] DecryptBlock(byte[] inp)
        {
            byte[] temp = new byte[inp.Length];

            byte[,] state = new byte[4, Nb];
            for (int i = 0; i < inp.Length; i++)
                state[i / 4, i % 4] = inp[i % 4 * 4 + i / 4];

            state = AddRoundKey(state, w, Nr);
            for (int i = Nr - 1; i >= 1; i--)
            {
                state = InvShiftRows(state);
                state = InvSubBytes(state);
                state = AddRoundKey(state, w, i);
                state = InvMixColumns(state);
            }
            state = InvSubBytes(state);
            state = InvShiftRows(state);
            state = AddRoundKey(state, w, 0);

            for (int i = 0; i < temp.Length; i++)
                temp[i % 4 * 4 + i / 4] = state[i / 4, i % 4];

            return temp;
        }
        /*
            Функция шифрования.
            Получаем ключ и текст, переводим их в массив байт.
            Дополняем блок в случае необходимости.
            Транспонируем полученный ключ,
            Генерируем раундовые ключи,
            разбиваем текст на блоки.
            В цикле по количеству блоков шифруем блок.
        */
        public override string Encrypt(string plainText, Config config)
        {
            string _key = CheckKey(config.Key);
            byte[] key = Encoding.Unicode.GetBytes(_key);
            byte[] text = Encoding.Unicode.GetBytes(plainText);
            text = DopBlock(text);
            Sbox = Tables.AES_Sbox;
            InvSbox = Tables.AES_InvSbox;
            Rcon = Tables.Rcon;

            byte[,] keys = GenerateSubKeys(key);
            w = GenerateWordKeys(keys);
            byte[][] blocks = GiveAllBlocks(text);

            byte[] cipherText = new byte[blocks.Length * 16];
            for(int i = 0; i < blocks.Length; i++)
            {
                byte[] block = EncryptBlock(blocks[i]);
                Array.Copy(block, 0, cipherText, i * block.Length, block.Length);
            }
            flagD = false;
            encrypt = cipherText;
            return Encoding.Unicode.GetString(cipherText);
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
        /*
            Процедура шифрования блока.
            Фомируем из блока входных байт массив state, транспонируя исходные байты.
            Складываем state с раундовым ключом,
            в цикле по числу раундов:
            1 - замена байт по таблице
            2 - циклический сдвиг строки влево на 1,2,3 байта
            3 - перемешивание байтов 
            4 - сложение с раундовым ключом
            На последнем шаге применяем вышеперечисленные процедуры без перемешивания байтов.
        */
        private byte[] EncryptBlock(byte[] inp)
        {
            byte[] temp = new byte[inp.Length];

            byte[,] state = new byte[4, Nb];
            for (int i = 0; i < inp.Length; i++)
                state[i / 4, i % 4] = inp[i % 4 * 4 + i / 4];

            state = AddRoundKey(state, w, 0);
            for(int i = 1; i < Nr; i++)
            {
                state = SubBytes(state);
                state = ShiftRows(state);
                state = MixColumns(state);
                state = AddRoundKey(state, w, i);
            }
            state = SubBytes(state);
            state = ShiftRows(state);
            state = AddRoundKey(state, w, Nr);

            for (int i = 0; i < temp.Length; i++)
                temp[i % 4 * 4 + i / 4] = state[i / 4, i % 4];

            return temp;
        }
        /*
            Процедура замены байтов на байты из таблицы.
            В цикле по state
            Получаем байт из таблицы замены под индексом текущего байта state         
        */
        private byte[,] SubBytes(byte[,] state)
        {
            byte[,] tmp = new byte[4, 4];
            for (int row = 0; row < 4; row++)
                for (int col = 0; col < Nb; col++)
                    tmp[row, col] = Sbox[state[row, col]];

            return tmp;
        }
        /*
            Процедура замены байтов на байты из таблицы.
            В цикле по state
            Получаем байт из таблицы замены под индексом текущего байта state         
        */
        private byte[,] InvSubBytes(byte[,] state)
        {
            for (int row = 0; row < 4; row++)
                for (int col = 0; col < Nb; col++)
                    state[row, col] = InvSbox[state[row, col]];

            return state;
        }
        /*
            Процедура сдвига строк.
            Первая строка неподвижна,
            Остальные циклически сдвигаются на 1,2,3 байта соответственно влево.
        */
        private byte[,] ShiftRows(byte[,] state)
        {

            byte[] t = new byte[4];
            for (int r = 1; r < 4; r++)
            {
                for (int c = 0; c < Nb; c++)
                    t[c] = state[r, (c + r) % Nb];
                for (int c = 0; c < Nb; c++)
                    state[r, c] = t[c];
            }

            return state;
        }
        /*
            Процедура обратного сдвига строк.
            Первая строка неподвижна,
            Остальные циклически сдвигаются на 1,2,3 байта соответственно вправо
        */
        private byte[,] InvShiftRows(byte[,] state)
        {
            byte[] t = new byte[4];
            for (int r = 1; r < 4; r++)
            {
                for (int c = 0; c < Nb; c++)
                    t[(c + r) % Nb] = state[r, c];
                for (int c = 0; c < Nb; c++)
                    state[r, c] = t[c];
            }
            return state;
        }
        /*
            Обратная процедура перемешивания матрицы.
            Каждый столбец исходной матрицы принимается за многочлен и умножается на фиксированный многочлен над полем GF(2^8)             
        */
        private byte[,] InvMixColumns(byte[,] s)
        {
            int[] sp = new int[4];
            byte b02 = (byte)0x0e, b03 = (byte)0x0b, b04 = (byte)0x0d, b05 = (byte)0x09;
            for (int c = 0; c < 4; c++)
            {
                sp[0] = FFMul(b02, s[0, c]) ^ FFMul(b03, s[1, c]) ^ FFMul(b04, s[2, c]) ^ FFMul(b05, s[3, c]);
                sp[1] = FFMul(b05, s[0, c]) ^ FFMul(b02, s[1, c]) ^ FFMul(b03, s[2, c]) ^ FFMul(b04, s[3, c]);
                sp[2] = FFMul(b04, s[0, c]) ^ FFMul(b05, s[1, c]) ^ FFMul(b02, s[2, c]) ^ FFMul(b03, s[3, c]);
                sp[3] = FFMul(b03, s[0, c]) ^ FFMul(b04, s[1, c]) ^ FFMul(b05, s[2, c]) ^ FFMul(b02, s[3, c]);
                for (int i = 0; i < 4; i++)
                    s[i, c] = (byte)(sp[i]);
            }

            return s;
        }
        /*
            Процедура перемешивания матрицы.
            Каждый столбец исходной матрицы принимается за многочлен и умножается на фиксированный многочлен над полем GF(2^8)
        */
        private byte[,] MixColumns(byte[,] s)
        {
            int[] sp = new int[4];
            byte b02 = (byte)0x02, b03 = (byte)0x03;
            for (int c = 0; c < 4; c++)
            {
                sp[0] = FFMul(b02, s[0, c]) ^ FFMul(b03, s[1, c]) ^ s[2, c] ^ s[3, c];
                sp[1] = s[0, c] ^ FFMul(b02, s[1, c]) ^ FFMul(b03, s[2, c]) ^ s[3, c];
                sp[2] = s[0, c] ^ s[1, c] ^ FFMul(b02, s[2, c]) ^ FFMul(b03, s[3, c]);
                sp[3] = FFMul(b03, s[0, c]) ^ s[1, c] ^ s[2, c] ^ FFMul(b02, s[3, c]);
                for (int i = 0; i < 4; i++)
                    s[i, c] = (byte)(sp[i]);
            }

            return s;
        }
        /*
             Умножение в поле GF(2^8)
        */
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
        /*
            Генерация раундовых ключей.
            первые 4 слова заполнены ключом шифра,
            остальные вычисляются по формуле w[i+5] = w[i+4] ^ w[i+1].
            Раунд вычисляет 4 слова, каждое первое слово каждого раунда
            использует циклический сдвиг слова влево на один байт,
            а затем табличная замена полученного слова.
        */
        private byte[,] GenerateWordKeys(byte[,] keys)
        {
            byte[,] w = new byte[44,4];
            for(int i = 0; i < 4; i++)
                for(int j = 0; j < 4; j++)
                {
                    w[i, j] = keys[j, i]; 
                }
            
            for(int i = 4; i < 44;i++)
                for(int j = 0; j < 4; j++)
                {
                    byte[] temp = new byte[4];
                    for (int k = 0; k < 4; k++) temp[j] = w[i - 1, k];

                    if (i % 4 == 0)
                    {
                        temp = SubWord(RotWord(temp));
                        for (int l = 0; l < 4; l++)
                        {
                            temp[l] = (byte)(temp[l] ^ (Rcon[i / 4][l]));
                            w[i, l] = temp[l];
                        }

                    }
                    else
                    {
                        for(int l = 0; l < 4; l++)
                        {
                            w[i, l] = (byte)(w[i - 1, l] ^ w[i - 4, l]);
                        }
                    }
                }

            return w;


        }
        /*
            Циклический сдвиг четырехбайтового слова влево на один байт 
        */
        private static byte[] RotWord(byte[] input)
        {
            byte[] tmp = new byte[input.Length];
            byte t = input[0];
            for (int i = 0; i < 3; i++)
                tmp[i] = input[i + 1];
            tmp[3] = t;

            return tmp;
        }
        /*
            замена каждого байта в соответствии с таблицей
        */
        private static byte[] SubWord(byte[] inp)
        {
            byte[] tmp = new byte[inp.Length];

            for (int i = 0; i < tmp.Length; i++)
                tmp[i] = (byte)(Sbox[inp[i]]);

            return tmp;
        }
        /*
            Разбить входной текст на блоки 
        */
        private byte[][] GiveAllBlocks(byte[] text)
        {
            int len = text.Length / BLOCK_SIZE + 1;
            int j = 0, i = 0;
            byte[][] res = new byte[len][];
            for (; j < len; j++)
            {
                if (j * BLOCK_SIZE + BLOCK_SIZE < text.Length)
                {
                    res[j] = new byte[BLOCK_SIZE];
                    for (; i < j * BLOCK_SIZE + BLOCK_SIZE; i++)
                    {
                        res[j][i % BLOCK_SIZE] = text[i];
                    }
                }
                else
                {
                    res[j] = new byte[text.Length - (j * BLOCK_SIZE)];
                    for (int l = 0; l < text.Length - (j * BLOCK_SIZE); l++)
                    {
                        res[j][l] = text[i++];
                    }
                }
            }
            return res;
        }
        /*
            Исходный ключ переписываем с транспонированием 
        */
        private byte[,] GenerateSubKeys(byte[] key)
        {
            byte[,] rez = new byte[Nk, Nk];
            for (int i = 0; i < Nk; i++)
            {
                for (int j = 0; j < Nk; j++)
                {
                    rez[i,j] = key[i + 4 * j];
                }
            }
            return rez;
        }
        /*
            Процедура сложения матрицы с раундовым ключом.
            Побитово сложить столбец исходной матрицы с 4-байтовым фрагментом ключа
        */
        private byte[,] AddRoundKey(byte[,] state, byte[,] w, int round)
        {
            byte[,] tmp = new byte[Nb, Nb];
            for(int i = 0; i < Nb; i++)
            {
                for(int j = 0; j < Nb; j++)
                {
                    tmp[j, i] = (byte)(state[j, i] ^ w[4 * round + i,j]);
                }
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
    }
}

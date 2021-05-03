using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWP.Помошники;

namespace UWP.Алгоритмы
{
    class Magma : Algorithm
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

        public int KeyLength
        {
            get
            {
                return KEY_LENGTH;
            }
        }

        public string Key { get; private set; }

        public override string Name => "Шифр Магма";

        public override string DefaultKey => "1234567890abcdef";

        public override bool IsReplaceText => false;

        public override string Group => "Комбинационные шифры";

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
        private string mode;
        private byte[] encrypt;

        /*
            Функция расшифрования блока. 
        */
        public byte[] Decrypt(byte[] data, byte[] key)
        {
            if (mode == "Режим простой замены" || string.IsNullOrEmpty(mode))
            {
                flagD = true;
                return SimpleCipher(data, key);
            }
            else if (mode == "Режим гаммирования")
                return GammaCipher(data, key);
            else if (mode == "Режим гаммирования по выходу")
                return GammaOut(data, key);
            else if (mode == "Режим гаммирования по шифртексту")
            {
                flagD = true;
                return GammaText(data, key);
            }
            

            return new byte[0];
        }
        /*
            Функция шифрования блока. 
        */
        public byte[] Encrypt(byte[] data, byte[] key)
        {
            if (mode == "Режим простой замены" || string.IsNullOrEmpty(mode))
                return SimpleCipher(data, key);
            else if (mode == "Режим гаммирования")
                return GammaCipher(data, key);
            else if (mode == "Режим гаммирования по выходу")
                return GammaOut(data, key);
            else if (mode == "Режим гаммирования по шифртексту")
                return GammaText(data, key);

            return new byte[0];
        }
        /*
            Шифрование в режиме гаммирования с обратной связью по шифртексту.
            Берем старшую часть синхропосылки,
            шифруем в режиме простой замены.
            Складываем по модулю 2 гамму и блок открытого текста.
            Сдвигаем синхропосылку в сторону старшей части,
            на место младшей части ставим:
            1) При шифровании ставим полученный блок зашифрованного текста
            2) При расшифровании ставим полученный блок зашифрованного текста
        */
        private byte[] GammaText(byte[] data, byte[] key)
        {
            byte[] result = new byte[data.Length];
            byte[] left = CalcSyn();
            byte[] gamma = new byte[8];
            if(flagD == false)
                gamma = SimpleCipher(left, key);
            else
            {
                flagD = false;
                gamma = SimpleCipher(left, key);
                flagD = true;
            }
            byte[] resBlock = XOR(gamma, data);
            if (flagD == false)
                IncOut(resBlock);
            else
                IncOut(data);
            flagD = false;
            Array.Copy(resBlock, 0, result, 0, 8);
            return result;
        }
        /*
            Шифрование в режиме гаммирования с обратной связью по выходу.
            Берем старшую часть синхропосылки,
            шифруем её в режиме простой замены.
            Сдвигаем синхропосылку в сторону старших разрядов,
            на место младших разрядов ставим полученную гамму.
            Складываем гамму и полученный блок по модулю 2.
        */
        private byte[] GammaOut(byte[] data, byte[] key)
        {
            byte[] result = new byte[data.Length];
            byte[] left = CalcSyn();
            byte[] gamma = SimpleCipher(left, key);
            IncOut(gamma);
            Array.Copy(XOR(gamma, data), 0, result, 0, 8);
            return result;
        }
        /*
            Функция сдвига синхропосылки.
            Младшую часть синхропосылки пишем в старшую массива,
            на место младших разрядов массива пишем полученный блок.
        */
        private void IncOut(byte[] gamma)
        {
            byte[] temp = new byte[16];
            Array.Copy(s, 8, temp, 0, 8);
            Array.Copy(gamma, 0, temp, 8, 8);
            Array.Copy(temp, 0, s, 0, 16);
        }
        /*
            Вернуть старшую часть синхропосылки. 
        */
        private byte[] CalcSyn()
        {
            byte[] temp = new byte[8];
            for(int i = 0; i < 8; i++)
            {
                temp[i] = s[i];
            }
            return temp;
        }
        /*
            Шифрование в режиме гаммирования.
            Шифруем синхропосылку в режиме простой замены,
            увеличиваем синхропосылку на 1.
            Складываем полученный блок и гамму по модулю 2.
        */
        private byte[] GammaCipher(byte[] data,byte[] key)
        {
            byte[] gamma = SimpleCipher(s, key);
            IncGamma();
            return XOR(gamma, data);
        }
        // сложение по модулю 2.
        private byte[] XOR(byte[] block, byte[] data)
        {
            byte[] res = new byte[8];
            for (int i = 0; i < 8; i++)
                res[i] = (byte)(block[i] ^ data[i]);

            return res;
        }
        // увеличение синхропосылки на 1.
        private void IncGamma()
        {
            ulong t = BitConverter.ToUInt64(s, 0);
            t += 1;
            s = BitConverter.GetBytes(t);
        }
        /*
            Функция разбиения полученного ключа на 8 частей 
        */
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
        /*
            Шифрование в режиме простой замены.
            Разбиваем блок на левую и правую часть,
            в цикле в 31 раунд:
            получаем индекс ключа исходя из текущего раунда.
            складываем левую часть блока с правой, к которой применили функцию G.
            Меняем части местами
            В последнем раунде повторяем действия без смены частей местами.
        */
        private byte[] SimpleCipher(byte[] data, byte[] key)
        {
            byte[] dataR = new byte[data.Length];
            Array.Copy(data, dataR, data.Length);
            Array.Reverse(dataR);
            uint a0 = BitConverter.ToUInt32(dataR, 0);
            uint a1 = BitConverter.ToUInt32(dataR, 4);

            byte[] result = new byte[8];

            for (int i = 0; i < 31; i++)
            {
                int keyIndex = 0;
                if (flagD == false) keyIndex = (i < 24) ? i % 8 : 7 - (i % 8);
                if (flagD == true && (mode == "Режим простой замены" || string.IsNullOrEmpty(mode))) 
                    keyIndex = (i < 8) ? i % 8 : 7 - (i % 8);
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
        /*
            Складываем часть блока и ключ,
            заменяем результат по таблице замен и 
            циклически сдвигаем влево на 11.
        */
        private uint funcG(uint a, uint k)
        {
            uint c = a + k;
            uint tmp = funcT(c);
            return (tmp << 11) | (tmp >> 21);
        }
        /*
            Замена по таблице. 
        */
        private uint funcT(uint a)
        {
            uint res = 0;

            for (int i = 0; i < 8; i++)
            {
                res ^= (uint)(_sBox[i][a >> (4 * i) & 0b_1111] << (4 * i));
            }

            return res;
        }
    
        /*
            Функция дополнения блока.
            В конце неполного блока(если все блоки полные, добавляем новый)
            добавляем 1, затем оставшуюся часть заполняем 0.
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
            Функция генерации рандомного ключа
        */
        public override string GenerateKey()
        {
            string alf = "abcdefghijklmnopqrstuvwxyz1234567890";
            StringBuilder str = new StringBuilder(16);
            Random rand = new Random();
            for (int i = 0; i < str.Capacity; i++)
            {
                int index = rand.Next(1, alf.Length);
                char symbol = alf[index];
                str.Append(symbol);
            }

            return str.ToString();
        }

        public override string KeyView()
        {
            byte[] key = Encoding.Unicode.GetBytes(Key);
            return string.Join(" ", key.Select(x => Convert.ToString(x, 2)));
        }
        /*
            Функция шифрования текста.
            Получаем текст, ключ и синхропосылку.
            Расчитываем раундовые ключи.
            Дополняем блок.
            Разбиваем текст на блоки и шифруем каждый.
        */
        public override string Encrypt(string plainText, Config config)
        {
            mode = config.Mode;
            s = Encoding.Unicode.GetBytes(config.Synhro);
            Key = CheckKey(config.Key);
            byte[] text = Encoding.Unicode.GetBytes(plainText);
            byte[] key = Encoding.Unicode.GetBytes(config.Key);
            CalculateSubKeys(key);

            text = DopBlock(text);
            byte[] result = new byte[text.Length];
            for(int i = 0; i < text.Length / BLOCK_SIZE; i++)
            {
                byte[] block = new byte[BLOCK_SIZE];
                Array.Copy(text, i * BLOCK_SIZE, block, 0, BLOCK_SIZE);
                Array.Copy(Encrypt(block, key), 0, result, i * BLOCK_SIZE, BLOCK_SIZE);
            }
            encrypt = result;
            return Encoding.Unicode.GetString(result);
        }
        // удаление дополненной части расшифрованного текста
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
            Функция расшифрования.
            Аналогична функции шифрования.
        */
        public override string Decrypt(string cipherText, Config config)
        {
            mode = config.Mode;
            s = Encoding.Unicode.GetBytes(config.Synhro);
            if(mode == "Режим гаммирования") Array.Copy(s, 0, s, 0, 8);
            Key = CheckKey(config.Key);
            byte[] text = Encoding.Unicode.GetBytes(cipherText);
            if(encrypt != null)
            {
                Array.Copy(encrypt, text, encrypt.Length);
                encrypt = null;
            }
            byte[] key = Encoding.Unicode.GetBytes(config.Key);
            CalculateSubKeys(key);

            byte[] result = new byte[text.Length];
            for(int i = 0; i < text.Length / BLOCK_SIZE; i++)
            {
                byte[] block = new byte[BLOCK_SIZE];
                Array.Copy(text, i * BLOCK_SIZE, block, 0, BLOCK_SIZE);
                Array.Copy(Decrypt(block, key), 0, result, i * BLOCK_SIZE, BLOCK_SIZE);
            }
            result = ClearBlocks(result);
            flagD = false;
            return Encoding.Unicode.GetString(result);
        }

        public override string CheckKey(string key)
        {
            byte[] k = Encoding.Unicode.GetBytes(key);
            if (k.Length == 0)
                throw new Error(Error.MissingKey);
            if (k.Length != 32)
                throw new Error(Error.KeyLength32Byte);

            return key;
        }
    }
}

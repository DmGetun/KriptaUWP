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
        private byte[][] _subKeys;

        private byte[] _pi = Tables._pi;
        private byte[] _reversepi = Tables._reversePi;

        private readonly byte[] _lFactors = {
            0x94, 0x20, 0x85, 0x10, 0xC2, 0xC0, 0x01, 0xFB,
            0x01, 0xC0, 0xC2, 0x10, 0x85, 0x20, 0x94, 0x01
            };

        private const int BLOCK_SIZE = 16;
        private const int KEY_LENGTH = 32;

        private const int SUB_LENGTH = KEY_LENGTH / 2;

        private byte[] resultArray;

        public override string Name => "Шифр Кузнечик";

        public override string DefaultKey => "1234567890123456";

        public override bool IsReplaceText => false;

        public byte[] Key { get; private set; }

        public override string Group => "Комбинационные шифры";

        /*
            Проверка ключа на длину в 256 бит 
        */
        public override string CheckKey(string key)
        {
            byte[] k = Encoding.Unicode.GetBytes(key);
            if (k.Length != 32)
                throw new Error(Error.KeyLength32Byte);
            return key;
        }
        /*
            Функция расшифрования.
            Получаем текст и ключ,
            вычисляем раундовые ключи,
            к каждому блоку применяем функцию расшифрования блока.
        */
        public override string Decrypt(string cipherText, Config config)
        {
            string keyString = config.Key;
            byte[] keys = Encoding.Unicode.GetBytes(keyString);
            Key = keys;
            byte[] text = Encoding.Unicode.GetBytes(cipherText);
            text = resultArray;
            
            byte[] result = new byte[text.Length];
            SetKey(keys);
            Parallel.For(0, text.Length / BLOCK_SIZE, i =>
            {
                byte[] block = new byte[BLOCK_SIZE];
                Array.Copy(text, i * BLOCK_SIZE, block, 0, BLOCK_SIZE);
                Array.Copy(Decrypt(block), 0, result, i * BLOCK_SIZE, BLOCK_SIZE);
            });
            result = ClearBlocks(result);
            return Encoding.Unicode.GetString(result);
        }
        /*
            Функция шифрования.
            Получаем текст и ключ,
            вычисляем раундовые ключи,
            к каждому блоку применяем функцию шифрования блока. 
        */
        public override string Encrypt(string plainText, Config config)
        {
            string keyString = CheckKey(config.Key);
            byte[] keys = Encoding.Unicode.GetBytes(keyString);
            Key = keys;
            byte[] text = Encoding.Unicode.GetBytes(plainText);
            text = DopBlock(text);
            byte[] result = new byte[text.Length];
            SetKey(keys);
            Parallel.For(0, text.Length / BLOCK_SIZE, i =>
            {
                byte[] block = new byte[BLOCK_SIZE];
                Array.Copy(text, i * BLOCK_SIZE, block, 0, BLOCK_SIZE);
                Array.Copy(Encrypt(block), 0, result, i * BLOCK_SIZE, BLOCK_SIZE);
            });

            resultArray = result;
            return Encoding.Unicode.GetString(result);
        }

        /*
            Функция шифрования блока.
            9 раз применяем к блоку процедуру LSX с раундовым ключом.
            На 10 раз складываем блок и ключ раунда по модулю 2.
        */
        public byte[] Encrypt(byte[] data)
        {
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
        /*
            Функция расшифрования блока.
            9 раз применяем на блок процедуру, обратную LSX с раундовым ключом,
            начиная отсчет ключей с конца.
            На 10 раз выполняем сложение блока с ключом по модулю 2.
        */
        public byte[] Decrypt(byte[] data)
        {
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
        /*
            Функция получения раундовых ключей из исходного.
            Первые два ключа равны половинам исходного,
            остальные вычисляются с помощью процедур C и F.
        */
        public void SetKey(byte[] key)
        {
            _subKeys = new byte[10][];
            for (int i = 0; i < 10; i++)
            {
                _subKeys[i] = new byte[SUB_LENGTH];
            }

            byte[] x = new byte[SUB_LENGTH];
            byte[] y = new byte[SUB_LENGTH];

            byte[] c = new byte[SUB_LENGTH];

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
        // умножение в поле Галуа.
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
        /*
            Процедура S.
            Заменяет каждый байт входного массива на байт из таблицы замен.
        */
        private void S(ref byte[] data)
        {
            data = data.Select(i => _pi[i]).ToArray();
        }
        /*
            Обратная процедура S.
            Заменяет каждый байт входного массива на байт из обратной таблицы замен.
        */
        private void ReverseS(ref byte[] data)
        {
            data = data.Select(i => _reversepi[i]).ToArray();
        }
        /*
            Процедура X.
            Выполняет сложение по модулю 2 двух массивов
        */
        private void X(ref byte[] result, ref byte[] data)
        {
            for (int i = 0; i < result.Length; i++)
            {
                result[i] ^= data[i];
            }
        }
        /*
            Операция l.
            Выполняет сложение по модулю 2 произведения каждого байта массива на константу в поле Галуа.
        */
        private byte l(ref byte[] data)
        {
            byte x = data[15];
            for (int i = 14; i >= 0; i--)
            {
                x ^= kuz_mul_gf256_slow(data[i], _lFactors[i]);
            }
            return x;
        }
        /*
            Процедура R.
            Сдвигает полученный массив в сторону младших разярдов,
            а на место старшего ставит результат процедуры l исходного массива
            в поле Галуа.
        */
        private void R(ref byte[] data)
        {
            byte z = l(ref data);
            for (int i = 15; i > 0; i--)
            {
                data[i] = data[i - 1];
            }
            data[0] = z;
        }
        /*
            Обратная процедура R.
            Выполняет сдвиг массива в сторону старших разрядов,
            а на место младшего ставит результат процедуры l сдвинутого массива в поле Галуа
        */
        private void ReverseR(ref byte[] data)
        {
            byte z = data[0];
            byte[] temp = new byte[16];

            for (int i = 15; i > 0; i--)
            {
                temp[i - 1] = data[i];
            }

            temp[15] = z;
            byte r = l(ref temp);
            temp[15] = r;
            Array.Copy(temp, 0, data, 0, 16);
        }
        /*
            Процедура L.
            Применяет процедуру R 16 раз.
        */
        private void L(ref byte[] data)
        {
            for (int i = 0; i < 16; i++)
            {
                R(ref data);
            }
        }
        /*
            Обратная процедура L.
            Применяет обратную процедуру R 16 раз.
        */
        private void ReverseL(ref byte[] data)
        {
            for (int i = 0; i < 16; i++)
            {
                ReverseR(ref data);
            }
        }
        /*
            Процедура F.
            Применяет процедуру LSX и X на два переданных ключа.
        */
        private void F(ref byte[] k, ref byte[] a1, ref byte[] a0)
        {
            byte[] temp = new byte[SUB_LENGTH];

            LSX(ref temp, ref k, ref a1);
            X(ref temp, ref a0);

            Array.Copy(a1, a0, SUB_LENGTH);
            Array.Copy(temp, a1, SUB_LENGTH);

        }
        /*
            Процедура LSX.
            Последовательно применяет процедуры X,S,L 
        */
        private void LSX(ref byte[] result, ref byte[] k, ref byte[] a)
        {
            Array.Copy(k, result, BLOCK_SIZE);
            X(ref result, ref a);
            S(ref result);
            L(ref result);
        }
        /*
            Обратная процедура LSX.
            Последовательно применяет процедуры X,L,S
        */
        private void ReverseLSX(ref byte[] result, ref byte[] k, ref byte[] a)
        {
            Array.Copy(k, result, BLOCK_SIZE);
            X(ref result, ref a);
            ReverseL(ref result);
            ReverseS(ref result);
        }
        /*
            Процедура вычисления константы.
            Константа получается применением процедуры L на индекс константы.
        */
        private void C(ref byte[] c, int i)
        {
            Array.Clear(c, 0, SUB_LENGTH);
            c[15] = (byte)i;
            L(ref c);
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
            Генерация рандомного ключа. 
        */
        public override string GenerateKey()
        {
            Random rand = new Random();
            byte[] key = new byte[KEY_LENGTH];
            for (int i = 0; i < KEY_LENGTH; i++)
                rand.NextBytes(key);
            Key = key;
            return Encoding.Unicode.GetString(key);

        }
        // вывод вида ключа
        public override string KeyView()
        {
            return BitConverter.ToString(Key);
        }

    }
}

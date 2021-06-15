using App3.Помошники;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using Tests.Алгоритмы;
using UWP.Алгоритмы;
using UWP.Помошники;

namespace Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            FastExponent(5,2206,22);
            
        }

        private static int FastExponent(uint bas, uint degree, uint mode)
        {
            string bin = Convert.ToString(degree, 2);
            uint[] numbers = new uint[bin.Length];
            numbers[0] = bas;
            string[] values = new string[numbers.Length];

            for(int i = 1,j = 0; i < Math.Pow(2,bin.Length); i <<= 1,j++)
            {
                string temp = $"a^{i}" + " ";
                Console.Write(temp);
                values[j] = temp;
            }
            Console.WriteLine();

            for(int i = 1; i < bin.Length; i++)
            {
                numbers[i] = (uint)(Math.Pow(numbers[i-1], 2) % mode);
            }

            for (int i = 0; i < numbers.Length; i++)
            {
                    Console.Write(numbers[numbers.Length - i - 1] + "".PadRight(values[i].Length));
            }
            Console.WriteLine();
            Console.WriteLine();

            for (int i = 0; i < numbers.Length; i++)
            {
                Console.Write(bin[i] + "".PadRight(4));
            }
            Console.WriteLine();

            for (int i = 0; i < numbers.Length; i++)
            {
                if(bin[i] == '1')
                    Console.Write(numbers[numbers.Length - i - 1] + "".PadRight(values[i].Length));
                else
                    Console.Write(" ".PadRight(values[i].Length));
            }

            return 0;
        }

        private static BigInteger GetHashMessage(string plainText, BigInteger p)
        {
            var alf = Alphabet.GenerateAlphabet();
            int h = 0;
            foreach (char s in plainText)
            {
                int index = Alphabet.GetSymbol(alf, s);
                h = (int)((Math.Pow((h + index), 2)) % (int)p);
            }
            return h;
        }

        private static void A52Test()
        {
            A52 a = new();
            string text = "НЕ";
            string key = "ЛЕХА";
            byte[] t = Encoding.Unicode.GetBytes(text);
            byte[] k = Encoding.Unicode.GetBytes(key);
            var cypher = a.Encrypt(t, k);
        }

        private static void SimpleTest()
        {
            string plain = "fedcba9876543210";
            string key = "ffeeddccbbaa99887766554433221100f0f1f2f3f4f5f6f7f8f9fafbfcfdfeff";

            byte[] text = Split(plain, 2);
            byte[] keys = Split(key, 2);

            MagmaClass a = new();
            string synhro = "1234567890abcdef234567890abcdef1";
            byte[] s = Split(synhro, 2);
            byte[] news = new byte[s.Length];
            Array.Copy(s, 0, news, 0, s.Length);
            byte[] rez = a.Encrypt(text, keys, s, "simple");
            byte[] sh = a.Decrypt(rez, keys, news,"simple");
            byte[][] rez_ = GiveBlocks(rez);
            byte[][] sh_ = GiveBlocks(sh);
            Console.WriteLine($"Тестовый пример: {plain}");
            Console.WriteLine($"Ключ: {key}");
            Console.WriteLine($"Результат:");
            for (int i = 0; i < rez_.Length; i++)
            {
                Console.WriteLine($"Блок C[{i}]: {BitConverter.ToString(rez_[i])}");
            }
            Console.WriteLine($"Расшифрованная фраза:");
            for (int i = 0; i < rez_.Length; i++)
            {
                Console.WriteLine($"Блок P[{i}]: {BitConverter.ToString(sh_[i])}");
            }
        }

        private static void A51Test()
        {
            A51 a = new();
            string text = "НЕ";
            string key = "ЛЕХА";
            byte[] t = Encoding.Unicode.GetBytes(text);
            byte[] k = Encoding.Unicode.GetBytes(key);
            var cypher = a.Encrypt(t, k);
        }

        private static void GammaReverseTextTest()
        {
            string plain = "92def06b3c130a59db54c704f8189d204a98fb2e67a8024c8912409b17b57e41";
            string key = "ffeeddccbbaa99887766554433221100f0f1f2f3f4f5f6f7f8f9fafbfcfdfeff";

            byte[] text = Split(plain, 2);
            byte[] keys = Split(key, 2);

            GammaReverseText a = new();
            string synhro = "1234567890abcdef234567890abcdef1";
            byte[] s = Split(synhro, 2);
            byte[] news = new byte[s.Length];
            Array.Copy(s, 0, news, 0, s.Length);
            byte[] rez = a.Encrypt(text, keys, s);
            byte[] sh = a.Decrypt(rez, keys, news);
            byte[][] rez_ = GiveBlocks(rez);
            byte[][] sh_ = GiveBlocks(sh);
            Console.WriteLine($"Тестовый пример: {plain}");
            Console.WriteLine($"Ключ: {key}");
            Console.WriteLine($"Результат:");
            for (int i = 0; i < rez_.Length; i++)
            {
                Console.WriteLine($"Блок C[{i}]: {BitConverter.ToString(rez_[i])}");
            }
            Console.WriteLine($"Расшифрованная фраза:");
            for (int i = 0; i < rez_.Length; i++)
            {
                Console.WriteLine($"Блок P[{i}]: {BitConverter.ToString(sh_[i])}");
            }
        }

        private static void GammaReverseTest()
        {
            Console.WriteLine("МАГМА: Режим гаммирования с обратной связью по выходу");
            string plain = "92def06b3c130a59db54c704f8189d204a98fb2e67a8024c8912409b17b57e41";
            string key = "ffeeddccbbaa99887766554433221100f0f1f2f3f4f5f6f7f8f9fafbfcfdfeff";

            byte[] text = Split(plain, 2);
            byte[] keys = Split(key, 2);

            GammaReverse a = new();
            string t = Encoding.Unicode.GetString(text);
            string k = Encoding.Unicode.GetString(keys);
            Config conf = new Config() { Key = k };
            string synhro = "1234567890abcdef234567890abcdef1";
            byte[] s = Split(synhro, 2);
            byte[] sa = new byte[s.Length];
            s.CopyTo(sa, 0);
            byte[] rez = a.Encrypt(text, keys, s);
            byte[] sh = a.Decrypt(rez, keys, sa);
            byte[][] rez_ = GiveBlocks(rez);
            byte[][] sh_ = GiveBlocks(sh);
            Console.WriteLine($"Тестовый пример: {plain}");
            Console.WriteLine($"Ключ: {key}");
            Console.WriteLine($"Результат:");
            for (int i = 0; i < rez_.Length; i++)
            {
                Console.WriteLine($"Блок C[{i}]: {BitConverter.ToString(rez_[i])}");
            }
            Console.WriteLine($"Расшифрованная фраза:");
            for (int i = 0; i < rez_.Length; i++)
            {
                Console.WriteLine($"Блок P[{i}]: {BitConverter.ToString(sh_[i])}");
            }
        }

        private static void AESTest()
        {
            string plain = "00112233445566778899aabbccddeeff";
            string key = "000102030405060708090a0b0c0d0e0f";

            byte[] text = Split(plain, 2);
            byte[] keys = Split(key, 2);

            AES a = new();
            byte[] rez = a.Encrypt(text, keys);
            byte[] sh = a.Decrypt(rez, keys);
            Console.WriteLine($"Тестовый пример: {plain}");
            Console.WriteLine($"Ключ: {key}");
            Console.WriteLine($"Результат: {BitConverter.ToString(rez)}");
            Console.WriteLine($"Расшифрованная фраза: {BitConverter.ToString(sh)}");
        }

        private static void GammaTest()
        {
            string plain = "92def06b3c130a59";
            string key = "ffeeddccbbaa99887766554433221100f0f1f2f3f4f5f6f7f8f9fafbfcfdfeff";

            byte[] text = Split(plain, 2);
            byte[] keys = Split(key, 2);

            MagmaClass a = new();
            string t = Encoding.Unicode.GetString(text);
            string k = Encoding.Unicode.GetString(keys);
            Config conf = new Config() { Key = k };
            string syn = "1234567800000000";
            byte[] s = Split(syn, 2);
            byte[] rez = a.Encrypt(text, keys,s,"gamma");
            byte[] sh = a.Decrypt(rez, keys,s,"gamma");
            Console.WriteLine($"Тестовый пример: {plain}");
            Console.WriteLine($"Ключ: {key}");
            Console.WriteLine($"Результат: {BitConverter.ToString(rez)}");
            Console.WriteLine($"Расшифрованная фраза: {BitConverter.ToString(sh)}");
        }

        private static void SboxTest()
        {
/*            string plain = "fedcba9876543210";
            string key = "ffeeddccbbaa99887766554433221100f0f1f2f3f4f5f6f7f8f9fafbfcfdfeff";

            byte[] text = Split(plain, 2);
            byte[] keys = Split(key, 2);

            MagmaClass a = new();
            byte[] rez = a.Encrypt(text, keys);
            byte[] sh = a.Decrypt(rez, keys);
            Console.WriteLine($"Тестовый пример: {plain}");
            Console.WriteLine($"Ключ: {key}");
            Console.WriteLine($"Результат: {BitConverter.ToString(rez)}");
            Console.WriteLine($"Расшифрованная фраза: {BitConverter.ToString(sh)}");*/
        }

        private static void KuznecTest()
        {
            string plain = "1122334455667700ffeeddccbbaa9988";
            string key = "8899aabbccddeeff0011223344556677fedcba98765432100123456789abcdef";

            byte[] text = Split(plain, 2);
            byte[] keys = Split(key, 2);

            KuznecTest a = new();
            byte[] rez = a.Encrypt(text, keys);
            byte[] des = a.Decrypt(rez, keys);
            Console.WriteLine($"Тестовый пример: {plain}");
            Console.WriteLine($"Ключ: {key}");
            Console.WriteLine($"Результат: {BitConverter.ToString(rez)}");
            Console.WriteLine($"Расшифровка: {BitConverter.ToString(des)}");
        }

        private static byte[] Split(string plaintext, int v)
        {
            string[] rez = new string[plaintext.Length / 2];
            int j = 0;
            for (int i = 0; i < plaintext.Length; i += 2)
                rez[j++] = plaintext.Substring(i, 2);

            return rez.Select(i => Convert.ToByte(i, 16)).ToArray();
        }
        private static byte[][] GiveBlocks(byte[] data)
        {
            int blockSize = 8;
            byte[][] res = new byte[data.Length / 8][];
            for (int i = 0; i < res.Length; i++)
            {
                res[i] = new byte[8];
                for (int j = 0; j < blockSize; j++)
                    res[i][j] = data[i * blockSize + j];
            }
            return res;
        }
    }
}

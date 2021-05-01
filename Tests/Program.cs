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
            GammaReverseTest();
            int a = 2, b = 6, p = 11;
            Elliptic ell = new Elliptic(a, b, p);
            ell.Calculate_Q();
            Point G = new Point(10, 6);
            Point rez = ell.GetValue(5, G);
            Console.WriteLine(rez);
        }

        private static void GammaReverseTextTest()
        {
            Console.WriteLine("МАГМА: Режим гаммирования с обратной связью по шифртексту");
            string plain = "92def06b3c130a59db54c704f8189d204a98fb2e67a8024c8912409b17b57e41";
            string key = "ffeeddccbbaa99887766554433221100f0f1f2f3f4f5f6f7f8f9fafbfcfdfeff";

            byte[] text = Split(plain, 2);
            byte[] keys = Split(key, 2);

            GammaReverseText a = new();
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
            for(int i = 0; i < rez_.Length; i++)
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

            GammaGost28147 a = new();
            string t = Encoding.Unicode.GetString(text);
            string k = Encoding.Unicode.GetString(keys);
            Config conf = new Config() { Key = k };
            string syn = "1234567800000000";
            byte[] s = Split(syn, 2);
            byte[] rez = a.Encrypt(text, keys,s);
            byte[] sh = a.Decrypt(rez, keys,s);
            Console.WriteLine($"Тестовый пример: {plain}");
            Console.WriteLine($"Ключ: {key}");
            Console.WriteLine($"Результат: {BitConverter.ToString(rez)}");
            Console.WriteLine($"Расшифрованная фраза: {BitConverter.ToString(sh)}");
        }

        private static void SboxTest()
        {
            string plain = "fedcba9876543210";
            string key = "ffeeddccbbaa99887766554433221100f0f1f2f3f4f5f6f7f8f9fafbfcfdfeff";

            byte[] text = Split(plain, 2);
            byte[] keys = Split(key, 2);

            MagmaClass a = new();
            byte[] rez = a.Encrypt(text, keys);
            byte[] sh = a.Decrypt(rez,keys);
            Console.WriteLine($"Тестовый пример: {plain}");
            Console.WriteLine($"Ключ: {key}");
            Console.WriteLine($"Результат: {BitConverter.ToString(rez)}");
            Console.WriteLine($"Расшифрованная фраза: {BitConverter.ToString(sh)}");
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

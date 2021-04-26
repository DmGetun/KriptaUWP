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
            A51Test();
        }

        private static void A51Test()
        {
            string plain = "hello";
            string key = "1223456789ABCDEF";
            int frameNumber = 0x_000134;

            byte[] text = Split(plain, 2);
            byte[] keys = Split(key, 2);

            A51 a = new();
            byte[] rez = a.Encrypt(text, keys);
            Console.WriteLine($"Тестовый пример: {plain}");
            Console.WriteLine($"Ключ: {key}");
            Console.WriteLine($"Результат: {BitConverter.ToString(rez)}");
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
    }
}

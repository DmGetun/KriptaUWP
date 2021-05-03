using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWP.Помошники;

namespace UWP.Алгоритмы
{
    abstract class Algorithm
    {
        public abstract string Name { get; }
        public abstract string DefaultKey { get; }
        public static string NonKey { get; } = "Данный шифр является бесключевым";
        public abstract string Group { get; }
        public abstract bool IsReplaceText { get; }
        public abstract string Encrypt(string plainText, Config config);
        public abstract string Decrypt(string cipherText, Config config);
        public abstract string CheckKey(string key);
        public abstract string KeyView();
        public abstract string GenerateKey();
    }
}

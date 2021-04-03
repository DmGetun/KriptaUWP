using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UWP.Алгоритмы;
using UWP.Помошники;

namespace Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            var text_ = "1122334455667700ffeeddccbbaa9988";
            string key = "8899aabbccddeeff0011223344556677fedcba98765432100123456789abcdef";
            var a = Regex.Matches(key, @"(\w{8})");
            var array = a.Select(i => i.Value).ToArray();
            uint[] keys_ = array.Select(i => Convert.ToUInt32(i, 16)).ToArray();
            var crypt = new AES();
            array = Regex.Matches(text_, @"(\w{8})").Select(i => i.Value).ToArray();
            var text = array.Select(i => Convert.ToUInt32(i, 16)).ToArray().Select(i => BitConverter.GetBytes(i)).ToArray();
            byte[] _text_ = new byte[16];
            int i = 0;
            foreach (byte[] arr in text)
            {
                foreach (byte b in arr)
                {
                    _text_[i++] = b;
                }
            }
            Config config = new Config();
            byte[] _keys_ = new byte[keys_.Length * 4];
            i = 0;
            foreach(uint num in keys_)
            {
                Array.Copy(BitConverter.GetBytes(num), 0, _keys_, i * 4, 4);
                i++;
            }
            config.Key = Encoding.Unicode.GetString(_keys_);
            var res = crypt.Encrypt(Encoding.Unicode.GetString(_text_), config);
            string result = "4ee901e5c2d8ca3d";
            var c = Regex.Matches(result, @"(\w{8})").Select(i => i.Value).ToArray();
            text = c.Select(i => Convert.ToUInt32(i, 16)).ToArray().Select(i => BitConverter.GetBytes(i)).ToArray();
            Console.WriteLine();
        }
    }
}

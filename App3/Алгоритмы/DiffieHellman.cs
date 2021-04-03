using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWP.Помошники;

namespace UWP.Алгоритмы
{
    class DiffieHellman : Algorithm
    {
        public override string Name => "Обмен ключами Диффи-Хеллман";

        public override string DefaultKey => "";

        public override bool IsReplaceText => throw new NotImplementedException();

        public override string CheckKey(string key)
        {
            throw new NotImplementedException();
        }

        public override string Decrypt(string cipherText, Config config)
        {
            throw new NotImplementedException();
        }

        public override string Encrypt(string plainText, Config config)
        {
            throw new NotImplementedException();
        }

        public override string GenerateKey()
        {
            throw new NotImplementedException();
        }

        public override string KeyView()
        {
            throw new NotImplementedException();
        }
    }
}

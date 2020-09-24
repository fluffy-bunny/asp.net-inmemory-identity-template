using System.Collections.Generic;

namespace oauth2.helpers.Default
{
    class NullEncryptor : ISymmetricEncryptor
    {
        public string DecryptString(string key, string cipherText)
        {
            return cipherText;
        }

        public string EncryptString(string key, string plainText)
        {
            return plainText;
        }

        public string GenerateKey()
        {
            return "null";
        }
    }
}

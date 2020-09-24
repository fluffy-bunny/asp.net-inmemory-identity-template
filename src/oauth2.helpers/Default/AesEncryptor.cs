using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace oauth2.helpers.Default
{
    class AesEncryptor : ISymmetricEncryptor
    {
        public string DecryptString(string key, string cipherText)
        {
            var parts = GetKeyParts(key);
          
            byte[] buffer = Convert.FromBase64String(cipherText);

            using (Aes aes = Aes.Create())
            {
                aes.Key = parts.key;
                aes.IV = parts.iv;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }

        public string EncryptString(string key, string plainText)
        {

            var parts = GetKeyParts(key);

            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = parts.key;
                aes.IV = parts.iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                        {
                            streamWriter.Write(plainText);
                        }

                        array = memoryStream.ToArray();
                    }
                }
            }

            return Convert.ToBase64String(array);
        }

        private (byte[] key, byte[] iv) GetKeyParts(string fullKey)
        {
            var parts = fullKey.Split('.');
            return (Convert.FromBase64String(parts[0]), Convert.FromBase64String(parts[1]));
        }
        public string GenerateKey()
        {
            SymmetricAlgorithm aes = new AesManaged();
            byte[] byteKey = aes.Key;
            byte[] iv = aes.IV;
            string sKey = $"{Convert.ToBase64String(byteKey)}.{Convert.ToBase64String(iv)}";
            return sKey;
        }
    }
}

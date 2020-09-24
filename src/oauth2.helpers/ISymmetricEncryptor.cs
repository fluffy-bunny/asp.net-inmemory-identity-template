namespace oauth2.helpers
{
    public interface ISymmetricEncryptor
    {
        string GenerateKey();
        string EncryptString(string key, string plainText);
        string DecryptString(string key, string cipherText);
    }
}

namespace IT2163_Assignment2_234695G.Utils
{
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;

    public static class EncryptionHelper
    {
        private static readonly string Key = "YourEncryptionKeyHere"; // Use a secure, long key (ideally stored securely)

        public static string Encrypt(string plainText)
        {
            using var aes = Aes.Create();
            var keyBytes = Encoding.UTF8.GetBytes(Key);
            aes.Key = keyBytes.Take(32).ToArray(); // Use the first 32 bytes for AES-256
            aes.IV = new byte[16]; // Use a zero IV (you can randomize this and store it with the cipher text for more security)

            using var encryptor = aes.CreateEncryptor();
            using var ms = new MemoryStream();
            using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
            using (var writer = new StreamWriter(cs))
            {
                writer.Write(plainText);
            }

            return Convert.ToBase64String(ms.ToArray());
        }

        public static string Decrypt(string cipherText)
        {
            using var aes = Aes.Create();
            var keyBytes = Encoding.UTF8.GetBytes(Key);
            aes.Key = keyBytes.Take(32).ToArray();
            aes.IV = new byte[16];

            using var decryptor = aes.CreateDecryptor();
            using var ms = new MemoryStream(Convert.FromBase64String(cipherText));
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var reader = new StreamReader(cs);

            return reader.ReadToEnd();
        }
    }

}

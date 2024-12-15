using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Banking_Application
{
    public static class EncryptionHelper
    {
        private static readonly string Key = Environment.GetEnvironmentVariable("ENCRYPTION_KEY") ?? "DefaultKey123456"; // Fallback for dev
        private static readonly string IV = Environment.GetEnvironmentVariable("ENCRYPTION_IV") ?? "DefaultIV123456";   // Fallback for dev

        /// <summary>
        /// Encrypts plaintext using AES encryption.
        /// </summary>
        /// <param name="plainText">The plaintext to encrypt.</param>
        /// <returns>Base64-encoded ciphertext.</returns>
        public static string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return plainText;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(Key);
                aes.IV = Encoding.UTF8.GetBytes(IV);
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                {
                    byte[] inputBytes = Encoding.UTF8.GetBytes(plainText);
                    byte[] encryptedBytes = encryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);
                    return Convert.ToBase64String(encryptedBytes);
                }
            }
        }

        /// <summary>
        /// Decrypts ciphertext using AES encryption.
        /// </summary>
        /// <param name="encryptedText">The Base64-encoded ciphertext to decrypt.</param>
        /// <returns>Decrypted plaintext.</returns>
        public static string Decrypt(string encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText))
                return encryptedText;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(Key);
                aes.IV = Encoding.UTF8.GetBytes(IV);
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                {
                    byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
                    byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
                    return Encoding.UTF8.GetString(decryptedBytes);
                }
            }
        }
    }
}

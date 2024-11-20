using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Banking_Application
{
    public static class EncryptionHelper
    {
        // AES encryption key and IV (for demo purposes, stored here; should be securely stored in production)
        private static readonly string Key = "YourStrongKey123"; // Use a strong 16/24/32 character key.
        private static readonly string IV = "YourIV1234567890";   // Use a 16-character initialization vector.

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

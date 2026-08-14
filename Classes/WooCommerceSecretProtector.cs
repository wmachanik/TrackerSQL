using System;
using System.Configuration;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace TrackerSQL.Classes
{
    /// <summary>
    /// AES encrypt/decrypt for WooCommerce API secrets stored in the database.
    /// Key from appSettings WooCommerceCryptoKey (Base64 32-byte key recommended).
    /// </summary>
    public static class WooCommerceSecretProtector
    {
        private const string ConfigKeyName = "WooCommerceCryptoKey";

        public static string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return null;

            byte[] key = GetKeyBytes();
            using (var aes = Aes.Create())
            {
                aes.Key = key;
                aes.GenerateIV();
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (var encryptor = aes.CreateEncryptor())
                using (var ms = new MemoryStream())
                {
                    ms.Write(aes.IV, 0, aes.IV.Length);
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (var sw = new StreamWriter(cs, Encoding.UTF8))
                    {
                        sw.Write(plainText);
                    }
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        public static string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
                return null;

            byte[] payload = Convert.FromBase64String(cipherText);
            if (payload.Length < 17)
                throw new InvalidOperationException("Encrypted secret payload is invalid.");

            byte[] key = GetKeyBytes();
            byte[] iv = new byte[16];
            Buffer.BlockCopy(payload, 0, iv, 0, 16);
            byte[] cipher = new byte[payload.Length - 16];
            Buffer.BlockCopy(payload, 16, cipher, 0, cipher.Length);

            using (var aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (var decryptor = aes.CreateDecryptor())
                using (var ms = new MemoryStream(cipher))
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var sr = new StreamReader(cs, Encoding.UTF8))
                {
                    return sr.ReadToEnd();
                }
            }
        }

        public static bool HasCryptoKeyConfigured()
        {
            try
            {
                GetKeyBytes();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static byte[] GetKeyBytes()
        {
            string configured = ConfigurationManager.AppSettings[ConfigKeyName];
            if (string.IsNullOrWhiteSpace(configured))
                throw new InvalidOperationException(
                    "WooCommerceCryptoKey is missing from appSettings. Add a Base64-encoded 32-byte key.");

            byte[] key;
            try
            {
                key = Convert.FromBase64String(configured.Trim());
            }
            catch (FormatException)
            {
                // Fallback: derive 32 bytes from the configured string (dev convenience only).
                using (var sha = SHA256.Create())
                {
                    key = sha.ComputeHash(Encoding.UTF8.GetBytes(configured.Trim()));
                }
                return key;
            }

            if (key.Length != 32)
                throw new InvalidOperationException(
                    "WooCommerceCryptoKey must decode to 32 bytes (AES-256), or be a passphrase (SHA-256 derived).");

            return key;
        }
    }
}

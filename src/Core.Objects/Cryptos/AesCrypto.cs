using Newtonsoft.Json;
using System;
using System.Text;

namespace Core.Objects.Cryptos
{
    public class AesCrypto<T> : ICrypto<T>
    {
        readonly string decryptionKey;
        readonly AesThenHmac crypto = new();

        public AesCrypto(string key = null) => decryptionKey = key;

        public string Encrypt(T source, string key)
        {
            Encoding e = Encoding.UTF8;
            byte[] rawData = e.GetBytes(JsonConvert.SerializeObject(source));
            byte[] cipherData = crypto.SimpleEncryptWithPassword(rawData, key);
            return Convert.ToBase64String(cipherData);
        }

        public string Encrypt(T source)
        {
            if (decryptionKey == null)
            {
                throw new InvalidOperationException("Decryption key not set.");
            }

            Encoding e = Encoding.UTF8;
            byte[] rawData = e.GetBytes(JsonConvert.SerializeObject(source));
            byte[] cipherData = crypto.SimpleEncryptWithPassword(rawData, decryptionKey);
            return Convert.ToBase64String(cipherData);
        }

        public T Decrypt(string source, string key)
        {
            Encoding e = Encoding.UTF8;
            byte[] decryptedBytes = crypto.SimpleDecryptWithPassword(Convert.FromBase64String(source), key);
            return Data.ParseJson<T>(e.GetString(decryptedBytes));
        }

        public T Decrypt(string source)
        {
            if (decryptionKey == null)
            {
                throw new InvalidOperationException("Decryption key not set.");
            }

            Encoding e = Encoding.UTF8;
            byte[] decryptedBytes = crypto.SimpleDecryptWithPassword(Convert.FromBase64String(source), decryptionKey);
            return Data.ParseJson<T>(e.GetString(decryptedBytes));
        }
    }
}
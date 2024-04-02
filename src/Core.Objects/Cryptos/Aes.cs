/*
 * This work (Modern Encryption of a String C#, by James Tuley), 
 * identified by James Tuley, is free of known copyright restrictions.
 * https://gist.github.com/4336842
 * http://creativecommons.org/publicdomain/mark/1.0/ 
 */

using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Core.Objects.Cryptos
{
    /// <summary>
    /// Use when signing data to encrypt the signature in a verifyable manner
    /// </summary>
    internal class AesThenHmac
    {
        private static readonly RandomNumberGenerator Random = RandomNumberGenerator.Create();

        //Preconfigured Encryption Parameters
        public int BlockBitSize { get; set; } = 128;
        public int KeyBitSize { get; set; } = 256;

        //Preconfigured Password Key Derivation Parameters
        public int SaltBitSize { get; set; } = 64;
        public int Iterations { get; set; } = 10000;
        public int MinPasswordLength { get; set; } = 12;

        public int KeyByteSize => KeyBitSize / 8;
        public int SaltByteSize => SaltBitSize / 8;

        /// <summary>
        /// Helper that generates a random key on each call.
        /// </summary>
        /// <returns></returns>
        public byte[] NewKey()
        {
            byte[] key = new byte[KeyBitSize / 8];
            Random.GetBytes(key);
            return key;
        }

        /// <summary>
        /// Simple Encryption (AES) then Authentication (HMAC) for a UTF8 Message.
        /// </summary>
        /// <param name="secretMessage">The secret message.</param>
        /// <param name="cryptKey">The crypt key.</param>
        /// <param name="authKey">The auth key.</param>
        /// <param name="nonSecretPayload">(Optional) Non-Secret Payload.</param>
        /// <returns>
        /// Encrypted Message
        /// </returns>
        /// <exception cref="ArgumentException">Secret Message Required!;secretMessage</exception>
        /// <remarks>
        /// Adds overhead of (Optional-Payload + BlockSize(16) + Message-Padded-To-Blocksize +  HMac-Tag(32)) * 1.33 Base64
        /// </remarks>
        public string SimpleEncrypt(string secretMessage, byte[] cryptKey, byte[] authKey, byte[] nonSecretPayload = null)
        {
            if (string.IsNullOrEmpty(secretMessage))
                throw new ArgumentException("Secret Message Required!", nameof(secretMessage));

            byte[] plainText = Encoding.UTF8.GetBytes(secretMessage);
            byte[] cipherText = SimpleEncrypt(plainText, cryptKey, authKey, nonSecretPayload);
            return Convert.ToBase64String(cipherText);
        }

        public byte[] SimpleEncrypt(byte[] secretMessage, byte[] cryptKey, byte[] authKey, byte[] nonSecretPayload = null)
        {
            //User Error Checks
            if (cryptKey == null || cryptKey.Length != KeyBitSize / 8)
                throw new ArgumentException(string.Format("Key needs to be {0} bit!", KeyBitSize), nameof(cryptKey));

            if (authKey == null || authKey.Length != KeyBitSize / 8)
                throw new ArgumentException(string.Format("Key needs to be {0} bit!", KeyBitSize), nameof(authKey));

            if (secretMessage == null || secretMessage.Length < 1)
                throw new ArgumentException("Secret Message Required!", nameof(secretMessage));


            //non-secret payload optional
            nonSecretPayload ??= Array.Empty<byte>();

            byte[] cipherText;
            byte[] iv;

            Aes aes = Aes.Create();

            aes.BlockSize = BlockBitSize;
            aes.KeySize = KeyBitSize;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using (aes)
            {
                //Use random IV
                aes.GenerateIV();
                iv = aes.IV;

                using ICryptoTransform encrypter = aes.CreateEncryptor(cryptKey, iv);
                using MemoryStream cipherStream = new();
                using (CryptoStream cryptoStream = new(cipherStream, encrypter, CryptoStreamMode.Write))
                using (BinaryWriter binaryWriter = new(cryptoStream))
                {
                    binaryWriter.Write(secretMessage); //Encrypt Data
                }

                cipherText = cipherStream.ToArray();
            }

            //Assemble encrypted message and add authentication
            using HMACSHA256 hmac = new(authKey);
            using MemoryStream encryptedStream = new();
            using (BinaryWriter binaryWriter = new(encryptedStream))
            {
                binaryWriter.Write(nonSecretPayload);  //Prepend non-secret payload if any
                binaryWriter.Write(iv);                //Prepend IV
                binaryWriter.Write(cipherText);        //Write Ciphertext
                binaryWriter.Flush();

                byte[] tag = hmac.ComputeHash(encryptedStream.ToArray()); //Authenticate all data
                binaryWriter.Write(tag);                               //Postpend tag
            }

            return encryptedStream.ToArray();
        }

        /// <summary>
        /// Simple Authentication (HMAC) then Decryption (AES) for a secrets UTF8 Message.
        /// </summary>
        /// <param name="encryptedMessage">The encrypted message.</param>
        /// <param name="cryptKey">The crypt key.</param>
        /// <param name="authKey">The auth key.</param>
        /// <param name="nonSecretPayloadLength">Length of the non secret payload.</param>
        /// <returns>
        /// Decrypted Message
        /// </returns>
        /// <exception cref="ArgumentException">Encrypted Message Required!;encryptedMessage</exception>
        public string SimpleDecrypt(string encryptedMessage, byte[] cryptKey, byte[] authKey, int nonSecretPayloadLength = 0)
        {
            if (string.IsNullOrWhiteSpace(encryptedMessage))
                throw new ArgumentException("Encrypted Message Required!", nameof(encryptedMessage));

            byte[] cipherText = Convert.FromBase64String(encryptedMessage);
            byte[] plainText = SimpleDecrypt(cipherText, cryptKey, authKey, nonSecretPayloadLength);
            return plainText == null ? null : Encoding.UTF8.GetString(plainText);
        }

        public byte[] SimpleDecrypt(byte[] encryptedMessage, byte[] cryptKey, byte[] authKey, int nonSecretPayloadLength = 0)
        {

            //Basic Usage Error Checks
            if (cryptKey == null || cryptKey.Length != KeyByteSize)
                throw new ArgumentException(string.Format("CryptKey needs to be {0} bit!", KeyBitSize), nameof(cryptKey));

            if (authKey == null || authKey.Length != KeyByteSize)
                throw new ArgumentException(string.Format("AuthKey needs to be {0} bit!", KeyBitSize), nameof(authKey));

            if (encryptedMessage == null || encryptedMessage.Length == 0)
                throw new ArgumentException("Encrypted Message Required!", nameof(encryptedMessage));

            using HMACSHA256 hmac = new(authKey);
            byte[] sentTag = new byte[hmac.HashSize / 8];

            //Calculate Tag
            byte[] calcTag = hmac.ComputeHash(encryptedMessage, 0, encryptedMessage.Length - sentTag.Length);
            int ivLength = BlockBitSize / 8;

            //if message length is to small just return null
            if (encryptedMessage.Length < sentTag.Length + nonSecretPayloadLength + ivLength)
                return Array.Empty<byte>();

            //Grab Sent Tag
            Array.Copy(encryptedMessage, encryptedMessage.Length - sentTag.Length, sentTag, 0, sentTag.Length);

            //Compare Tag with constant time comparison
            int compare = 0;
            for (int i = 0; i < sentTag.Length; i++)
                compare |= sentTag[i] ^ calcTag[i];


            //if message doesn't authenticate return null
            if (compare != 0)
                return Array.Empty<byte>();

            using Aes aes = Aes.Create();

            aes.BlockSize = BlockBitSize;
            aes.KeySize = KeyBitSize;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            //Grab IV from message
            byte[] iv = new byte[ivLength];
            Array.Copy(encryptedMessage, nonSecretPayloadLength, iv, 0, iv.Length);

            using ICryptoTransform decrypter = aes.CreateDecryptor(cryptKey, iv);
            using MemoryStream plainTextStream = new();
            using (CryptoStream decrypterStream = new(plainTextStream, decrypter, CryptoStreamMode.Write))
            using (BinaryWriter binaryWriter = new(decrypterStream))
            {
                //Decrypt Cipher Text from Message
                binaryWriter.Write(
                  encryptedMessage,
                  nonSecretPayloadLength + iv.Length,
                  encryptedMessage.Length - nonSecretPayloadLength - iv.Length - sentTag.Length
                );
            }

            //Return Plain Text
            return plainTextStream.ToArray();
        }

        /// <summary>
        /// Simple Encryption (AES) then Authentication (HMAC) of a UTF8 message
        /// using Keys derived from a Password (PBKDF2).
        /// </summary>
        /// <param name="secretMessage">The secret message.</param>
        /// <param name="password">The password.</param>
        /// <param name="nonSecretPayload">The non secret payload.</param>
        /// <returns>
        /// Encrypted Message
        /// </returns>
        /// <exception cref="ArgumentException">password</exception>
        /// <remarks>
        /// Significantly less secure than using random binary keys.
        /// Adds additional non secret payload for key generation parameters.
        /// </remarks>
        public string SimpleEncryptWithPassword(string secretMessage, string password, byte[] nonSecretPayload = null)
        {
            if (string.IsNullOrEmpty(secretMessage))
                throw new ArgumentException("Secret Message Required!", nameof(secretMessage));

            byte[] plainText = Encoding.UTF8.GetBytes(secretMessage);
            byte[] cipherText = SimpleEncryptWithPassword(plainText, password, nonSecretPayload);
            return Convert.ToBase64String(cipherText);
        }

        public byte[] SimpleEncryptWithPassword(byte[] secretMessage, string password, byte[] nonSecretPayload = null)
        {
            nonSecretPayload ??= Array.Empty<byte>();

            //User Error Checks
            if (string.IsNullOrWhiteSpace(password) || password.Length < MinPasswordLength)
                throw new ArgumentException(string.Format("Must have a password of at least {0} characters!", MinPasswordLength), nameof(password));

            if (secretMessage == null || secretMessage.Length == 0)
                throw new ArgumentException("Secret Message Required!", nameof(secretMessage));

            byte[] payload = new byte[SaltBitSize / 8 * 2 + nonSecretPayload.Length];

            Array.Copy(nonSecretPayload, payload, nonSecretPayload.Length);
            int payloadIndex = nonSecretPayload.Length;

            byte[] cryptKey;
            byte[] authKey;

            //Use Random Salt to prevent pre-generated weak password attacks.
            using (Rfc2898DeriveBytes generator = new(password, SaltBitSize / 8, Iterations, HashAlgorithmName.SHA1))
            {
                byte[] salt = generator.Salt;
                cryptKey = generator.GetBytes(KeyBitSize / 8);            //Generate Keys
                Array.Copy(salt, 0, payload, payloadIndex, salt.Length);  //Create Non Secret Payload
                payloadIndex += salt.Length;
            }

            //Deriving separate key, might be less efficient than using HKDF, 
            //but now compatible with RNEncryptor which had a very similar wireformat and requires less code than HKDF.
            using (Rfc2898DeriveBytes generator = new(password, SaltBitSize / 8, Iterations, HashAlgorithmName.SHA1))
            {
                byte[] salt = generator.Salt;
                authKey = generator.GetBytes(KeyBitSize / 8);             //Generate Keys
                Array.Copy(salt, 0, payload, payloadIndex, salt.Length);  //Create Rest of Non Secret Payload
            }

            return SimpleEncrypt(secretMessage, cryptKey, authKey, payload);
        }

        /// <summary>
        /// Simple Authentication (HMAC) and then Descryption (AES) of a UTF8 Message
        /// using keys derived from a password (PBKDF2). 
        /// </summary>
        /// <param name="encryptedMessage">The encrypted message.</param>
        /// <param name="password">The password.</param>
        /// <param name="nonSecretPayloadLength">Length of the non secret payload.</param>
        /// <returns>
        /// Decrypted Message
        /// </returns>
        /// <exception cref="ArgumentException">Encrypted Message Required!;encryptedMessage</exception>
        /// <remarks>
        /// Significantly less secure than using random binary keys.
        /// </remarks>
        public string SimpleDecryptWithPassword(string encryptedMessage, string password, int nonSecretPayloadLength = 0)
        {
            if (string.IsNullOrWhiteSpace(encryptedMessage))
                throw new ArgumentException("Encrypted Message Required!", nameof(encryptedMessage));

            byte[] cipherText = Convert.FromBase64String(encryptedMessage);
            byte[] plainText = SimpleDecryptWithPassword(cipherText, password, nonSecretPayloadLength);
            return plainText == null ? null : Encoding.UTF8.GetString(plainText);
        }

        public byte[] SimpleDecryptWithPassword(byte[] encryptedMessage, string password, int nonSecretPayloadLength = 0)
        {
            //User Error Checks
            if (string.IsNullOrWhiteSpace(password) || password.Length < MinPasswordLength)
                throw new ArgumentException(string.Format("Must have a password of at least {0} characters!", MinPasswordLength), nameof(password));

            if (encryptedMessage == null || encryptedMessage.Length == 0)
                throw new ArgumentException("Encrypted Message Required!", nameof(encryptedMessage));

            //Grab Salt from Non-Secret Payload
            byte[] cryptSalt = encryptedMessage.Skip(nonSecretPayloadLength).Take(SaltByteSize).ToArray();
            byte[] authSalt = encryptedMessage.Skip(nonSecretPayloadLength + cryptSalt.Length).Take(SaltByteSize).ToArray();

            // generate keys
            using Rfc2898DeriveBytes cryptGen = new(password, cryptSalt, Iterations, HashAlgorithmName.SHA1);
            using Rfc2898DeriveBytes authGen = new(password, authSalt, Iterations, HashAlgorithmName.SHA1);

            byte[] cryptKey = cryptGen.GetBytes(KeyByteSize);
            byte[] authKey = authGen.GetBytes(KeyByteSize);

            return SimpleDecrypt(encryptedMessage, cryptKey, authKey, cryptSalt.Length + authSalt.Length + nonSecretPayloadLength);
        }
    }
}
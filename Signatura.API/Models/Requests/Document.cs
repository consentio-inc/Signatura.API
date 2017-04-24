using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Utilities.Encoders;
using Signatura.API.Crypto;
using Signatura.API.Models;

namespace Signatura.API.Models.Requests
{
    public class Document
    {
        private byte[] _content;

        private PrivateKey _ownerKey;

        private byte[] _documentKey;

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("hash")]
        public string Hash { get; set; }

        [JsonProperty("raw_hash")]
        public string RawHash { get; set; }

        [JsonProperty("content")]
        public byte[] Content
        {
            get { return _content; }
            set {
                _content = Encrypt(value);

                var hasher = SHA256.Create();

                var sha256 = hasher.ComputeHash(value);
                RawHash = Hex.ToHexString(sha256);

                var sha256d = hasher.ComputeHash(sha256);
                Hash = Hex.ToHexString(sha256d);
            }
        }

        [JsonProperty("mime_type")]
        public string MimeType { get; set; }

        [JsonProperty("filename")]
        public string FileName { get; set; }

        [JsonProperty("participants")]
        public List<Participant> Participants { get; }

        public Document(PrivateKey privateKey)
        {
            _ownerKey = privateKey;
            Participants = new List<Participant>();
        }

        private byte[] Encrypt(byte[] plainText)
        {
            _documentKey = new byte[32];
            var iv = new byte[16];

            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(_documentKey);
                rng.GetBytes(iv);
            }

            using (var aes = new RijndaelManaged()
            {
                KeySize = 256,
                BlockSize = 128,
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7
            })
            {
                var encryptor = aes.CreateEncryptor(_documentKey, iv);

                using (var msEncrypt = new MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        csEncrypt.Write(plainText, 0, plainText.Length);
                        csEncrypt.FlushFinalBlock();

                        var cipherText = msEncrypt.ToArray();

                        byte[] mac;
                        using (var hmac = new HMACSHA256(_documentKey))
                        {
                            mac = hmac.ComputeHash(cipherText);
                        }

                        var buf = new byte[iv.Length + cipherText.Length + mac.Length];
                        Array.Copy(iv, buf, iv.Length);
                        Array.Copy(cipherText, 0, buf, iv.Length, cipherText.Length);
                        Array.Copy(mac, 0, buf, iv.Length + cipherText.Length, mac.Length);

                        return buf;
                    }
                }
            }
        }

        public void AddParticipant(Participant participant)
        {
            if (participant.PublicKey == null)
                throw new ArgumentNullException(nameof(participant.PublicKey));

            var share = new ECIES(
                privateKey: _ownerKey,
                publicKey: participant.PublicKey
            ).Encrypt(_documentKey);

            participant.Key = AsHexString(share);

            Participants.Add(participant);
        }

        private static string AsHexString(byte[] buf)
        {
            var sb = new StringBuilder();
            foreach (var b in buf)
                sb.Append(b.ToString("x2"));

            return sb.ToString();
        }

    }

    public class Participant
    {

        [JsonProperty("id")]
        public string ParticipantId { get; set; }

        [JsonIgnore]
        public PublicKey PublicKey { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }
    }

    public class Signer : Participant
    {
        [JsonProperty("type")]
        public readonly string Type = "signer";

        [JsonIgnore]
        public string SignerId
        {
            get { return ParticipantId; }
            set { ParticipantId = value;  }
        }

    }
}

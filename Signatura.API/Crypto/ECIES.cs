using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using NBitcoin;
using NBitcoin.BouncyCastle.Math;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Encoders;
using Signatura.API.Models;

namespace Signatura.API.Crypto
{
    public class ECIES
    {
        private readonly PrivateKey _privateKey;

        private readonly PublicKey _publicKey;

        private byte[] _kE;

        private byte[] _kM;

        private static readonly X9ECParameters Curve = Org.BouncyCastle.Crypto.EC.CustomNamedCurves.GetByName("secp256k1");
        private static readonly ECDomainParameters Domain = new ECDomainParameters(Curve.Curve, Curve.G, Curve.N, Curve.H, Curve.GetSeed());

        public ECIES(PrivateKey privateKey, PublicKey publicKey)
        {
            _privateKey = privateKey;
            _publicKey = publicKey;
        }

        public void ComputeSharedKey()
        {
            var prvKey = _privateKey.ToBytes();
            var pubKey = _publicKey.ToBytes();

            var r = new Org.BouncyCastle.Math.BigInteger(1, prvKey);

            var KB = Domain.G.Multiply(r);
            var P = KB.Normalize().Multiply(r);
            var S = P.Normalize().AffineXCoord;
            var Sbuf = S.GetEncoded();

            var kEkM = SHA512.Create().ComputeHash(Sbuf);

            var publicParams = new ECPublicKeyParameters(P, Domain);
            var pubkey = publicParams.Q.GetEncoded();

            _kE = new byte[32];
            Array.Copy(kEkM, _kE, 32);

            _kM = new byte[32];
            Array.Copy(kEkM, 32, _kM, 0, 32);
        }

        public byte[] Encrypt(byte[] message)
        {
            var iv = new byte[16];
            using (var hmac = new HMACSHA256(_privateKey.ToBytes()))
            {
                var buf = hmac.ComputeHash(message);
                Array.Copy(buf, iv, iv.Length);
            }

            ComputeSharedKey();

            var c = EncryptCipherText(message, _kE, iv);

            var d = new byte[32];
            using (var hmac = new HMACSHA256(_kM))
            {
                var buf = hmac.ComputeHash(c);
                Array.Copy(buf, d, d.Length);
            }

            var rbuf = _privateKey.PublicKey.ToBytes();

            var encbuf = new byte[rbuf.Length + c.Length + d.Length];
            Array.Copy(rbuf, encbuf, rbuf.Length);
            Array.Copy(c, 0, encbuf, rbuf.Length, c.Length);
            Array.Copy(d, 0, encbuf, rbuf.Length + c.Length, d.Length);

            return encbuf;
        }

        private byte[] EncryptCipherText(byte[] message, byte[] key, byte[] iv)
        {
            using (var aes = new RijndaelManaged()
            {
                KeySize = 256,
                BlockSize = 128,
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7
            })
            {
                var encryptor = aes.CreateEncryptor(key, iv);

                using (var msEncrypt = new MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        csEncrypt.Write(message, 0, message.Length);
                        csEncrypt.FlushFinalBlock();

                        var cipherText = msEncrypt.ToArray();
                        var buf = new byte[iv.Length + cipherText.Length];
                        Array.Copy(iv, buf, iv.Length);
                        Array.Copy(cipherText, 0, buf, iv.Length, cipherText.Length);

                        return buf;
                    }
                }
            }
        }

        // TODO: return byte array
        private string DecryptCypherText(byte[] message, byte[] key)
        {
            using (var aes = new RijndaelManaged()
            {
                KeySize = 256,
                BlockSize = 128,
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7
            })
            {
                var iv = new byte[16];
                Array.Copy(message, iv, iv.Length);
                var cipherText = new byte[message.Length - iv.Length];
                Array.Copy(message, iv.Length, cipherText, 0, cipherText.Length);

                aes.Key = key;
                aes.IV = iv;

                var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (var msDecrypt = new MemoryStream(cipherText))
                {
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (var srDecrypt = new StreamReader(csDecrypt))
                        {

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }

        private byte[] HMACSHA256(byte[] message, byte[] key)
        {
            using (var hmac = new HMACSHA256(key))
            {
                return hmac.ComputeHash(message);
            }
        }

        public byte[] Decrypt(byte[] encbuf)
        {
            var tagLength = 32;
            var offset = 33;

            var c = new byte[encbuf.Length - tagLength - offset];
            Array.Copy(encbuf, offset, c, 0, encbuf.Length - tagLength);

            var d = new byte[tagLength];
            Array.Copy(encbuf, offset + encbuf.Length - tagLength, d, 0, tagLength);

            ComputeSharedKey();

            var d2 = HMACSHA256(c, _kM);

            var equal = Org.BouncyCastle.Utilities.Arrays.ConstantTimeAreEqual(d, d2);
            if (!equal)
                throw new ArgumentException("invalid checksum");

            DecryptCypherText(c, _kE);

            return null;
        }
    }
}

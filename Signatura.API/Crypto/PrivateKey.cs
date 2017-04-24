using System;
using System.Collections.ObjectModel;
using System.Text;
using NBitcoin;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;
using Signatura.API.Crypto;

namespace Signatura.API.Models
{

    public class PrivateKey
    {
        private readonly Key _key;

        public PublicKey PublicKey
        {
            get { return new PublicKey(_key.PubKey.ToBytes()); }
        }

        public PrivateKey(Key key)
        {
            _key = key;
        }

        public static PrivateKey FromWIF(string wif)
        {
            var secret = new BitcoinSecret(wif);
            return new PrivateKey(secret.PrivateKey);
        }

        public byte[] ToBytes()
        {
            return _key.ToBytes();
        }

        public byte[] Sign(byte[] data)
        {
            uint256 hash = new uint256(data);
            return _key.SignCompact(hash);
        }
    }
}

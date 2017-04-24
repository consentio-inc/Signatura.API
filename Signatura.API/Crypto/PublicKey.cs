using NBitcoin;
using Org.BouncyCastle.Math;

namespace Signatura.API.Crypto
{
    public class PublicKey
    {
        private PubKey _pubKey;

        public PublicKey(byte[] bytes)
        {
            _pubKey = new PubKey(bytes);
        }

        public byte[] ToBytes()
        {
            return _pubKey.ToBytes();
        }
    }
}

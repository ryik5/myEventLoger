using System;
using System.Security;
using System.Security.Cryptography;
using System.Globalization;

namespace myEventLoger
{
    public sealed class KuSigner
    {
        private const int INTRICACY = 256;
        private const int HASH_LENGTH = 16;
        public const string PUBLIC_TEMPLATE = "<KuKeyValue><SignatureLength>{0}</SignatureLength><Public>{1}</Public></KuKeyValue>";
        public const string PRIVATE_TEMPLATE = "<KuKeyValue><SignatureLength>{0}</SignatureLength><Public>{1}</Public><Private>{2}</Private></KuKeyValue>";

        private int _keyLength;
        private int _signatureLength;
        
        private byte[] _publicKey;
        private byte[] _privateKey;

        MD4 _md4 = new MD4();

        public bool HasPrivateKey
        {
            get
            {
                return (null != _privateKey);
            }
        }

        public byte[] PublicKey
        {
            get
            {
                return _publicKey;
            }
        }

        public byte[] PrivateKey
        {
            get
            {
                return _privateKey;
            }
        }

        public KuSigner(string xmlString)
        {
            if (string.IsNullOrEmpty(xmlString))
                throw new ArgumentNullException("xmlString");

            SecurityElement topElement = SecurityElement.FromString(xmlString);

            string publicString = topElement.SearchForTextOfTag("Public");

            if (string.IsNullOrEmpty(publicString))
                throw new ArgumentException("Input string does not contain a valid encoding of the 'PublicKey' parameter.");

            _publicKey = Convert.FromBase64String(publicString);

            string signatureLengthString = topElement.SearchForTextOfTag("SignatureLength");

            if (string.IsNullOrEmpty(signatureLengthString))
                throw new ArgumentException("Input string does not contain a valid encoding of the 'SignatureLength' parameter.");

            _signatureLength = int.Parse(signatureLengthString, NumberStyles.Integer, CultureInfo.InvariantCulture);
            _keyLength = _publicKey.Length / _signatureLength;

            if (_keyLength <= 0)
                throw new ArgumentOutOfRangeException("keyLength");

            if (_signatureLength <= 0 || _signatureLength > HASH_LENGTH)
                throw new ArgumentOutOfRangeException("signatureLength");

            string privateString = topElement.SearchForTextOfTag("Private");

            if (string.IsNullOrEmpty(privateString))
                return;

            _privateKey = Convert.FromBase64String(privateString);

            if (_publicKey.Length != _privateKey.Length)
                throw new ArgumentException("publicKey.Length != privateKey.Length");

            if (0 == _publicKey.Length)
                throw new ArgumentException("0 == publicKey.Length");

            if (_publicKey.Length < _signatureLength)
                throw new ArgumentException("publicKey.Length < signatureLength");

            if (_publicKey.Length % _signatureLength != 0)
                throw new ArgumentException("publicKey.Length % signatureLength != 0");
        }

        public KuSigner(int keyLength, int signatureLength)
        {
            if (keyLength <= 0)
                throw new ArgumentOutOfRangeException("keyLength");

            if (signatureLength <= 0 || signatureLength > HASH_LENGTH)
                throw new ArgumentOutOfRangeException("signatureLength");

            _keyLength = keyLength;
            _signatureLength = signatureLength;
        }

        public KuSigner(byte[] publicKey, byte[] privateKey, int signatureLength)
        {
            if (null == publicKey)
                throw new ArgumentNullException("publicKey");

            if (null == privateKey)
                throw new ArgumentNullException("privateKey");

            if (signatureLength <= 0 || signatureLength > HASH_LENGTH)
                throw new ArgumentOutOfRangeException("signatureLength");

            if (publicKey.Length != privateKey.Length)
                throw new ArgumentException("publicKey.Length != privateKey.Length");

            if (0 == publicKey.Length)
                throw new ArgumentException("0 == publicKey.Length");

            if (publicKey.Length < signatureLength)
                throw new ArgumentException("publicKey.Length < signatureLength");

            if (publicKey.Length % signatureLength != 0)
                throw new ArgumentException("publicKey.Length % signatureLength != 0");

            _publicKey = publicKey;
            _privateKey = privateKey;

            _signatureLength = signatureLength;
            _keyLength = publicKey.Length / signatureLength;
        }

        public KuSigner(byte[] publicKey, int signatureLength)
        {
            if (null == publicKey)
                throw new ArgumentNullException("publicKey");

            if (signatureLength <= 0 || signatureLength > HASH_LENGTH)
                throw new ArgumentOutOfRangeException("signatureLength");

            if (0 == publicKey.Length)
                throw new ArgumentException("0 == publicKey.Length");

            if (publicKey.Length < signatureLength)
                throw new ArgumentException("publicKey.Length < signatureLength");

            if (publicKey.Length % signatureLength != 0)
                throw new ArgumentException("publicKey.Length % signatureLength != 0");

            _publicKey = publicKey;

            _signatureLength = signatureLength;
            _keyLength = publicKey.Length / signatureLength;
        }

        public void GenerateKeys()
        {
            _publicKey = new byte[_keyLength * _signatureLength];
            _privateKey = new byte[_keyLength * _signatureLength];

            // Генерация приватного ключа (заполняем массив случайными числами)
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            rng.GetBytes(_privateKey);

            // Генерация публичного ключа (долго...)
            for (int i = 0; i < _keyLength; i++)
            {
                byte[] toHash = new byte[_signatureLength];
                Array.Copy(_privateKey, i * _signatureLength, toHash, 0, _signatureLength);

                byte[] hash = computeHash(toHash);
                Array.Copy(hash, 0, _publicKey, i * _signatureLength, _signatureLength);
            }
        }

        public byte[] Sign(byte[] value)
        {
            if (null == value)
                throw new ArgumentNullException("value");

            if (value.Length <= 0)
                throw new ArgumentException("value.Length <= 0");

            if (null == _privateKey)
                throw new InvalidOperationException("null == _privateKey");

            byte[] hash = computeHash(value);

            uint pos = BitConverter.ToUInt32(hash, hash.Length - _signatureLength);

            pos = pos%((uint) _keyLength);

            byte[] signature = new byte[_signatureLength];

            for (int i = 0; i < _signatureLength; i++)
            {
                signature[i] = _privateKey[pos*_signatureLength+i];
            }

            return signature;
        }

        public bool VerifySignature(byte[] value, byte[] signature)
        {
            if (null == value)
                throw new ArgumentNullException("value");

            if (value.Length <= 0)
                throw new ArgumentException("value.Length <= 0");

            if (null == signature)
                throw new ArgumentNullException("signature");

            if (signature.Length <= 0)
                throw new ArgumentException("signature.Length <= 0");

            if (null == _publicKey)
                throw new InvalidOperationException("null == _publicKey");

            byte[] hash = computeHash(value);

            uint pos = BitConverter.ToUInt32(hash, hash.Length - _signatureLength);

            pos = pos % ((uint)_keyLength);

            byte[] etalone = new byte[_signatureLength];

            for (int i = 0; i < _signatureLength; i++)
            {
                etalone[i] = _publicKey[pos * _signatureLength + i];
            }

            byte[] signatureHash = computeHash(signature);

            for (int i = 0; i < _signatureLength; i++)
            {
                if (etalone[i] != signatureHash[i])
                    return false;
            }

            return true;
        }

        public string ToXmlString(bool includePrivateParameters)
        {
            if (null == _publicKey)
                throw new InvalidOperationException("null == _publicKey");

            if (null != _privateKey && includePrivateParameters)
                return
                    string.Format(CultureInfo.InvariantCulture, PRIVATE_TEMPLATE, _signatureLength,
                                  Convert.ToBase64String(_publicKey),
                                  Convert.ToBase64String(_privateKey));

            return
                string.Format(CultureInfo.InvariantCulture, PUBLIC_TEMPLATE, _signatureLength,
                              Convert.ToBase64String(_publicKey));
        }

        private byte[] computeHash(byte[] value)
        {
            for (int i = 0; i < INTRICACY; i++)
            {
                _md4.Initialize();
                value = _md4.ComputeHash(value);
            }

            return value;
        }
    }
}
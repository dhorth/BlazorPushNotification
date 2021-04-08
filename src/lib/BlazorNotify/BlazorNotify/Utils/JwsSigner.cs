using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Irc.Infrastructure.Helpers;
using Newtonsoft.Json;

namespace BlazorNotify.Utils
{
    public class JwsSigner
    {
        private readonly ECPrivateKeyParameters _privateKey;

        public JwsSigner(ECPrivateKeyParameters privateKey)
        {
            _privateKey = privateKey;
        }

        /// <summary>
        ///     Generates a Jws Signature.
        /// </summary>
        /// <param name="header"></param>
        /// <param name="payload"></param>
        /// <returns></returns>
        public string GenerateSignature(Dictionary<string, object> header, Dictionary<string, object> payload)
        {
            var securedInput = SecureInput(header, payload);
            var message = Encoding.UTF8.GetBytes(securedInput);

            var hashedMessage = Sha256Hash(message);

            var signer = new ECDsaSigner();
            signer.Init(true, _privateKey);
            var results = signer.GenerateSignature(hashedMessage);

            // Concated to create signature
            var a = results[0].ToByteArrayUnsigned();
            var b = results[1].ToByteArrayUnsigned();

            // a,b are required to be exactly the same length of bytes
            if (a.Length != b.Length)
            {
                var largestLength = Math.Max(a.Length, b.Length);
                a = ByteArrayPadLeft(a, largestLength);
                b = ByteArrayPadLeft(b, largestLength);
            }

            var signature = (a.Concat(b).ToArray()).FromBase64();
            return $"{securedInput}.{signature}";
        }

        private static string SecureInput(Dictionary<string, object> header, Dictionary<string, object> payload)
        {
            var encodeHeader =  (Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(header))).FromBase64();
            var encodePayload = (Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))).FromBase64();

            return $"{encodeHeader}.{encodePayload}";
        }

        private static byte[] ByteArrayPadLeft(byte[] src, int size)
        {
            var dst = new byte[size];
            var startAt = dst.Length - src.Length;
            Array.Copy(src, 0, dst, startAt, src.Length);
            return dst;
        }

        private static byte[] Sha256Hash(byte[] message)
        {
            var sha256Digest = new Sha256Digest();
            sha256Digest.BlockUpdate(message, 0, message.Length);
            var hash = new byte[sha256Digest.GetDigestSize()];
            sha256Digest.DoFinal(hash, 0);
            return hash;
        }
    }
}

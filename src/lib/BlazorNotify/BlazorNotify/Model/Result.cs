using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorNotify.Model
{
    public class Result
    {
        public byte[] PublicKey { get; set; }
        public byte[] Payload { get; set; }
        public byte[] Salt { get; set; }

        public string Base64EncodePublicKey()
        {
            return Convert.ToBase64String(PublicKey);
        }

        public string Base64EncodeSalt()
        {
            return Convert.ToBase64String(Salt);
        }
    }
}

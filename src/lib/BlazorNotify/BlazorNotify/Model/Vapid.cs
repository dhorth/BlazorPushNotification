using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorNotify.Model
{
    public class VapidDetails
    {
        public VapidDetails()
        {
        }

        public VapidDetails(string subject, string publicKey, string privateKey)
        {
            Subject = subject;
            PublicKey = publicKey;
            PrivateKey = privateKey;
        }

        public string Subject { get; set; }
        public string PublicKey { get; set; }
        public string PrivateKey { get; set; }

        public long Expiration { get; set; } = -1;
    }
}

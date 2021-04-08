using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Irc.Infrastructure.Helpers
{
    public static class Base64Helper
    {
        public static byte[] ToBase64(this string base64)
        {
            base64 = base64.Replace('-', '+').Replace('_', '/');

            while (base64.Length % 4 != 0)
            {
                base64 += "=";
            }

            return Convert.FromBase64String(base64);
        }

        public static string FromBase64(this byte[] data)
        {
            return Convert.ToBase64String(data).Replace('+', '-').Replace('/', '_').TrimEnd('=');
        }
    }
}

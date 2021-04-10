using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorNotify.Exceptions
{
    public class BlazorNotifyBaseException : Exception
    {
        public BlazorNotifyBaseException(string msg) : base(msg)
        {

        }
    }

    public class BlazorNotifyConfigurationException: BlazorNotifyBaseException
    {
        public BlazorNotifyConfigurationException(string setting) :base(setting)
        {
            Log.Logger.Error($" {setting}");
        }
    }
}

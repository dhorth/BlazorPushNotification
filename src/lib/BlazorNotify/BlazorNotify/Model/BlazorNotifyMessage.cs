using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorNotify.Model
{
    public class BlazorNotifyMessage
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public string Url { get; set; }
        public string Icon { get; set; }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}

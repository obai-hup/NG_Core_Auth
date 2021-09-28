using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NG_Core_Auth.Helpers
{
    public class AppSetting
    {
        public string Site { get; set; }
        public string Audience { get; set; }
        public string ExpaireTime { get; set; }
        public string Secret { get; set; }
    }
}

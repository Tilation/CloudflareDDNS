using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudflareDDNS
{
    internal class Settings
    {
        public bool Ready { get; set; } = false;
        public string DomainName { get; set; } = "your.domain.com";
        public string NewIp { get; set; } = "your.public.ip";
        public string CloudflareEmail { get; set; } = "auth@email";
        public string CloudflareGlobalAPIKey { get; set; } = "auth-key";
    }
}

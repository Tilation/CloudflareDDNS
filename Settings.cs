using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudflareDDNS
{
    internal class Settings
    {
        public string DomainName { get; set; } = "your.domain.com";
        public bool Proxied { get; set; } = false;
        public string Type { get; set; } = "A";
        public string Comment { get; set; } = "a domain";
        public int TTL { get; set; } = 3600;
        public string NewIp { get; set; } = "your.public.ip";
        public string Identifier { get; set; } = "023e105f4ecef8ad9ca31a8372d0c353";
        public string ZoneIdentifier { get; set; } = "023e105f4ecef8ad9ca31a8372d0c353";

        public bool AutoGetPublicIp { get; set; } = true; 
    }
}

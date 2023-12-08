using CloudFlare.Client;
using CloudFlare.Client.Api.Result;
using CloudFlare.Client.Contexts;
using Newtonsoft.Json.Linq;
using Serilog;
using System.Net;

namespace CloudflareDDNS
{
    internal class CloudflareDDNSState
    {
        Timer publicIpTimer;
        HttpClient ipHttpClient;
        Settings settings;

        CloudFlareClient cloudflare;

        public CloudflareDDNSState(Settings settings)
        {
            this.settings = settings;

            cloudflare = new CloudFlareClient(settings.CloudflareEmail, settings.CloudflareGlobalAPIKey, new ConnectionInfo());

            ipHttpClient = new HttpClient();
            publicIpTimer = new Timer(getip, null, 1 * 1000, 10 * 1000);
        }



        async void UpdateIp(string newIp)
        {
            if (settings.NewIp == newIp)
            {
                return;
            }

            Log.Information("Ip changing from {0} to {1}", settings.NewIp, newIp);

            var zonesResult = await cloudflare.Zones.GetAsync();

            if (zonesResult.Errors.Count > 0)
            {
                PrintErrors(zonesResult);
                return;
            }

            var zones = zonesResult.Result;

            foreach (var zone in zones)
            {
                var dnsListResult = await cloudflare.Zones.DnsRecords.GetAsync(zone.Id);

                if (dnsListResult.Errors.Count > 0)
                {
                    PrintErrors(dnsListResult);
                    return;
                }

                var dnsList = dnsListResult.Result;

                foreach (var dns in dnsList)
                {
                    if (dns.Name == settings.DomainName)
                    {
                        var dnsChangeResult = await cloudflare.Zones.DnsRecords.UpdateAsync(zone.Id, dns.Id, new CloudFlare.Client.Api.Zones.DnsRecord.ModifiedDnsRecord
                        {
                            Content = newIp,
                            Name = dns.Name,
                            Priority = dns.Priority,
                            Proxied = dns.Proxied,
                            Ttl = dns.Ttl,
                            Type = dns.Type
                        });

                        if (dnsChangeResult.Errors.Count > 0)
                        {
                            PrintErrors(dnsChangeResult);
                            return;
                        }

                        settings.NewIp = dnsChangeResult.Result.Content;
                    }
                }
            }
            Log.Information("Ip changed from {0} to {1}", settings.NewIp, newIp);
        }

        void PrintErrors<T>(CloudFlareResult<T> result)
        {
            if (result.Errors.Count == 0) return;
            foreach(var error in result.Errors)
            {
                PrintErrors(error);
            }
        }

        void PrintErrors(ApiError error)
        {
            Log.Error("Error: {0} - {1}", error.Code, error.Message);
            if (error.ErrorChain == null || error.ErrorChain.Count == 0) return;
            foreach (var _err in error.ErrorChain)
            {
                PrintErrors(_err);
            }
        }

        void PrintErrors(ErrorDetails error)
        {
            Log.Error("Error: {0} - {1}", error.Code, error.Message);
        }

        async void getip(object? state)
        {
            var response = await ipHttpClient.GetAsync("https://api.ipify.org?format=json");

            if (response == null)
            {
                Log.Warning("Could not get public ip! Response was null.");
                return;
            }

            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode == false)
            {
                Log.Warning("Response was not successful, {0}", content);
            }
            try
            {
                dynamic ipObject = JObject.Parse(content);
                string ip = ipObject.ip;
                if (string.IsNullOrWhiteSpace(ip))
                {
                    Log.Error("Response from ip server was not in the correct format, {0}", content);
                }
                else
                {
                    UpdateIp(ip);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "");
            }
        }

    }
}

using Newtonsoft.Json;

namespace CloudflareDDNS
{
    internal class Program
    {
        struct newip
        {
            public string ip;
        }
        static void Main(string[] args)
        {
            if (!File.Exists("settings.json"))
            {
                File.WriteAllText("settings.json", JsonConvert.SerializeObject(new Settings(), Formatting.Indented));
                Console.WriteLine("Created settings.json, please edit!");
                return;
            }

            Settings settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText("settings.json"));

            if (settings == null)
            {
                Console.WriteLine("Invalid configuration file!");
                return;
            }

            HttpClient client = new HttpClient();

            if (settings.AutoGetPublicIp)
            {
                var val = client.GetAsync("https://api.ipify.org?format=json").Result;
                if (val.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var _newip = JsonConvert.DeserializeObject<newip>(val.Content.ReadAsStringAsync().Result);
                    settings.NewIp = _newip.ip; 
                }
            }


            // Create the HttpContent for the form to be posted.
            var requestContent = new FormUrlEncodedContent(new Dictionary<string, string> {
                ["content"] = settings.NewIp,
                ["name"] = settings.DomainName,
                ["proxied"] = settings.Proxied.ToString().ToLower(),
                ["type"] = settings.Type,
                ["comment"] = settings.Comment,
                ["ttl"] = settings.TTL.ToString(),
            });

            // Get the response.
            HttpResponseMessage response = client.PutAsync(
                $"https://api.cloudflare.com/client/v4/zones/{settings.ZoneIdentifier}/dns_records/{settings.Identifier}",
                requestContent).Result;

            // Get the response content.
            var result = response.Content.ReadAsStringAsync().Result;

            // Write the output.
            Console.WriteLine(result);
            
        }
    }
}
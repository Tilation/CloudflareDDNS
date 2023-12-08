using Newtonsoft.Json;
using Serilog;

namespace CloudflareDDNS
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string settingsFile = "./settings.json";
            Log.Logger = new LoggerConfiguration().WriteTo.Console().WriteTo.File("log.log").CreateLogger();


            if (!File.Exists(settingsFile))
            {
                File.WriteAllText(settingsFile, JsonConvert.SerializeObject(new Settings(), Formatting.Indented));
                Log.Information("Created settings file at: {0}", settingsFile);
                Log.Information("Edit the file, set the Ready property to true, and re-run.");
                return;
            }

            Settings settings;

            try
            {
                if (JsonConvert.DeserializeObject<Settings>(File.ReadAllText(settingsFile)) is Settings s)
                {
                    settings = s;
                }
                else
                {
                    Log.Error("Could not read settings!");
                    return;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Could not read settings!");
                return;
            }

            if (settings.Ready == false)
            {
                Log.Warning("Property Ready is {0}, set it to {1} to continue execution.", false, true);
                return;
            }

            Log.Information("Starting main loop");


            CloudflareDDNSState state = new CloudflareDDNSState(settings);

            while (true)
            {
                Console.ReadLine();
            }
        }
    }
}
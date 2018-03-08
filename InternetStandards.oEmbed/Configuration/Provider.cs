using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using InternetStandards.oEmbed.Configuration;
using Newtonsoft.Json;

namespace InternetStandards.oEmbed.Configuration
{
    public class Provider
    {
        [JsonProperty("provider_name")] public string Name { get; set; }

        [JsonProperty("provider_url")] public string Url { get; set; }

        public Endpoint[] Endpoints { get; set; }

        public static Provider[] Providers
        {
            get
            {
                var assembly = Assembly.GetExecutingAssembly();
                using (var textStreamReader =
                    new StreamReader(
                        assembly.GetManifestResourceStream("InternetStandards.oEmbed.Configuration.providers.json")))
                {
                    using (var jsonReader = new JsonTextReader(textStreamReader))
                    {
                        return JsonSerializer.CreateDefault().Deserialize<Provider[]>(jsonReader);
                    }
                }
            }
        }

        public static IEnumerable<(Endpoint Endpoint, Provider Provider)> FindEndpoints(IEnumerable<Provider> providers,
            string url)
        {
            return from provider in providers
                where provider.Endpoints != null
                from endpoint in provider.Endpoints.Where(endpoint => endpoint.Match(url))
                select (endpoint, provider);
        }

        public static IEnumerable<(Endpoint Endpoint, Provider Provider)> FindEndpoints(string url)
        {
            return FindEndpoints(Providers, url);
        }
    }
}

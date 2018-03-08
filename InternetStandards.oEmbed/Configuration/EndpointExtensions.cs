using System.Linq;
using System.Text.RegularExpressions;

namespace InternetStandards.oEmbed.Configuration
{
    public static class EndpointExtensions
    {
        public static string ExpandUrl(this Endpoint endpoint, string format, out bool changed)
        {
            changed = endpoint.Url.Contains("{format}");
            return changed ? endpoint.Url.Replace("{format}", format) : endpoint.Url;
        }

        public static string ExpandUrl(this Endpoint endpoint, string format)
        {
            return ExpandUrl(endpoint, format, out _);
        }

        public static bool Match(this Endpoint endpoint, string url)
        {
            return endpoint.Schemes != null && endpoint.Schemes.Any(scheme => Match(scheme, url));
        }

        public static bool Match(string scheme, string url)
        {
            return Regex.IsMatch(url, string.Join(".*?", scheme.Split('*').Select(Regex.Escape)));
        }
    }
}

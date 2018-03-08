using System.Linq;
using System.Text.RegularExpressions;

namespace InternetStandards.oEmbed.Configuration
{
    public class Endpoint
    {
        public string[] Schemes { get; set; }
        public string Url { get; set; }
        public string[] Formats { get; set; }
        public bool Discovery { get; set; }
    }
}
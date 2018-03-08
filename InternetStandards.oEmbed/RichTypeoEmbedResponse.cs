using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace InternetStandards.oEmbed
{
    [XmlRoot("oembed")]
    public class RichTypeoEmbedResponse : oEmbedResponse
    {
        [JsonProperty("html")]
        [XmlElement(ElementName = "html")]
        public string Html { get; set; }

        [JsonProperty("width")]
        [XmlElement(ElementName = "width")]
        public double Width { get; set; }

        [JsonProperty("height")]
        [XmlElement(ElementName = "height")]
        public double Height { get; set; }
    }
}

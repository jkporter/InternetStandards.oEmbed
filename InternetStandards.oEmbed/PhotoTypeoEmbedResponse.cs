using System.Xml.Serialization;
using Newtonsoft.Json;

namespace InternetStandards.oEmbed
{
    [XmlRoot("oembed")]
    public class PhotoTypeoEmbedResponse : oEmbedResponse
    {
        [JsonProperty("url")]
        [XmlElement(ElementName = "url")]
        public string Url { get; set; }

        [JsonProperty("width")]
        [XmlElement(ElementName = "width")]
        public double Width { get; set; }

        [JsonProperty("height")]
        [XmlElement(ElementName = "height")]
        public double Height { get; set; }
    }
}

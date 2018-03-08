using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace InternetStandards.oEmbed
{
    [XmlRoot("oembed")]
    public class oEmbedResponse
    {
        [JsonProperty("type")]
        [XmlElement(ElementName = "type")]
        public string Type { get; set; }

        [JsonProperty("version")]
        [XmlElement(ElementName = "version")]
        public string Version { get; set; }

        [JsonProperty("title")]
        [XmlElement(ElementName = "title")]
        public string Title { get; set; }

        [JsonProperty("author_name")]
        [XmlElement(ElementName = "author_name")]
        public string AuthorName { get; set; }

        [JsonProperty("author_url")]
        [XmlElement(ElementName = "author_url")]
        public string AuthorUrl { get; set; }

        [JsonProperty("provider_name")]
        [XmlElement(ElementName = "provider_name")]
        public string ProviderName { get; set; }

        [JsonProperty("provider_url")]
        [XmlElement(ElementName = "provider_url")]
        public string ProviderUrl { get; set; }

        [JsonProperty("cache_age")]
        [XmlElement(ElementName = "cache_age")]
        public double? CacheAge { get; set; }

        [JsonProperty("thumbnail_url")]
        [XmlElement(ElementName = "thumbnail_url")]
        public string ThumbnailUrl { get; set; }

        [JsonProperty("thumbnail_width")]
        [XmlElement(ElementName = "thumbnail_width")]
        public double? ThumbnailWidth { get; set; }

        [JsonProperty("thumbnail_height")]
        [XmlElement(ElementName = "thumbnail_height")]
        public double? ThumbnailHeight { get; set; }
    }
}
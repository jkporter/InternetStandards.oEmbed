using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using AngleSharp.Extensions;
using AngleSharp.Network;
using AngleSharp.Parser.Html;
using InternetStandards.oEmbed.Network;
using InternetStandards.WHATWG.Url;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using HttpMethod = System.Net.Http.HttpMethod;

namespace InternetStandards.oEmbed
{
    public class oEmbedClient
    {
        private readonly HttpClient _httpClient;

        public oEmbedClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public oEmbedClient() : this(new HttpClient())
        { }

        public Task<T> MakeRequest<T>(string endpointUrl, double? maxWidth = null, double? maxHeight = null,
            Dictionary<string, string> additionalArguments = null, CancellationToken cancellationToken = default)
            where T : oEmbedResponse
        {
            return MakeRequest<T>(endpointUrl, null, maxWidth, maxHeight, additionalArguments: additionalArguments,
                cancellationToken: cancellationToken);
        }

        public async Task<T> MakeRequest<T>(string endpointUrl, string url = null,
            double? maxWidth = null,
            double? maxHeight = null, string format = null, Dictionary<string, string> additionalArguments = null,
            CancellationToken cancellationToken = default) where T : oEmbedResponse
        {
            List<(string name, string value)> arguments;

            var queryDelimterIndex = endpointUrl.IndexOf("?", StringComparison.InvariantCulture);
            if (queryDelimterIndex != -1)
            {
                var query = endpointUrl.Substring(queryDelimterIndex).Remove(0, 1);
                endpointUrl = endpointUrl.Substring(0, queryDelimterIndex + 1);
                arguments = new List<(string name, string value)>(application_x_www_form_urlencoded.Parse(query));
            }
            else
            {
                arguments = new List<(string name, string value)>();
            }

            void AddArgument(string name, string value)
            {
                if (value == null)
                    return;

                var index = arguments.FindIndex(tuple => tuple.name == name);
                if (index == -1)
                {
                    arguments.Add((name, value));
                }
                else
                {
                    arguments[index] = (name, value);
                }
            }

            AddArgument("url", url);
            AddArgument("maxwidth", maxWidth?.ToString(CultureInfo.InvariantCulture));
            AddArgument("maxheight", maxHeight?.ToString(CultureInfo.InvariantCulture));
            AddArgument("format", format);

            if (additionalArguments != null)
                arguments.AddRange(additionalArguments.Select(argument => (argument.Key, argument.Value)));

            if (arguments.Count > 0)
            {
                if (!endpointUrl.EndsWith("?")) endpointUrl += "?";
                endpointUrl += application_x_www_form_urlencoded.Serializer(arguments);
            }

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, endpointUrl);
            httpRequest.Headers.Accept.ParseAdd("application/json;q=0.003");
            httpRequest.Headers.Accept.ParseAdd("text/xml;q=0.002");
            httpRequest.Headers.Accept.ParseAdd("application/xml;q=0.001");

            using (var response = await _httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false))
            {
                using (var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                {
                    Type GetoEmbedResponseType(string type)
                    {
                        switch (type)
                        {
                            case "photo":
                                return typeof(PhotoTypeoEmbedResponse);
                            case "video":
                                return typeof(VideoTypeoEmbedResponse);
                            case "link":
                                return typeof(LinkTypeoEmbedResponse);
                            case "rich":
                                return typeof(RichTypeoEmbedResponse);
                            default:
                                throw new Exception();
                        }
                    }

                    switch (response.Content.Headers.ContentType.MediaType.ToLowerInvariant())
                    {
                        case "application/json":
                            using (var streamReader = new StreamReader(responseStream))
                            {
                                using (var reader = new JsonTextReader(streamReader))
                                {
                                    var serializer = new JsonSerializer();
                                    var oEmbedResponse = serializer.Deserialize<JObject>(reader);
                                    return (T)oEmbedResponse.ToObject(
                                        GetoEmbedResponseType(oEmbedResponse["type"].Value<string>()));
                                }
                            }

                        case "text/xml":
                        case "application/xml":
                            var document = XDocument.Load(responseStream);
                            using (var xmlReader = document.CreateReader())
                            {
                                var xmlSerializer =
                                    new XmlSerializer(
                                        GetoEmbedResponseType(document.XPathSelectElement("oembed/type").Value));
                                return (T)xmlSerializer.Deserialize(xmlReader);
                            }
                    }
                }
            }

            throw new Exception();
        }

        public Task<oEmbedResponse> MakeRequest(string endpointUrl, string url = null,
            double? maxWidth = null,
            double? maxHeight = null, string format = null, Dictionary<string, string> additionalArguments = null,
            CancellationToken cancellationToken = default)
        {
            return MakeRequest<oEmbedResponse>(endpointUrl, url, maxWidth, maxHeight, format, additionalArguments,
                cancellationToken);
        }

        public static string[] oEmbedLinkTypes = { "application/json+oembed", "text/xml+oembed" };

        private IConfiguration AngleSharpConfig()
        {
            var defaultConfig = AngleSharp.Configuration.Default;
            var requesters = defaultConfig.WithDefaultLoader().Services.OfType<IRequester>().ToList();
            var requesterIndex = 0;
            for (; requesterIndex < requesters.Count; requesterIndex++)
            {
                var requester = requesters[requesterIndex];
                if (requester.SupportsProtocol("http") || requester.SupportsProtocol("https")) break;
            }
            requesters.Insert(requesterIndex, new HttpClientRequester(_httpClient));

            return defaultConfig.WithDefaultLoader(null, requesters);
        }

        public async Task<IEnumerable<(string Url, string Type)>> Discover(string uri,
            CancellationToken cancellationToken = default)
        {
            var context = BrowsingContext.New(AngleSharpConfig());
            var request = DocumentRequest.Get(new Url(uri));
            if (context?.Active != null)
            {
                request.Referer = context.Active.DocumentUri;
            }

            var loader = context.Loader;
            var download = loader.DownloadAsync(request);
            cancellationToken.Register(download.Cancel);

            using (var response = await download.Task.ConfigureAwait(false))
            {
                var syntax = response.GetContentType().Equals(new MimeType("application/xhtml+xml"))
                    ? LanguageSyntax.Xhtml
                    : LanguageSyntax.Html;
                return await Discover(response.Content, context, response.Address.ToString(), syntax, cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        public Task<IEnumerable<(string Url, string Type)>> Discover(TextReader source, string baseUri = null,
            LanguageSyntax syntax = LanguageSyntax.Html, CancellationToken cancellationToken = default)
        {
            return Discover(baseUri, syntax,
                async (p, b, c) => await p.ParseAsync(await source.ReadToEndAsync().ConfigureAwait(false), c)
                    .ConfigureAwait(false), null, (s, b) => XmlReader.Create(source, s, b), cancellationToken);
        }

        public Task<IEnumerable<(string Url, string Type)>> Discover(Stream source, string baseUri = null,
            LanguageSyntax syntax = LanguageSyntax.Html, CancellationToken cancellationToken = default)
        {
            return Discover(source, null, baseUri, syntax, cancellationToken);
        }

        private Task<IEnumerable<(string Url, string Type)>> Discover(Stream source, IBrowsingContext browsingContext, string baseUri = null,
            LanguageSyntax syntax = LanguageSyntax.Html, CancellationToken cancellationToken = default)
        {
            return Discover(baseUri, syntax, async (p, b, c) => await p.ParseAsync(source, c).ConfigureAwait(false),
                browsingContext, (s, b) => XmlReader.Create(source, s, b), cancellationToken);
        }

        private async Task<IEnumerable<(string Url, string Type)>> Discover(string baseUri, LanguageSyntax syntax,
            Func<HtmlParser, string, CancellationToken, Task<IDocument>> htmlParseAsync, IBrowsingContext browsingContext,
            Func<XmlReaderSettings, string, XmlReader> createReader, CancellationToken cancellationToken = default)
        {
            switch (syntax)
            {
                case LanguageSyntax.Html:
                    var parser = browsingContext == null
                        ? new HtmlParser(AngleSharpConfig())
                        : new HtmlParser(new HtmlParserOptions(), browsingContext);
                    return Discover(await htmlParseAsync(parser, baseUri, cancellationToken).ConfigureAwait(false));
                case LanguageSyntax.Xhtml:
                    using (var xmlReader = createReader(new XmlReaderSettings { DtdProcessing= DtdProcessing.Ignore}, baseUri))
                    {
                        return Discover(XDocument.Load(xmlReader, LoadOptions.SetBaseUri), xmlReader.NameTable);
                    }
                default:
                    throw new ArgumentOutOfRangeException(nameof(syntax), syntax, null);
            }
        }

        private static IEnumerable<(string Url, string Type)> Discover(IDocument document)
        {
            return document.Head.QuerySelectorAll("link[rel=alternate][type]").Cast<IHtmlLinkElement>()
                .Where(
                    link =>
                        oEmbedLinkTypes.Any(type =>
                            string.Compare(type, link.Type,
                                StringComparison.InvariantCultureIgnoreCase) == 0))
                .Select(link => (new Url(link.BaseUrl, link.Href).ToString(), link.Type));
        }

        private static IEnumerable<(string Url, string Type)> Discover(XDocument document, XmlNameTable nameTable)
        {
            var namespaceManager = new XmlNamespaceManager(nameTable);
            namespaceManager.AddNamespace("xhtml", "http://www.w3.org/1999/xhtml");
            return document.XPathSelectElements("/xhtml:html/xhtml:head/xhtml:link[@rel='alternate']", namespaceManager)
                .Where(link => oEmbedLinkTypes.Any(type =>
                    string.Compare(type, link.Attribute("type").Value, StringComparison.InvariantCultureIgnoreCase) ==
                    0)).Select(link => (new Url(new Url(link.BaseUri), link.Attribute("href").Value).ToString(),
                    link.Attribute("type").Value));
        }
    }
}

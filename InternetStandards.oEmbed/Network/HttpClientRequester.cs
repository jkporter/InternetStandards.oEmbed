using AngleSharp.Network;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace InternetStandards.oEmbed.Network
{
    internal class HttpClientRequester : IRequester
    {
        private readonly HttpClient _httpClient;

        public HttpClientRequester(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IResponse> RequestAsync(IRequest request, CancellationToken cancel)
        {
            using (var requestMessage =
                new HttpRequestMessage(ConvertMethod(request.Method), request.Address.ToString()))
            {
                foreach (var header in request.Headers.Where(h => h.Value != null))
                    requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
                if (request.Content != null)
                    requestMessage.Content = new StreamContent(request.Content);

                return new HttpResponseMessageResponse(await _httpClient.SendAsync(requestMessage, cancel)
                    .ConfigureAwait(false));
            }
        }

        public bool SupportsProtocol(string protocol)
        {
            return protocol == ProtocolNames.Http || protocol == ProtocolNames.Https;
        }

        private System.Net.Http.HttpMethod ConvertMethod(AngleSharp.Network.HttpMethod httpMethod)
        {
            switch (httpMethod)
            {
                case AngleSharp.Network.HttpMethod.Delete:
                    return System.Net.Http.HttpMethod.Delete;
                case AngleSharp.Network.HttpMethod.Get:
                    return System.Net.Http.HttpMethod.Get;
                case AngleSharp.Network.HttpMethod.Post:
                    return System.Net.Http.HttpMethod.Post;
                case AngleSharp.Network.HttpMethod.Put:
                    return System.Net.Http.HttpMethod.Put;
                default:
                    throw new ArgumentOutOfRangeException(nameof(httpMethod), httpMethod, null);
            }
        }
    }
}

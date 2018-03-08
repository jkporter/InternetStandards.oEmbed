using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using AngleSharp;
using AngleSharp.Network;

namespace InternetStandards.oEmbed.Network
{
    internal class HttpResponseMessageResponse : IResponse
    {
        private readonly HttpResponseMessage _response;

        public HttpResponseMessageResponse(HttpResponseMessage response)
        {
            _response = response;
            Headers = new MultiHttpHeadersDictionary(new[]
                {new HttpHeadersDictionary(response.Headers), new HttpHeadersDictionary(response.Content.Headers)});
        }

        public void Dispose()
        {
            _response.Dispose();
        }

        public HttpStatusCode StatusCode => _response.StatusCode;
        public Url Address => new Url(_response.RequestMessage.RequestUri.ToString());
        public IDictionary<string, string> Headers { get; }
        public Stream Content => _response.Content.ReadAsStreamAsync().GetAwaiter().GetResult();
    }
}

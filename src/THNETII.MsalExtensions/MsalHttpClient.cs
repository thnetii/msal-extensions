using System;
using System.Net.Http;

namespace Microsoft.Identity.Client
{
    public class MsalHttpClient : IMsalHttpClientFactory
    {
        private readonly HttpClient httpClient;

        public MsalHttpClient(HttpClient httpClient)
        {
            this.httpClient = httpClient
                ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public HttpClient GetHttpClient() => httpClient;
    }
}

using System;
using System.Net.Http;

using Microsoft.Identity.Client;

namespace THNETII.MsalExtensions.Hosting
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

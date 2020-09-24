using System;
using System.Net.Http;

namespace Microsoft.Identity.Client
{
    public static class MsalHttpClientFactoryExtensions
    {
        public static T WithHttpClient<T>(
            this AbstractApplicationBuilder<T> appBuilder,
            HttpClient httpClient
            ) where T : AbstractApplicationBuilder<T>
        {
            _ = appBuilder ?? throw new ArgumentNullException(nameof(appBuilder));

            return appBuilder.WithHttpClientFactory(new MsalHttpClient(httpClient));
        }

        public static T WithHttpClientFactory<T>(
            this AbstractApplicationBuilder<T> appBuilder,
            IHttpClientFactory httpFactory, string? name = default
            ) where T : AbstractApplicationBuilder<T>
        {
            _ = appBuilder ?? throw new ArgumentNullException(nameof(appBuilder));
            _ = httpFactory ?? throw new ArgumentNullException(nameof(httpFactory));

            var msalHttpFactory = new MsalHttpClientFactory(httpFactory, name);
            return appBuilder.WithHttpClientFactory(msalHttpFactory);
        }
    }
}

using System.Net.Http;

namespace Microsoft.Identity.Client
{
    public class MsalHttpClientFactory : IMsalHttpClientFactory
    {
        private readonly string? name;
        private readonly IHttpClientFactory httpFactory;

        public MsalHttpClientFactory(IHttpClientFactory httpFactory,
            string? name = default)
        {
            this.name = name;
            this.httpFactory = httpFactory
                ?? throw new System.ArgumentNullException(nameof(httpFactory));
        }

        public HttpClient GetHttpClient()
        {
            return name switch
            {
                null => httpFactory.CreateClient(),
                _ => httpFactory.CreateClient(name),
            };
        }
    }
}

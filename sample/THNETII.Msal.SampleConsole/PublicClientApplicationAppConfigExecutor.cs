using System;
using System.Net.Http;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;

namespace THNETII.Msal.SampleConsole
{
    public class PublicClientApplicationAppConfigExecutor
        : ClientApplicationBaseAppConfigExecutor
    {
        public PublicClientApplicationAppConfigExecutor(
            IHttpClientFactory httpClientFactory,
            IOptions<PublicClientApplicationOptions> appOptions,
            MsalTokenCacheStorageProvider cacheStorageProvider,
            ILoggerFactory? loggerFactory = null)
            : base(cacheStorageProvider, loggerFactory)
        {
            var opts = appOptions?.Value
                ?? throw new ArgumentNullException(nameof(appOptions));
            loggerFactory ??= Microsoft.Extensions.Logging.Abstractions
                .NullLoggerFactory.Instance;

            var appBuilder = PublicClientApplicationBuilder
                .CreateWithApplicationOptions(opts)
                .WithLoggerFactory(loggerFactory);
            if (!(httpClientFactory is null))
                appBuilder.WithHttpClientFactory(httpClientFactory);
            Application = appBuilder.Build();
        }

        protected override IClientApplicationBase BaseApplication => Application;
        public IPublicClientApplication Application { get; }
    }
}

using System;
using System.Net.Http;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;

namespace THNETII.Msal.SampleConsole
{
    public class ConfidentialClientApplicationAppConfigExecutor
        : ClientApplicationBaseAppConfigExecutor
    {
        public ConfidentialClientApplicationAppConfigExecutor(
            IHttpClientFactory httpClientFactory,
            IOptions<ConfidentialClientApplicationOptions> appOptions,
            MsalTokenCacheStorageProvider cacheStorageProvider,
            ILoggerFactory? loggerFactory = null)
            : base(cacheStorageProvider, loggerFactory)
        {
            var opts = appOptions?.Value
                ?? throw new ArgumentNullException(nameof(appOptions));
            loggerFactory ??= Microsoft.Extensions.Logging.Abstractions
                .NullLoggerFactory.Instance;

            var appBuilder = ConfidentialClientApplicationBuilder
                .CreateWithApplicationOptions(opts)
                .WithLoggerFactory(loggerFactory);
            if (!(httpClientFactory is null))
                appBuilder.WithHttpClientFactory(httpClientFactory);
            Application = appBuilder.Build();
        }

        protected override IClientApplicationBase BaseApplication => Application;
        public IConfidentialClientApplication Application { get; }
    }
}

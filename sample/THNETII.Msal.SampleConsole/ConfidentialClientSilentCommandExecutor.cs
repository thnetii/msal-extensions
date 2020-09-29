using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;

namespace THNETII.Msal.SampleConsole
{
    public class ConfidentialClientSilentCommandExecutor
        : ClientApplicationBaseSilentCommandExecutor
    {
        private readonly ConfidentialClientApplicationAppConfigExecutor appConfigExecutor;

        public ConfidentialClientSilentCommandExecutor(
            IServiceProvider serviceProvider,
            IOptions<SilentAcquireTokenOptions> acquireTokenOptions,
            MsalTokenCacheStorageProvider cacheStorageProvider,
            ILoggerFactory? loggerFactory = null)
            : base(acquireTokenOptions, cacheStorageProvider, loggerFactory)
        {
            appConfigExecutor = ActivatorUtilities.GetServiceOrCreateInstance
                <ConfidentialClientApplicationAppConfigExecutor>(serviceProvider);
        }

        protected override IClientApplicationBase BaseApplication => Application;
        public IConfidentialClientApplication Application =>
            appConfigExecutor.Application;

    }
}

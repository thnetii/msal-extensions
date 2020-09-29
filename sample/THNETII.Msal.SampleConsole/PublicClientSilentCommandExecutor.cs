using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;

namespace THNETII.Msal.SampleConsole
{
    public class PublicClientSilentCommandExecutor
        : ClientApplicationBaseSilentCommandExecutor
    {
        private readonly PublicClientApplicationAppConfigExecutor appConfigExecutor;

        public PublicClientSilentCommandExecutor(
            IServiceProvider serviceProvider,
            IOptions<SilentAcquireTokenOptions> acquireTokenOptions,
            MsalTokenCacheStorageProvider cacheStorageProvider,
            ILoggerFactory? loggerFactory = null)
            : base(acquireTokenOptions, cacheStorageProvider, loggerFactory)
        {
            appConfigExecutor = ActivatorUtilities.GetServiceOrCreateInstance
                <PublicClientApplicationAppConfigExecutor>(serviceProvider);
        }

        protected override IClientApplicationBase BaseApplication => Application;
        public IPublicClientApplication Application =>
            appConfigExecutor.Application;
    }
}

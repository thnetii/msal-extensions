using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;

namespace THNETII.Msal.SampleConsole
{
    public abstract class ConfidentialClientApplicationAcquireTokenExecutor
        : ClientApplicationBaseAcquireTokenExecutor
    {
        private readonly ConfidentialClientApplicationAppConfigExecutor appConfigExecutor;

        protected ConfidentialClientApplicationAcquireTokenExecutor(
            IServiceProvider serviceProvider,
            MsalTokenCacheStorageProvider cacheStorageProvider,
            ILoggerFactory? loggerFactory = null)
            : base(cacheStorageProvider, loggerFactory)
        {
            appConfigExecutor = ActivatorUtilities.GetServiceOrCreateInstance
                <ConfidentialClientApplicationAppConfigExecutor>(serviceProvider);
        }

        protected override IClientApplicationBase BaseApplication => Application;
        public IConfidentialClientApplication Application => appConfigExecutor.Application;
    }
}

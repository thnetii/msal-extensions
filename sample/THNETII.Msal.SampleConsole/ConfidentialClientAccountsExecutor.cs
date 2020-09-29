using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;

namespace THNETII.Msal.SampleConsole
{
    public class ConfidentialClientAccountsExecutor : ClientApplicationBaseAccountsExecutor
    {
        private readonly ConfidentialClientApplicationAppConfigExecutor appConfigExecutor;

        public ConfidentialClientAccountsExecutor(
            IServiceProvider serviceProvider,
            MsalTokenCacheStorageProvider cacheStorageProvider,
            ILoggerFactory? loggerFactory = null)
            : base(cacheStorageProvider, loggerFactory)
        {
            appConfigExecutor = ActivatorUtilities.GetServiceOrCreateInstance
                <ConfidentialClientApplicationAppConfigExecutor>(serviceProvider);
        }

        protected override IClientApplicationBase BaseApplication => Application;
        public IConfidentialClientApplication Application =>
            appConfigExecutor.Application;
    }
}

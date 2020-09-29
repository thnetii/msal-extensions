using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;

namespace THNETII.Msal.SampleConsole
{
    public class PublicClientAccountsExecutor : ClientApplicationBaseAccountsExecutor
    {
        private readonly PublicClientApplicationAppConfigExecutor appConfigExecutor;

        public PublicClientAccountsExecutor(
            IServiceProvider serviceProvider,
            MsalTokenCacheStorageProvider cacheStorageProvider,
            ILoggerFactory? loggerFactory = null)
            : base(cacheStorageProvider, loggerFactory)
        {
            appConfigExecutor = ActivatorUtilities.GetServiceOrCreateInstance
                <PublicClientApplicationAppConfigExecutor>(serviceProvider);
        }

        protected override IClientApplicationBase BaseApplication => Application;
        public IPublicClientApplication Application =>
            appConfigExecutor.Application;
    }
}

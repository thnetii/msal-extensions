using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;

namespace THNETII.Msal.SampleConsole
{
    public abstract class PublicClientApplicationAcquireTokenExecutor
        : ClientApplicationBaseAcquireTokenExecutor
    {
        private readonly PublicClientApplicationAppConfigExecutor appConfigExecutor;

        protected PublicClientApplicationAcquireTokenExecutor(
            IServiceProvider serviceProvider)
            : base(
                  serviceProvider.GetRequiredService<MsalTokenCacheStorageProvider>(),
                  serviceProvider.GetService<ILoggerFactory>()
                  )
        {
            appConfigExecutor = ActivatorUtilities.GetServiceOrCreateInstance
                <PublicClientApplicationAppConfigExecutor>(serviceProvider);
        }

        protected override IClientApplicationBase BaseApplication => Application;
        public IPublicClientApplication Application => appConfigExecutor.Application;
    }
}

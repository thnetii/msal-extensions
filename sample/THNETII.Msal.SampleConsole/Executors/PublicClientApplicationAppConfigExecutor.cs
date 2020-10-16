using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;

namespace THNETII.Msal.SampleConsole
{
    public class PublicClientApplicationAppConfigExecutor
        : ClientApplicationBaseAppConfigExecutor
    {
        public PublicClientApplicationAppConfigExecutor(
            ClientApplicationFactory clientApplicationFactory,
            ILoggerFactory? loggerFactory = null)
            : base(clientApplicationFactory, loggerFactory) { }

        protected override sealed IClientApplicationBase CreateClientApplication() =>
            ClientApplicationFactory.CreatePublicClientApplication();
    }
}

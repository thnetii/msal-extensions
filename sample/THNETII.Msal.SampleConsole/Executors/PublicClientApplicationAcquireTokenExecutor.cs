using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;

namespace THNETII.Msal.SampleConsole
{
    public abstract class PublicClientApplicationAcquireTokenExecutor
        : ClientApplicationBaseAcquireTokenExecutor
    {
        protected PublicClientApplicationAcquireTokenExecutor(
            ClientApplicationFactory clientApplicationFactory,
            ILoggerFactory? loggerFactory = null)
            : base(clientApplicationFactory, loggerFactory) { }

        protected override sealed IClientApplicationBase CreateClientApplication() =>
            CreatePublicClientApplication();

        protected virtual IPublicClientApplication CreatePublicClientApplication() =>
            ClientApplicationFactory.CreatePublicClientApplication();
    }
}

using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;

namespace THNETII.Msal.SampleConsole
{
    public abstract class ConfidentialClientApplicationAcquireTokenExecutor
        : ClientApplicationBaseAcquireTokenExecutor
    {
        protected ConfidentialClientApplicationAcquireTokenExecutor(
            ClientApplicationFactory clientApplicationFactory,
            ILoggerFactory? loggerFactory = null)
            : base(clientApplicationFactory, loggerFactory) { }

        protected override sealed IClientApplicationBase CreateClientApplication() =>
            ClientApplicationFactory.CreateConfidentialClientApplication();
    }
}

using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;

namespace THNETII.Msal.SampleConsole
{
    public class ConfidentialClientAccountsExecutor : ClientApplicationBaseAccountsExecutor
    {
        protected ConfidentialClientAccountsExecutor(
            ClientApplicationFactory clientApplicationFactory,
            ILoggerFactory? loggerFactory = null)
            : base(clientApplicationFactory, loggerFactory) { }

        protected override sealed IClientApplicationBase CreateClientApplication() =>
            ClientApplicationFactory.CreateConfidentialClientApplication();
    }
}

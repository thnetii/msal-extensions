using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;

namespace THNETII.Msal.SampleConsole
{
    public class ConfidentialClientSilentCommandExecutor
        : ClientApplicationBaseSilentCommandExecutor
    {
        public ConfidentialClientSilentCommandExecutor(
            ClientApplicationFactory clientApplicationFactory,
            IOptions<SilentAcquireTokenOptions> acquireTokenOptions,
            ILoggerFactory? loggerFactory = null)
            : base(clientApplicationFactory, acquireTokenOptions, loggerFactory) { }

        protected override sealed IClientApplicationBase CreateClientApplication() =>
            ClientApplicationFactory.CreateConfidentialClientApplication();
    }
}

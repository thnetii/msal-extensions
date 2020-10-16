using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;

namespace THNETII.Msal.SampleConsole
{
    public class PublicClientSilentCommandExecutor
        : ClientApplicationBaseSilentCommandExecutor
    {
        public PublicClientSilentCommandExecutor(
            ClientApplicationFactory clientApplicationFactory,
            IOptions<SilentAcquireTokenOptions> acquireTokenOptions,
            ILoggerFactory? loggerFactory = null)
            : base(clientApplicationFactory, acquireTokenOptions, loggerFactory) { }

        protected override sealed IClientApplicationBase CreateClientApplication() =>
            ClientApplicationFactory.CreatePublicClientApplication();
    }
}

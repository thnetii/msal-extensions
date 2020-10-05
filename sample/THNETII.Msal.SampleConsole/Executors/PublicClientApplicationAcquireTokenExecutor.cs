using System.Threading.Tasks;

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

        protected override sealed async Task<IClientApplicationBase> CreateClientApplication()
        {
            return await CreatePublicClientApplication()
                .ConfigureAwait(continueOnCapturedContext: false);
        }

        protected Task<IPublicClientApplication> CreatePublicClientApplication() =>
            ClientApplicationFactory.CreatePublicClientApplication();
    }
}

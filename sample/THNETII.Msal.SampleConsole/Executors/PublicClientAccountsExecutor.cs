using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;

namespace THNETII.Msal.SampleConsole
{
    public class PublicClientAccountsExecutor : ClientApplicationBaseAccountsExecutor
    {
        public PublicClientAccountsExecutor(
            ClientApplicationFactory clientApplicationFactory,
            ILoggerFactory? loggerFactory = null)
            : base(clientApplicationFactory, loggerFactory) { }

        protected override sealed async Task<IClientApplicationBase> CreateClientApplication()
        {
            return await ClientApplicationFactory
                .CreatePublicClientApplication()
                .ConfigureAwait(continueOnCapturedContext: false);
        }
    }
}

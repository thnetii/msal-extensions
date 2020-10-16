using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;

namespace THNETII.Msal.SampleConsole
{
    public class ClientApplicationBaseAccountsExecutor
        : ClientApplicationBaseExecutor
    {
        protected ClientApplicationBaseAccountsExecutor(
            ClientApplicationFactory clientApplicationFactory,
            ILoggerFactory? loggerFactory = null)
            : base(clientApplicationFactory, loggerFactory) { }

        public override sealed async Task<int> RunAsync(CancellationToken cancelToken = default)
        {
            var application = CreateClientApplication();
            var accounts = await application.GetAccountsAsync()
                .ConfigureAwait(continueOnCapturedContext: false);
            foreach (var account in accounts)
            {
                LogAccount(account);
            }

            return 0;
        }

        protected override IClientApplicationBase CreateClientApplication()
        {
            return ClientApplicationFactory.CreateClientApplication();
        }

        private void LogAccount(IAccount account)
        {
            using var scope = Logger.BeginScope(account);
            var messageText = new StringBuilder();
            var messageArgs = new List<object?>();

            MessageAdd(nameof(account.Username), account.Username);
            MessageAdd(nameof(account.Environment), account.Environment);
            MessageAdd(nameof(account.HomeAccountId), account.HomeAccountId);
            if (account.HomeAccountId is AccountId accountId)
            {
                messageText.Append("- ");
                MessageAdd(nameof(accountId.Identifier), accountId.Identifier);
                messageText.Append("- ");
                MessageAdd(nameof(accountId.ObjectId), accountId.ObjectId);
                messageText.Append("- ");
                MessageAdd(nameof(accountId.TenantId), accountId.TenantId);
            }

            Logger.LogInformation(messageText.ToString().Trim(), messageArgs.ToArray());

            static string GetMessage(string name) => $"{name}: {{{name}}}";
            void MessageAdd(string name, object value)
            {
                messageText.AppendLine(GetMessage(name));
                messageArgs.Add(value);
            }
        }
    }
}

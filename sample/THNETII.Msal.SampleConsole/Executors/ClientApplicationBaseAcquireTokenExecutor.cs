using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;

namespace THNETII.Msal.SampleConsole
{
    public abstract class ClientApplicationBaseAcquireTokenExecutor
        : ClientApplicationBaseExecutor
    {
        protected ClientApplicationBaseAcquireTokenExecutor(
            ClientApplicationFactory clientApplicationFactory,
            ILoggerFactory? loggerFactory = null)
            : base(clientApplicationFactory, loggerFactory) { }

        public override sealed async Task<int> RunAsync(
            CancellationToken cancelToken = default)
        {
            AuthenticationResult authResult;
            try
            {
                authResult = await ExecuteAcquireToken(cancelToken)
                    .ConfigureAwait(continueOnCapturedContext: false);
            }
            catch (OperationCanceledException cancelExcept)
            {
                Logger.LogError(cancelExcept.Message);
                return cancelExcept.HResult switch
                {
                    0 => 1,
                    int code => code
                };
            }

            LogAuthenticateResult(authResult);

            return 0;
        }

        protected abstract Task<AuthenticationResult> ExecuteAcquireToken(
            CancellationToken cancelToken = default);

        private void LogAuthenticateResult(AuthenticationResult authResult)
        {
            using var scope = Logger.BeginScope(authResult);
            var messageText = new StringBuilder();
            var messageArgs = new List<object?>();
            static string GetMessage(string name) => $"{name}: {{{name}}}";
            void MessageAdd(string name, object value)
            {
                messageText.AppendLine(GetMessage(name));
                messageArgs.Add(value);
            }

            MessageAdd(nameof(authResult.IsExtendedLifeTimeToken), authResult.IsExtendedLifeTimeToken);
            MessageAdd(nameof(authResult.UniqueId), authResult.UniqueId);
            MessageAdd(nameof(authResult.ExpiresOn), authResult.ExpiresOn);
            MessageAdd(nameof(authResult.ExtendedExpiresOn), authResult.ExtendedExpiresOn);
            MessageAdd(nameof(authResult.TenantId), authResult.TenantId);
            MessageAdd(nameof(authResult.Account), authResult.Account);
            if (authResult.Account is IAccount account)
            {
                messageText.AppendLine($"- {nameof(account.Username)}: {{{nameof(authResult.Account)}_{nameof(account.Username)}}}");
                messageArgs.Add(account.Username);
                messageText.AppendLine($"- {nameof(account.Environment)}: {{{nameof(authResult.Account)}_{nameof(account.Environment)}}}");
                messageArgs.Add(account.Environment);
                messageText.AppendLine($"- {nameof(account.HomeAccountId)}: {{{nameof(authResult.Account)}_{nameof(account.HomeAccountId)}}}");
                messageArgs.Add(account.HomeAccountId);
                if (account.HomeAccountId is AccountId accountId)
                {
                    messageText.AppendLine($"  - {nameof(accountId.Identifier)}: {{{nameof(authResult.Account)}_{nameof(account.HomeAccountId)}_{nameof(accountId.Identifier)}}}");
                    messageArgs.Add(accountId.Identifier);
                    messageText.AppendLine($"  - {nameof(accountId.ObjectId)}: {{{nameof(authResult.Account)}_{nameof(account.HomeAccountId)}_{nameof(accountId.ObjectId)}}}");
                    messageArgs.Add(accountId.ObjectId);
                    messageText.AppendLine($"  - {nameof(accountId.TenantId)}: {{{nameof(authResult.Account)}_{nameof(account.HomeAccountId)}_{nameof(accountId.TenantId)}}}");
                    messageArgs.Add(accountId.TenantId);
                }
            }
            MessageAdd(nameof(authResult.Scopes), string.Join(", ", authResult.Scopes ?? Enumerable.Empty<string>()));
            MessageAdd(nameof(authResult.CorrelationId), authResult.CorrelationId);
            MessageAdd(nameof(authResult.AuthenticationResultMetadata), authResult.AuthenticationResultMetadata);
            if (authResult.AuthenticationResultMetadata is AuthenticationResultMetadata metadata)
            {
                messageText.AppendLine($"- {nameof(metadata.TokenSource)}: {{{nameof(authResult.Account)}_{nameof(metadata.TokenSource)}}}");
                messageArgs.Add(metadata.TokenSource);
            }

            string message = messageText.ToString().Trim();
            Logger.LogInformation(message, messageArgs.ToArray());
        }
    }
}

using System.CommandLine;

using THNETII.CommandLine.Hosting;

namespace THNETII.Msal.SampleConsole
{
    public class AccountCommandDefinition<TExecutor>
        : CommandLineHostingDefinition<TExecutor>
        where TExecutor : ClientApplicationBaseAccountsExecutor
    {
        public AccountCommandDefinition()
        {
            Command = new Command("accounts", "List all known cached commands")
            { Handler = CommandHandler };
        }

        public override Command Command { get; }
    }
}

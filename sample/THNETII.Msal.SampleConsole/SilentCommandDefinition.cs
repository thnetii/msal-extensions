using System.CommandLine;

namespace THNETII.Msal.SampleConsole
{
    public class SilentCommandDefinition<TExecutor>
        : AcquireTokenCommandDefinition<TExecutor>
        where TExecutor : ClientApplicationBaseSilentCommandExecutor
    {
        public SilentCommandDefinition()
        {
            Command = new Command("silent", "Perform silent locally cached token acquisition")
            { Handler = CommandHandler };

            AccountOption = new Option<string>("--account")
            {
                Name = nameof(SilentAcquireTokenOptions.AccountIdentifier),
                Description = "Previously acquired Account identifier",
                Argument = { Name = "ACCOUNTID" }
            };
            AccountOption.AddAlias("-a");
            Command.AddOption(AccountOption);

            ForceRefreshOption = new Option<bool>("--force-refresh")
            {
                Name = nameof(SilentAcquireTokenOptions.ForceRefresh),
                Description = "Force access token refresh using cached refresh token",
            };
            Command.AddOption(ForceRefreshOption);

            Command.AddOption(ScopesOption);
        }

        public override Command Command { get; }
        public Option<string> AccountOption { get; }
        public Option<bool> ForceRefreshOption { get; }
    }
}

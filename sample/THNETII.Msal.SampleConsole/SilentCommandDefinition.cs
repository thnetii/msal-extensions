using System.CommandLine;

using THNETII.CommandLine.Hosting;

namespace THNETII.Msal.SampleConsole
{
    public class SilentCommandDefinition
        : CommandLineHostingDefinition<SilentCommandExecutor>
    {
        public SilentCommandDefinition()
        {
            Command = new Command("silent", "Perform silent locally cached token acquisition")
            { Handler = CommandHandler };

            AccountOption = new Option<string>("--account")
            {
                Name = nameof(SilentAcquireTokenOptions.AccountIdentifier),
                Description = "Previously acquired Account identifier",
                Argument = { Name = "ACCOUNT" }
            };
            AccountOption.AddAlias("-a");
            Command.AddOption(AccountOption);

            ForceRefreshOption = new Option<bool>("--force-refresh")
            {
                Name = nameof(SilentAcquireTokenOptions.ForceRefresh),
                Description = "Force access token refresh using cached refresh token",
            };
            Command.AddOption(ForceRefreshOption);

            ScopesArgument = new Argument<string[]>()
            {
                Name = nameof(SilentAcquireTokenOptions.Scopes),
                Description = "Requested access scopes",
                Arity = ArgumentArity.ZeroOrMore
            };
            Command.AddArgument(ScopesArgument);
        }

        public override Command Command { get; }
        public Option<string> AccountOption { get; }
        public Argument<string[]> ScopesArgument { get; }
        public Option<bool> ForceRefreshOption { get; }
    }
}

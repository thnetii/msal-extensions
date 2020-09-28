using System.CommandLine;

using THNETII.CommandLine.Hosting;

namespace THNETII.Msal.SampleConsole
{
    public class IntegratedWindowsAuthenticationCommandDefinition
        : CommandLineHostingDefinition<IntegratedWindowsAuthenticationCommandExecutor>
    {
        public IntegratedWindowsAuthenticationCommandDefinition()
        {
            Command = new Command("iwa", "Perform Integrated Windows Authenticated Token acquisition")
            { Handler = CommandHandler };

            UserNameOption = new Option<string>("--username")
            {
                Name = nameof(IntegratedWindowsAuthenticationAcquireTokenOptions.Username),
                Description = "User Principal Name to authenticate",
                Argument = { Name = "UPN" }
            };
            UserNameOption.AddAlias("-u");
            Command.AddOption(UserNameOption);

            ScopesArgument = new Argument<string[]>()
            {
                Name = nameof(IntegratedWindowsAuthenticationAcquireTokenOptions.Scopes),
                Description = "Requested access scopes",
                Arity = ArgumentArity.ZeroOrMore
            };
            Command.AddArgument(ScopesArgument);
        }

        public override Command Command { get; }
        public Option<string> UserNameOption { get; }
        public Argument<string[]> ScopesArgument { get; }
    }
}

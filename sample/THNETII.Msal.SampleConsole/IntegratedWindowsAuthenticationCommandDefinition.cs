using System.CommandLine;

namespace THNETII.Msal.SampleConsole
{
    public class IntegratedWindowsAuthenticationCommandDefinition
        : AcquireTokenCommandDefinition<IntegratedWindowsAuthenticationCommandExecutor>
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

            Command.AddOption(ScopesOption);
        }

        public override Command Command { get; }
        public Option<string> UserNameOption { get; }
    }
}

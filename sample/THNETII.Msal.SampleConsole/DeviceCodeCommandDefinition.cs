using System.CommandLine;

namespace THNETII.Msal.SampleConsole
{
    public class DeviceCodeCommandDefinition
        : AcquireTokenCommandDefinition<DeviceCodeCommandExecutor>
    {
        public DeviceCodeCommandDefinition() : base()
        {
            Command = new Command("dca", "Perform Device Code Token acquisition")
            { Handler = CommandHandler };

            Command.AddOption(ScopesOption);
        }

        public override Command Command { get; }
    }
}

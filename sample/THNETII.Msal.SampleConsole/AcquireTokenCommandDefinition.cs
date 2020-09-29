using System.CommandLine;

using THNETII.CommandLine.Hosting;

namespace THNETII.Msal.SampleConsole
{
    public abstract class AcquireTokenCommandDefinition<TExecutor>
        : CommandLineHostingDefinition<TExecutor>
        where TExecutor : ClientApplicationBaseAcquireTokenExecutor
    {
        protected AcquireTokenCommandDefinition()
        {
            ScopesOption = new Option<string[]>("--scopes")
            {
                Name = nameof(SilentAcquireTokenOptions.Scopes),
                Description = "Requested access scopes",
                Argument = {
                    Name = "SCOPES",
                    Arity = ArgumentArity.ZeroOrMore
                }
            };
        }

        public Option<string[]> ScopesOption { get; }
    }
}

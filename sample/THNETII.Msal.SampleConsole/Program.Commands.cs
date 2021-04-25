using System.CommandLine;

using Microsoft.Identity.Client;

using THNETII.CommandLine.Hosting;

namespace THNETII.Msal.SampleConsole
{
    partial class Program
    {
        private static RootCommand CreateRootCommand()
        {
            string description = CommandLineHost.GetAssemblyDescription(typeof(Program));
            var cmdRoot = new RootCommand(description);
            var rootClientIdOption = new Option<string>("--clientid")
            {
                Name = nameof(ApplicationOptions.ClientId),
                Description = "Client ID",
                Argument = { Name = "CLIENTID" }
            };
            rootClientIdOption.AddAlias("-c");
            cmdRoot.AddOption(rootClientIdOption);
            var rootTenantIdOption = new Option<string>("--tenant")
            {
                Name = nameof(ApplicationOptions.TenantId),
                Description = "Tenant ID (aka. Realm)",
                Argument = { Name = "TENANT" }
            };
            rootTenantIdOption.AddAlias("-t");
            cmdRoot.AddOption(rootTenantIdOption);
            var rootInstanceOption = new Option<string>("--instance")
            {
                Name = nameof(ApplicationOptions.Instance),
                Description = "Azure AD instance URL",
                Argument = { Name = "INSTANCE" }
            };
            cmdRoot.AddOption(rootInstanceOption);
            var rootPiiLoggingOption = new Option<bool>("--pii-logging")
            {
                Name = nameof(ApplicationOptions.EnablePiiLogging),
                Description = "Enable loggig of Personal Identifiable Information",
            };
            cmdRoot.AddOption(rootPiiLoggingOption);

            cmdRoot.AddCommand(CreatePublicCommand());
            cmdRoot.AddCommand(CreateConfidentialCommand());

            return cmdRoot;
        }

        private static Option CreateAcquireTokenScopesOption()
        {
            return new Option<string[]>("--scopes")
            {
                Name = nameof(SilentAcquireTokenOptions.Scopes),
                Description = "Requested access scopes",
                Argument = {
                    Name = "SCOPES",
                    Arity = ArgumentArity.ZeroOrMore
                }
            };
        }

        private static Command CreatePublicCommand()
        {
            var pcaCommand = new Command("public", "Public Client Application")
            { Handler = CommandLineHost.GetCommandHandler<PublicClientApplicationAppConfigExecutor>() };
            pcaCommand.AddAlias("pca");

            pcaCommand.AddCommand(CreateDeviceCodeAuthorizationCommand());
            pcaCommand.AddCommand(CreateIntegratedWindowsAuthenticationCommand());
            pcaCommand.AddCommand(CreateAccountsCommand());
            pcaCommand.AddCommand(CreateSilentTokenAcquisitionCommand());

            return pcaCommand;
        }

        private static Command CreateConfidentialCommand()
        {
            var ccaCommand = new Command("confidential", "Confidential Client Application")
            { Handler = CommandLineHost.GetCommandHandler<ConfidentialClientApplicationAppConfigExecutor>() };
            ccaCommand.AddAlias("cca");
            var ccaClientSecretOption = new Option<string>("--secret")
            {
                Name = nameof(ConfidentialClientApplicationOptions.ClientSecret),
                Description = "Client Secret",
                Argument = { Name = "SECRET" }
            };
            ccaClientSecretOption.AddAlias("-s");
            ccaCommand.AddOption(ccaClientSecretOption);

            ccaCommand.AddCommand(CreateAccountsCommand());
            ccaCommand.AddCommand(CreateSilentTokenAcquisitionCommand());

            return ccaCommand;
        }

        private static Command CreateDeviceCodeAuthorizationCommand()
        {
            var dcaCommand = new Command("dca", "Perform Device Code Token acquisition")
            { Handler = CommandLineHost.GetCommandHandler<DeviceCodeCommandExecutor>() };

            dcaCommand.AddOption(CreateAcquireTokenScopesOption());

            return dcaCommand;
        }

        private static Command CreateIntegratedWindowsAuthenticationCommand()
        {
            var iwaCommand = new Command("iwa", "Perform Integrated Windows Authenticated Token acquisition")
            { Handler = CommandLineHost.GetCommandHandler<IntegratedWindowsAuthenticationCommandExecutor>() };

            var iwaUserNameOption = new Option<string>("--username")
            {
                Name = nameof(IntegratedWindowsAuthenticationAcquireTokenOptions.Username),
                Description = "User Principal Name to authenticate",
                Argument = { Name = "UPN" }
            };
            iwaUserNameOption.AddAlias("-u");
            iwaCommand.AddOption(iwaUserNameOption);

            iwaCommand.AddOption(CreateAcquireTokenScopesOption());

            return iwaCommand;
        }

        private static Command CreateAccountsCommand()
        {
            var accCommand = new Command("accounts", "List all known cached accounts")
            { Handler = CommandLineHost.GetCommandHandler<ClientApplicationBaseAccountsExecutor>() };

            return accCommand;
        }

        private static Command CreateSilentTokenAcquisitionCommand()
        {
            var silentCommand = new Command("silent", "Perform silent locally cached token acquisition")
            { Handler = CommandLineHost.GetCommandHandler<ClientApplicationBaseSilentCommandExecutor>() };

            var silentAccountOption = new Option<string>("--account")
            {
                Name = nameof(SilentAcquireTokenOptions.AccountIdentifier),
                Description = "Previously acquired Account identifier",
                Argument = { Name = "ACCOUNTID" }
            };
            silentAccountOption.AddAlias("-a");
            silentCommand.AddOption(silentAccountOption);

            var silentForceRefreshOption = new Option<bool>("--force-refresh")
            {
                Name = nameof(SilentAcquireTokenOptions.ForceRefresh),
                Description = "Force access token refresh using cached refresh token",
            };
            silentCommand.AddOption(silentForceRefreshOption);

            silentCommand.AddOption(CreateAcquireTokenScopesOption());

            return silentCommand;
        }
    }
}

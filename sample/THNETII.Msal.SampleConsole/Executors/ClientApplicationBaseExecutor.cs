using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;

using THNETII.CommandLine.Hosting;

namespace THNETII.Msal.SampleConsole
{
    public abstract class ClientApplicationBaseExecutor : ICommandLineExecutor
    {
        protected ClientApplicationBaseExecutor(
            ClientApplicationFactory clientApplicationFactory,
            ILoggerFactory? loggerFactory = null)
        {
            ClientApplicationFactory = clientApplicationFactory
                ?? throw new ArgumentNullException(nameof(clientApplicationFactory));
            loggerFactory ??= Microsoft.Extensions.Logging.Abstractions
                .NullLoggerFactory.Instance;
            Logger = loggerFactory.CreateLogger(GetType());
        }

        public ClientApplicationFactory ClientApplicationFactory { get; }
        public ILogger Logger { get; }

        public abstract Task<int> RunAsync(CancellationToken cancelToken = default);

        protected virtual IClientApplicationBase CreateClientApplication() =>
            ClientApplicationFactory.CreateClientApplication();
    }
}

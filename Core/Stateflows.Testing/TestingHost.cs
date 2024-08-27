using System.Threading;
using Microsoft.Extensions.Hosting;

namespace Stateflows.Testing
{
    internal class TestingHost : IHostApplicationLifetime
    {
        private readonly CancellationTokenSource ApplicationStartedToken = new CancellationTokenSource();
        public CancellationToken ApplicationStarted => ApplicationStartedToken.Token;

        private readonly CancellationTokenSource ApplicationStoppingToken = new CancellationTokenSource();
        public CancellationToken ApplicationStopping => ApplicationStoppingToken.Token;

        private readonly CancellationTokenSource ApplicationStoppedToken = new CancellationTokenSource();
        public CancellationToken ApplicationStopped => ApplicationStoppedToken.Token;

        public void StartApplication()
        {
            ApplicationStartedToken.Cancel();
        }

        public void StopApplication()
        {
            ApplicationStoppingToken.Cancel();
            ApplicationStoppedToken.Cancel();
        }
    }
}

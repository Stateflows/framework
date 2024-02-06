using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Tenant;
using Microsoft.Extensions.Logging;

namespace Stateflows.Common.Initializer
{
    internal class ThreadInitializer : IHostedService
    {
        private readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

        private readonly TenantsExecutor Executor;
        private readonly IBehaviorLocator Locator;
        private readonly IServiceScope Scope;
        private readonly ILogger<ThreadInitializer> Logger;

        private IServiceProvider ServiceProvider
            => Scope.ServiceProvider;

        public ThreadInitializer(IServiceProvider serviceProvider)
        {
            Scope = serviceProvider.CreateScope();
            Executor = ServiceProvider.GetRequiredService<TenantsExecutor>();
            Locator = ServiceProvider.GetRequiredService<IBehaviorLocator>();
            Logger = ServiceProvider.GetRequiredService<ILogger<ThreadInitializer>>();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                await Executor.ExecuteByTenantsAsync(() => InitiateBehaviors());
            }
            catch (Exception e)
            {
                Logger.LogError(LogTemplates.ExceptionLogTemplate, typeof(ThreadInitializer).FullName, nameof(StartAsync), e.GetType().Name, e.Message);
            }
        }

        private async Task InitiateBehaviors()
        {
            var tokens = BehaviorClassesInitializations.Instance.InitializationTokens;

            await Task.WhenAll(
                tokens.Select(async token =>
                {
                    token.RefreshEnvironment();

                    if (Locator.TryLocateBehavior(token.BehaviorClass.ToId(string.Empty), out var behavior))
                    {
                        await behavior.InitializeAsync(await token.InitializationRequestFactory(ServiceProvider, token.BehaviorClass));
                    }
                })
            );
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            CancellationTokenSource.Cancel();

            Scope.Dispose();

            return Task.CompletedTask;
        }
    }
}
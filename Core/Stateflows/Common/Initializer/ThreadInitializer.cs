using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Interfaces;

namespace Stateflows.Common.Initializer
{
    internal class ThreadInitializer : IHostedService
    {
        private readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

        private readonly IStateflowsTenantsManager TenantsManager;

        private readonly IBehaviorLocator Locator;

        private readonly IServiceScope Scope;

        private IServiceProvider ServiceProvider
            => Scope.ServiceProvider;

        public ThreadInitializer(IServiceProvider serviceProvider)
        {
            Scope = serviceProvider.CreateScope();
            TenantsManager = ServiceProvider.GetRequiredService<IStateflowsTenantsManager>();
            Locator = ServiceProvider.GetRequiredService<IBehaviorLocator>();
        }

        public Task StartAsync(CancellationToken cancellationToken)
            => TenantsManager.ExecuteByTenantsAsync(InitiateBehaviors);

        private async Task InitiateBehaviors(string tenantId)
        {
            var tokens = BehaviorClassesInitializations.Instance.InitializationTokens;

            await Task.WhenAll(
                tokens.Select(async token =>
                {
                    token.RefreshEnvironment();

                    if (Locator.TryLocateBehavior(new BehaviorId(token.BehaviorClass, string.Empty), out var behavior))
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
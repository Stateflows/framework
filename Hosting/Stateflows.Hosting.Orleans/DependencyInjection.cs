using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.Placement;
using Orleans.Runtime.Placement;
using Stateflows.Common;
using Stateflows.Common.Initializer;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Registration.Interfaces;
using Stateflows.Common.Utilities;
using Stateflows.Filters;
using Stateflows.Interfaces;
using Stateflows.Storage;

namespace Stateflows;

public static class DependencyInjection
{
    public static ISiloBuilder AddStateflowsRuntime(this ISiloBuilder builder,
        System.Action<StateflowsRuntimeOptions>? configure = null)
    {
        var options = builder.Services.AddOptions<StateflowsRuntimeOptions>();
        if (configure != null)
            options.Configure(configure);
        builder.Services.TryAddSingleton<IAttributeToFactoryMapper<StateflowsPersistentStateAttribute>, StateflowsPersistentStateMapper>();
        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IConfigurationValidator, RuntimeConfigurationValidator>());
        builder
            .Services
            .AddScoped<IStateflowsStorage, GrainStorageBridge>()
            .AddTransient<IBehaviorFactory, GrainBehaviorFactory>()
            .AddTransient<IStateflowsSubscriber, GrainSubscriber>()
            
            .AddSingleton<LocalBehaviorCatalog>()
            .AddSingleton<ISiloCapabilitiesClient, SiloCapabilitiesClient>()
            .AddSingleton<ClusterBehaviorProvider>()
            .AddSingleton<IBehaviorProvider>(services => services.GetRequiredService<ClusterBehaviorProvider>())
            .AddSingleton<IHostedService>(services => services.GetRequiredService<ClusterBehaviorProvider>())
            .AddPlacementFilter<BehaviorPlacementFilterStrategy, BehaviorPlacementFilterDirector>(ServiceLifetime.Singleton)
            .AddPlacementDirector<SiloPinnedPlacement, SiloPinnedPlacementDirector>();
        builder.Services.AddSingleton<ILifecycleParticipant<ISiloLifecycle>, ResourceShutdownParticipant>();
        builder.Services.AddOptions<BehaviorDiscoveryOptions>();

        builder.AddStartupTask(async (serviceProvider, cancellationToken) =>
        {
            await using var scope = serviceProvider.CreateAsyncScope();
            var scopedServices = scope.ServiceProvider;
            var catalog = scopedServices.GetRequiredService<LocalBehaviorCatalog>();
            var tenantExecutor = scopedServices.GetRequiredService<IStateflowsTenantExecutor>();
            var locator = scopedServices.GetRequiredService<IBehaviorLocator>();
            var logger = scopedServices.GetRequiredService<ILoggerFactory>().CreateLogger("Stateflows.Orleans.Startup");

            logger.LogInformation("Stateflows silo hosts {ClassCount} behavior classes.", catalog.Classes.Count());

            await tenantExecutor.ExecuteByTenantsAsync(async () =>
            {
                var tokens = BehaviorClassesInitializations.Instance.DefaultInstanceInitializationTokens
                    .Where(token => catalog.Classes.Contains(token.BehaviorClass))
                    .ToArray();

                logger.LogInformation("Initializing {Count} Stateflows default instances.", tokens.Length);

                var sendMethod = typeof(IBehavior).GetMethod(nameof(IBehavior.SendAsync));

                var results = await Task.WhenAll(tokens.Select(async token =>
                {
                    try
                    {
                        token.RefreshEnvironment();

                        if (!locator.TryLocateBehavior(new BehaviorId(token.BehaviorClass, string.Empty), out var behavior))
                        {
                            logger.LogWarning("Default instance of Stateflows behavior class {BehaviorClass} could not be located.", token.BehaviorClass);
                            return false;
                        }

                        var initializationEvent = await token.InitializationRequestFactory(scopedServices, token.BehaviorClass).ConfigureAwait(false);
                        var sendTask = (Task<SendResult>)sendMethod!
                            .MakeGenericMethod(initializationEvent.GetType())
                            .Invoke(behavior, [initializationEvent, null])!;
                        var result = await sendTask;

                        if (result.Status is EventStatus.Failed or EventStatus.Overloaded or EventStatus.Undelivered)
                        {
                            logger.LogWarning("Default instance of Stateflows behavior class {BehaviorClass} was not initialized: {Status}.", token.BehaviorClass, result.Status);
                            return false;
                        }

                        logger.LogDebug("Default instance of Stateflows behavior class {BehaviorClass} initialization returned {Status}.", token.BehaviorClass, result.Status);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to initialize default instance for behavior class {BehaviorClass}.", token.BehaviorClass);
                        return false;
                    }
                }));

                logger.LogInformation(
                    "Stateflows default instance initialization completed: {Succeeded} succeeded, {Failed} failed.",
                    results.Count(r => r), results.Count(r => !r));
            });

        });

        return builder;
    }
}
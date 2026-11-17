using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Engine;
using Stateflows.Common.Utilities;

namespace Stateflows.Common.Classes;

internal static class StagedOperations
{
    private static BehaviorId GetId<TContext>(object context)
        => typeof(TContext) == typeof(IParentBehaviorContext)
            ? ((IParentBehaviorContext)context).Id
            : typeof(TContext) == typeof(IOwnerBehaviorContext)
                ? ((IOwnerBehaviorContext)context).Id
                : ((IBehaviorContext)context).Id;

    private static IBehavior Locate(object context, BehaviorId id)
    {
        if (context is not IBehaviorLocator locator || !locator.TryLocateBehavior(id, out var behavior))
            throw new InvalidOperationException($"Cannot stage an operation: behavior '{id}' was not found.");
        return behavior;
    }

    public static void StageSending<TContext, TEvent>(object context, TEvent @event, IDictionary<string, EventHeader>? headers)
    {
        var bag = StagingBag.RequireCurrent();
        var id = GetId<TContext>(context);
        var snapshot = StateflowsJsonConverter.Clone(@event);
        var deliveryHeaders = headers == null ? null : StateflowsJsonConverter.Clone(headers);
        var envelope = snapshot.ToTypedEventHolder(deliveryHeaders);
        var source = ((IBehaviorContext)context).Id;
        var scopeFactory = ((IBehaviorContext)context).ServiceProvider.GetRequiredService<IServiceScopeFactory>();
        BehaviorObservation.Stage(id, envelope, () =>
        {
            if (bag.TryStageDurably(id, envelope))
                return;
            var logger = ((IBehaviorContext)context).ServiceProvider.GetRequiredService<ILoggerFactory>()
                .CreateLogger(typeof(StagedOperations));
            var behavior = Locate(context, id);
            bag.AddAfterCommit(() =>
            {
                using var scope = scopeFactory.CreateScope();
                using var observation = BehaviorObservation.Enter(source, scope.ServiceProvider);
                BehaviorObservation.Dispatch(id, envelope, () => _ = SendAsync(behavior, logger));
            }, behavior);
        });

        async Task SendAsync(IBehavior behavior, ILogger logger)
        {
            try
            {
                var result = await behavior.SendAsync(envelope, deliveryHeaders);
                if (result.Status is EventStatus.Failed or EventStatus.Aborted or EventStatus.Undelivered or EventStatus.Rejected or EventStatus.Overloaded)
                    logger.LogError("Staged send to {BehaviorId} returned {Status}.", id, result.Status);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Staged send to {BehaviorId} failed after source commit.", id);
            }
        }
    }
}

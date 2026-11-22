using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Stateflows.Common;
using Stateflows.Common.Utilities;
using Stateflows.Common.Engine;
using Stateflows.StateMachines.Exceptions;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Engine
{
    internal class Behaviors : StateMachinePlugin
    {
        public override async Task<EventStatus> ProcessEventAsync<TEvent>(IEventContext<TEvent> context, Func<Task<EventStatus>> next)
        {
            var status = EventStatus.Failed;
            try
            {
                return status = await next();
            }
            finally
            {
                if (context.Behavior.IsEmbedded)
                {
                    if (status == EventStatus.NotConsumed)
                    {
                        var headers = context.Headers
                            .Where(p => p.Value is not BehaviorEmbedding)
                            .ToDictionary();

                        var noBubblingAttribute = context.Event.GetType().GetCustomAttribute<NoBubblingAttribute>();
                        if (!headers.Values.Any(h => h is NoBubbling) && noBubblingAttribute == null)
                        {
                            var noForwardingAttribute = context.Event.GetType().GetCustomAttribute<NoForwardingAttribute>();
                            if (!headers.Values.Any(h => h is NoForwarding) && noForwardingAttribute == null)
                            {
                                headers[nameof(NoForwarding)] = new NoForwarding();
                            }

                            if (context.TryGetParentBehaviorContext(out var parentBehaviorContext))
                            {
                                parentBehaviorContext.Send(context.Event, headers);
                            };
                        }
                    }
                }
            }

        }

        public override async Task OnStateEntryAsync(IStateActionContext context, Func<Task> next)
        {
            await next();
            var vertex = ((StateActionContext)context).Vertex;

            var stateValues = ((IRootContext)context).Context.GetStateValues(vertex.Name);

            if (vertex.BehaviorName == null) return;
            var behaviorId = vertex.GetBehaviorId(context.Behavior.Id);
            var initializationRequest = (vertex.BehaviorInitializationBuilder != null
                ? vertex.BehaviorInitializationBuilder(context) : new Initialize()).ToTypedEventHolder();
            initializationRequest.Headers[nameof(BehaviorEmbedding)] = new BehaviorEmbedding
            {
                OwnerId = context.TryGetOwnerBehaviorContext(out var ownerBehavior) ? ownerBehavior.Id : context.Behavior.Id,
                ParentId = context.Behavior.Id
            };
            initializationRequest.Headers[nameof(ForcedReset)] = new ForcedReset();
            var root = ((StateActionContext)context).Context;
            await root.Executor.Inspector.OnHostedBehaviorInitializeAsync(
                new HostedBehaviorObservationContext(root, vertex, behaviorId, initializationRequest), () =>
                {
                    if (!context.TryLocateBehavior(behaviorId, out var behavior))
                        throw new StateDefinitionException(context.State.Name,
                            $"Embedded {vertex.BehaviorType} '{vertex.BehaviorName}' not found", context.Behavior.Id.BehaviorClass);
                    stateValues.BehaviorId = behaviorId;
                    BehaviorObservation.Dispatch(behaviorId, initializationRequest,
                        () => _ = initializationRequest.SendAsync(behavior, initializationRequest.Headers));
                    return Task.CompletedTask;
                });
        }

        public override async Task OnStateExitAsync(IStateActionContext context, Func<Task> next)
        {
            var vertex = ((StateActionContext)context).Vertex;
            if (vertex.BehaviorName != null)
            {
                var root = ((IRootContext)context).Context;
                var stateValues = root.GetStateValues(vertex.Name);
                if (stateValues.BehaviorId is BehaviorId behaviorId)
                {
                    var delivery = new Finalize().ToEventHolder();
                    await root.Executor.Inspector.OnHostedBehaviorFinalizeAsync(
                        new HostedBehaviorObservationContext(root, vertex, behaviorId, delivery), () =>
                    {
                        if (!context.TryLocateBehavior(behaviorId, out var behavior))
                            throw new StateDefinitionException(context.State.Name,
                                $"Embedded {vertex.BehaviorType} '{vertex.BehaviorName}' not found", context.Behavior.Id.BehaviorClass);
                        BehaviorObservation.Dispatch(behaviorId, delivery, () => _ = delivery.SendAsync(behavior));
                        stateValues.BehaviorId = null;
                        return Task.CompletedTask;
                    });
                }
            }
            await next();
        }
    }
}

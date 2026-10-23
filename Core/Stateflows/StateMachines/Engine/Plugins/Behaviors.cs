using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Stateflows.Common;
using Stateflows.Common.Utilities;
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

            // if (vertex.BehaviorType == "AIAgent")
            // {
            //     return;
            // }

            var stateValues = ((IRootContext)context).Context.GetStateValues(vertex.Name);

            if (vertex.BehaviorName != null)
            {
                var behaviorId = vertex.GetBehaviorId(context.Behavior.Id);

                if (context.TryLocateBehavior(behaviorId, out var behavior))
                {
                    stateValues.BehaviorId = behaviorId;

                    var initializationRequest = (
                        vertex.BehaviorInitializationBuilder != null
                            ? vertex.BehaviorInitializationBuilder(context)
                            : new Initialize()
                    ).ToTypedEventHolder();

                    _ = initializationRequest.SendAsync(
                        behavior,
                        new Dictionary<string, EventHeader>
                        {
                            {
                                nameof(BehaviorEmbedding),
                                new BehaviorEmbedding()
                                {
                                    OwnerId = context.TryGetOwnerBehaviorContext(out var ownerBehavior)
                                        ? ownerBehavior.Id
                                        : context.Behavior.Id,
                                    ParentId = context.Behavior.Id
                                }
                            },
                            {
                                nameof(ForcedReset),
                                new ForcedReset()
                            }
                        }
                    );
                }
                else
                {
                    throw new StateDefinitionException(
                        context.State.Name,
                        vertex.BehaviorType switch
                        {
                            BehaviorType.Activity => "DoActivity",
                            BehaviorType.Action => "DoAction",
                            BehaviorType.StateMachine => "Submachine",
                            _ => vertex.BehaviorType
                        } +
                        $" '{vertex.BehaviorName}' not found",
                        context.Behavior.Id.BehaviorClass
                    );
                }
            }
        }

        public override async Task OnStateExitAsync(IStateActionContext context, Func<Task> next)
        {
            Before(context);
            await next();

            void Before(IStateActionContext context)
            {
                var vertex = ((StateActionContext)context).Vertex;

                // if (vertex.BehaviorType == "AIAgent")
                // {
                //     return;
                // }

                if (vertex.BehaviorName != null)
                {
                    var stateValues = ((IRootContext)context).Context.GetStateValues(vertex.Name);

                    if (
                        stateValues.BehaviorId.HasValue &&
                        context.TryLocateBehavior(stateValues.BehaviorId.Value, out var behavior)
                    )
                    {
                        _ = behavior.SendAsync(new Finalize());
                        stateValues.BehaviorId = null;
                    }
                }
            }
        }
    }
}

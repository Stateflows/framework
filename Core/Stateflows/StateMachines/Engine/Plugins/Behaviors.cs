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
        public override void AfterProcessEvent<TEvent>(IEventContext<TEvent> context, EventStatus eventStatus)
        {
            if (context.Behavior.IsEmbedded)
            {
                if (eventStatus == EventStatus.NotConsumed)
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

                        context.Behavior.Send(context.Event, headers);
                    }
                }

                // if (eventStatus == EventStatus.Consumed)
                // {
                //     context.Behavior.Send(new Completion());
                // }
            }
        }

        public override void AfterStateEntry(IStateActionContext context)
        {
            var vertex = ((StateActionContext)context).Vertex;

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
                                    OwnerId = context.Behavior.Id,
                                    ParentId = context.Behavior.ActualId
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

        public override void BeforeStateExit(IStateActionContext context)
        {
            var vertex = ((StateActionContext)context).Vertex;

            if (vertex.BehaviorName != null)
            {
                var stateValues = ((IRootContext)context).Context.GetStateValues(vertex.Name);

                if (
                    stateValues.BehaviorId.HasValue &&
                    context.TryLocateBehavior(stateValues.BehaviorId.Value, out var behavior)
                )
                {
                    behavior.SendAsync(new Finalize());
                    stateValues.BehaviorId = null;
                }
            }
        }
    }
}

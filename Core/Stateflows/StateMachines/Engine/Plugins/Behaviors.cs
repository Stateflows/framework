using System.Linq;
using Stateflows.Common;
using Stateflows.Common.Classes;
using Stateflows.StateMachines.Exceptions;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Engine
{
    public class Behaviors : StateMachineObserver
    {
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
                    
                    var request = new CompoundRequest()
                        .Add(new SetContextOwner() { ContextOwner = context.Behavior.Id })
                        .Add(new SetGlobalValues() { Values = ((ContextValuesCollection)context.Behavior.Values).Values })
                    ;

                    if (vertex.BehaviorSubscriptions.Any())
                    {
                        request.Add(vertex.GetSubscriptionRequest(context.Behavior.Id));
                    }

                    if (vertex.BehaviorRelays.Any())
                    {
                        request.Add(vertex.GetStartRelayRequest(context.Behavior.Id));
                    }

                    var initializationRequest = vertex.BehaviorInitializationBuilder != null
                        ? vertex.BehaviorInitializationBuilder(context)
                        : new Initialize();
                    
                    request.Events.Add(initializationRequest.ToEventHolder());
                    
                    _ = behavior.SendAsync(request);
                }
                else
                {
                    throw new StateDefinitionException(
                        context.State.Name,
                        vertex.BehaviorType == BehaviorType.Activity
                            ? "DoActivity"
                            : "Submachine" +
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
                    var request = new CompoundRequest();
                    
                    if (vertex.BehaviorRelays.Any())
                    {
                        request.Add(vertex.GetStopRelayRequest(context.Behavior.Id));
                    }
                    
                    if (vertex.BehaviorSubscriptions.Any())
                    {
                        request.Add(vertex.GetUnsubscriptionRequest(context.Behavior.Id));
                    }

                    request.Add(new Finalize());
                    
                    _ = behavior.SendAsync(request);
                    stateValues.BehaviorId = null;
                }
            }
        }
    }
}

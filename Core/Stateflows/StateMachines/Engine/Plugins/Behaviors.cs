using System.Linq;
using Stateflows.Common;
using Stateflows.StateMachines.Exceptions;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Engine
{
    internal class Behaviors : StateMachinePlugin
    {
        public override void AfterStateEntry(IStateActionContext context)
        {
            var vertex = (context as StateActionContext).Vertex;

            var stateValues = (context as IRootContext).Context.GetStateValues(vertex.Name);

            if (vertex.BehaviorName != null)
            {
                var behaviorId = vertex.GetBehaviorId(context.Behavior.Id);

                if (context.TryLocateBehavior(behaviorId, out var behavior))
                {
                    stateValues.BehaviorId = behaviorId;

                    if (vertex.BehaviorSubscriptions.Any())
                    {                        
                        _ = behavior.SendAsync(vertex.GetSubscriptionRequest(context.Behavior.Id));
                    }

                    var initializationRequest = vertex.BehaviorInitializationBuilder != null
                        ? vertex.BehaviorInitializationBuilder(context)
                        : new Initialize();
                    
                    _ = behavior.SendAsync(initializationRequest);
                }
                else
                {
                    throw new StateDefinitionException(context.State.Name, $"DoActivity '{vertex.BehaviorName}' not found", context.Behavior.Id.BehaviorClass);
                }
            }
        }

        public override void BeforeStateExit(IStateActionContext context)
        {
            var vertex = (context as StateActionContext).Vertex;

            if (vertex.BehaviorName != null)
            {
                var stateValues = (context as IRootContext).Context.GetStateValues(vertex.Name);

                if (
                    stateValues.BehaviorId.HasValue &&
                    context.TryLocateBehavior(stateValues.BehaviorId.Value, out var behavior)
                )
                {
                    if (vertex.BehaviorSubscriptions.Any())
                    {
                        _ = behavior.SendAsync(vertex.GetUnsubscriptionRequest(context.Behavior.Id));
                    }

                    _ = behavior.SendAsync(new Finalize());
                    stateValues.BehaviorId = null;
                }
            }
        }
    }
}

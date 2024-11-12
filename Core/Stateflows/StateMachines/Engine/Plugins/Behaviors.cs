using System.Linq;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Exceptions;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Engine
{
    internal class Behaviors : StateMachinePlugin
    {
        public override async Task AfterStateEntryAsync(IStateActionContext context)
        {
            var vertex = (context as StateActionContext).Vertex;

            var stateValues = (context as IRootContext).Context.GetStateValues(vertex.Name);

            if (vertex.BehaviorName != null)
            {
                var behaviorId = vertex.GetBehaviorId(context.StateMachine.Id);

                if (context.TryLocateBehavior(behaviorId, out var behavior))
                {
                    stateValues.BehaviorId = behaviorId;

                    if (vertex.BehaviorSubscriptions.Any())
                    {                        
                        _ = behavior.SendAsync(vertex.GetSubscriptionRequest(context.StateMachine.Id));
                    }

                    var initializationRequest = vertex.BehaviorInitializationBuilder?.Invoke(context) != null
                        ? await vertex.BehaviorInitializationBuilder(context) ?? new Initialize()
                        : new Initialize();

                    _ = behavior.SendAsync(initializationRequest);
                }
                else
                {
                    throw new StateDefinitionException(context.CurrentState.Name, $"DoActivity '{vertex.BehaviorName}' not found", context.StateMachine.Id.StateMachineClass);
                }
            }
        }

        public override Task BeforeStateExitAsync(IStateActionContext context)
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
                        _ = behavior.SendAsync(vertex.GetUnsubscriptionRequest(context.StateMachine.Id));
                    }

                    _ = behavior.SendAsync(new Finalize());
                    stateValues.BehaviorId = null;
                }
            }

            return Task.CompletedTask;
        }
    }
}

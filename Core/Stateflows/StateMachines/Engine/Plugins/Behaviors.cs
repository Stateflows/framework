using System.Linq;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.Utils;

namespace Stateflows.StateMachines.Engine
{
    internal class Behaviors : IStateMachinePlugin
    {
        public async Task AfterStateEntryAsync(IStateActionContext context)
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

                    var initializationRequest = vertex.BehaviorInitializationBuilder != null
                        ? await vertex.BehaviorInitializationBuilder(context) ?? (new Initialize()).ToEventHolder()
                        : (new Initialize()).ToEventHolder();

                    _ = behavior.SendAsync(initializationRequest);
                }
            }
        }

        public Task BeforeStateExitAsync(IStateActionContext context)
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

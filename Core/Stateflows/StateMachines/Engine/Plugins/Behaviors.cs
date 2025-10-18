using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Common.Extensions;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Utilities;
using Stateflows.StateMachines.Exceptions;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Engine
{
    internal class Behaviors(IStateflowsValueStorage valueStorage) : StateMachinePlugin
    {
        public override void AfterProcessEvent<TEvent>(IEventContext<TEvent> context, EventStatus eventStatus)
        {
            if (context.Behavior.IsEmbedded)
            {
                if (eventStatus == EventStatus.NotConsumed)
                {
                    var headers = context.Headers.ToList();
                    
                    var noBubblingAttribute = context.Event.GetType().GetCustomAttribute<NoBubblingAttribute>();
                    if (!headers.Any(h => h is NoBubbling) && noBubblingAttribute == null)
                    {
                        var noForwardingAttribute = context.Event.GetType().GetCustomAttribute<NoForwardingAttribute>();
                        if (!headers.Any(h => h is NoForwarding) && noForwardingAttribute == null)
                        {
                            headers.Add(new NoForwarding());
                        }

                        context.Behavior.Send(context.Event, headers);
                    }
                }

                if (eventStatus == EventStatus.Consumed)
                {
                    context.Behavior.Send(new Completion());
                }
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
                    
                    var request = new CompoundRequest()
                        .Add(new Reset())
                        .Add(((BaseContext)context).Context.Context.GetContextOwnerSetter())
                    ;

                    var initializationRequest = vertex.BehaviorInitializationBuilder != null
                        ? vertex.BehaviorInitializationBuilder(context)
                        : new Initialize();
                    
                    request.Events.Add(initializationRequest.ToTypedEventHolder());
                    
                    Task.Run(() => _ = behavior.SendAsync(request));
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
                    behavior.SendAsync(new Finalize()).GetAwaiter().GetResult();
                    stateValues.BehaviorId = null;
                }
            }
        }
    }
}

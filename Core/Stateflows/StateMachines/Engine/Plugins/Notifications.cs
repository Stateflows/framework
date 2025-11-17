using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Common.Engine;
using Stateflows.Common.Interfaces;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Extensions;

namespace Stateflows.StateMachines.Engine
{
    internal class Notifications(IStateflowsValueStorage valueStorage) : StateMachinePlugin
    {
        public override void AfterStateEntry(IStateActionContext context)
        {
            var stateflowsContext = ((BaseContext)context).Context.Context;
            Trace.WriteLine($"⦗→s⦘ State Machine '{stateflowsContext.Id.Name}:{stateflowsContext.Id.Instance}': entered '{context.State.Name}'");
        }

        public override void AfterStateExit(IStateActionContext context)
        {
            var stateflowsContext = ((BaseContext)context).Context.Context;
            Trace.WriteLine($"⦗→s⦘ State Machine '{stateflowsContext.Id.Name}:{stateflowsContext.Id.Instance}': exited '{context.State.Name}'");
        }

        public override void AfterStateInitialize(IStateActionContext context)
        {
            var stateflowsContext = ((BaseContext)context).Context.Context;
            Trace.WriteLine($"⦗→s⦘ State Machine '{stateflowsContext.Id.Name}:{stateflowsContext.Id.Instance}': initialized '{context.State.Name}'");
        }

        public override void AfterStateFinalize(IStateActionContext context)
        {
            var stateflowsContext = ((BaseContext)context).Context.Context;
            Trace.WriteLine($"⦗→s⦘ State Machine '{stateflowsContext.Id.Name}:{stateflowsContext.Id.Instance}': finalized '{context.State.Name}'");
        }

        public override void AfterStateMachineInitialize(IStateMachineInitializationContext context, bool implicitInitialization, bool initialized)
        {
            var stateflowsContext = ((BaseContext)context).Context.Context;
            Trace.WriteLine($"⦗→s⦘ State Machine '{stateflowsContext.Id.Name}:{stateflowsContext.Id.Instance}': {(initialized ? "" : "not ")}initialized{(implicitInitialization ? " implicitly" : "")}");

            var executor = context.Behavior.GetExecutor();
            var notification = new BehaviorInfo()
            {
                Id = executor.Context.Id,
                BehaviorStatus = BehaviorStatus.Initialized,
                ExpectedEvents = executor.GetExpectedEventNamesAsync().GetAwaiter().GetResult()
            };

            context.Behavior.Publish(notification);
        }

        public override void AfterStateMachineFinalize(IStateMachineActionContext context)
        {
            var stateflowsContext = ((BaseContext)context).Context.Context;
            
            valueStorage.RemoveAsync(stateflowsContext.Id, CommonValues.ForceFinalizeKey).GetAwaiter().GetResult();
            
            Trace.WriteLine($"⦗→s⦘ State Machine '{stateflowsContext.Id.Name}:{stateflowsContext.Id.Instance}': finalized");

            var notification = new BehaviorInfo()
            {
                Id = context.Behavior.Id,
                BehaviorStatus = BehaviorStatus.Finalized
            };

            context.Behavior.Publish(notification);
        }

        public override void BeforeTransitionEffect<TEvent>(ITransitionContext<TEvent> context)
        {
            var stateflowsContext = ((BaseContext)context).Context.Context;
            var eventName = Event.GetName(context.Event.GetType());
            if (context.Target != null)
            {
                Trace.WriteLine(string.IsNullOrEmpty(eventName)
                    ? $"⦗→s⦘ State Machine '{stateflowsContext.Id.Name}:{stateflowsContext.Id.Instance}': default transition from '{context.Source.Name}' to '{context.Target.Name}'"
                    : $"⦗→s⦘ State Machine '{stateflowsContext.Id.Name}:{stateflowsContext.Id.Instance}': event '{eventName}' triggered transition from '{context.Source.Name}' to '{context.Target.Name}'");
            }
            else
            {
                Trace.WriteLine($"⦗→s⦘ State Machine '{stateflowsContext.Id.Name}:{stateflowsContext.Id.Instance}': event '{eventName}' triggered internal transition in '{context.Source.Name}'");
            }
        }

        public override void AfterTransitionGuard<TEvent>(ITransitionContext<TEvent> context, bool guardResult)
        {
            var stateflowsContext = ((BaseContext)context).Context.Context;
            if (guardResult)
            {
                return;
            }
            
            var eventName = Event.GetName(context.Event.GetType());

            var transitionContext = (TransitionContext<TEvent>)context;
            var guardDelegations = context.Headers.OfType<TransitionGuardDelegation>();
            if (guardDelegations.Any(g => g.EdgeIdentifier == transitionContext.Edge.Identifier))
            {
                if (context.Target != null)
                {
                    Trace.WriteLine(string.IsNullOrEmpty(eventName)
                        ? $"⦗→s⦘ State Machine '{stateflowsContext.Id.Name}:{stateflowsContext.Id.Instance}': delegated guard on default transition from '{context.Source.Name}' to '{context.Target.Name}'"
                        : $"⦗→s⦘ State Machine '{stateflowsContext.Id.Name}:{stateflowsContext.Id.Instance}': delegated guard on transition from '{context.Source.Name}' to '{context.Target.Name}' triggered by event '{eventName}'");
                }
                else
                {
                    Trace.WriteLine($"⦗→s⦘ State Machine '{stateflowsContext.Id.Name}:{stateflowsContext.Id.Instance}': delegated guard on internal transition in '{context.Source.Name}' triggered by event '{eventName}'");
                }
            }
            else
            {
                if (context.Target != null)
                {
                    Trace.WriteLine(string.IsNullOrEmpty(eventName)
                        ? $"⦗→s⦘ State Machine '{stateflowsContext.Id.Name}:{stateflowsContext.Id.Instance}': guard stopped default transition from '{context.Source.Name}' to '{context.Target.Name}'"
                        : $"⦗→s⦘ State Machine '{stateflowsContext.Id.Name}:{stateflowsContext.Id.Instance}': guard stopped event '{eventName}' from triggering transition from '{context.Source.Name}' to '{context.Target.Name}'");
                }
                else
                {
                    Trace.WriteLine($"⦗→s⦘ State Machine '{stateflowsContext.Id.Name}:{stateflowsContext.Id.Instance}': guard stopped event '{eventName}' from triggering internal transition in '{context.Source.Name}'");
                }
            }
        }

        public override bool BeforeProcessEvent<TEvent>(IEventContext<TEvent> context)
        {
            var stateflowsContext = ((BaseContext)context).Context.Context;
            Trace.WriteLine($"⦗→s⦘ State Machine '{stateflowsContext.Id.Name}:{stateflowsContext.Id.Instance}': received event '{Event.GetName(context.Event.GetType())}', processing");

            return true;
        }

        public override void AfterProcessEvent<TEvent>(IEventContext<TEvent> context, EventStatus eventStatus)
        {
            var stateflowsContext = ((BaseContext)context).Context.Context;
            Trace.WriteLine($"⦗→s⦘ State Machine '{stateflowsContext.Id.Name}:{stateflowsContext.Id.Instance}': processed event '{Event.GetName(context.Event.GetType())}' with result '{eventStatus}'");
            
            var executor = context.Behavior.GetExecutor();
            if (!executor.StateHasChanged)
            {
                return;
            }
            
            // Trace.WriteLine($"⦗→s⦘ State Machine '{stateflowsContext.Id.Name}:{stateflowsContext.Id.Instance}': state has changed, emitting");
            var notification = new StateMachineInfo()
            {
                Id = executor.Context.Id,
                BehaviorStatus = executor.BehaviorStatus,
                CurrentStates = executor.GetStatesTree(),
                ExpectedEvents = executor.GetExpectedEventNamesAsync().GetAwaiter().GetResult(),
            };

            context.Behavior.Publish(notification);
        }

        public override bool OnStateMachineInitializationException(IStateMachineInitializationContext context, Exception exception)
        {
            var stateflowsContext = ((BaseContext)context).Context.Context;
            Trace.WriteLine($"⦗→s⦘ State Machine '{stateflowsContext.Id.Name}:{stateflowsContext.Id.Instance}': unhandled exception '{exception.GetType().Name}' thrown with message '{exception.Message}' on State Machine initialization");

            return false;
        }

        public override bool OnStateMachineFinalizationException(IStateMachineActionContext context, Exception exception)
        {
            var stateflowsContext = ((BaseContext)context).Context.Context;
            Trace.WriteLine($"⦗→s⦘ State Machine '{stateflowsContext.Id.Name}:{stateflowsContext.Id.Instance}': unhandled exception '{exception.GetType().Name}' thrown with message '{exception.Message}' on State Machine finalization");

            return false;
        }

        public override bool OnTransitionGuardException<TEvent>(ITransitionContext<TEvent> context, Exception exception)
        {
            var stateflowsContext = ((BaseContext)context).Context.Context;
            Trace.WriteLine($"⦗→s⦘ State Machine '{stateflowsContext.Id.Name}:{stateflowsContext.Id.Instance}': unhandled exception '{exception.GetType().Name}' thrown with message '{exception.Message}' on guard of transition from state '{context.Source.Name}'");

            return false;
        }

        public override bool OnTransitionEffectException<TEvent>(ITransitionContext<TEvent> context, Exception exception)
        {
            var stateflowsContext = ((BaseContext)context).Context.Context;
            Trace.WriteLine($"⦗→s⦘ State Machine '{stateflowsContext.Id.Name}:{stateflowsContext.Id.Instance}': unhandled exception '{exception.GetType().Name}' thrown with message '{exception.Message}' on effect of transition from state '{context.Source.Name}'");

            return false;
        }

        public override bool OnStateInitializationException(IStateActionContext context, Exception exception)
        {
            var stateflowsContext = ((BaseContext)context).Context.Context;
            Trace.WriteLine($"⦗→s⦘ State Machine '{stateflowsContext.Id.Name}:{stateflowsContext.Id.Instance}': unhandled exception '{exception.GetType().Name}' thrown with message '{exception.Message}' on state '{context.State.Name}' initialization");

            return false;
        }

        public override bool OnStateFinalizationException(IStateActionContext context, Exception exception)
        {
            var stateflowsContext = ((BaseContext)context).Context.Context;
            Trace.WriteLine($"⦗→s⦘ State Machine '{stateflowsContext.Id.Name}:{stateflowsContext.Id.Instance}': unhandled exception '{exception.GetType().Name}' thrown with message '{exception.Message}' on state '{context.State.Name}' finalization");

            return false;
        }

        public override bool OnStateEntryException(IStateActionContext context, Exception exception)
        {
            var stateflowsContext = ((BaseContext)context).Context.Context;
            Trace.WriteLine($"⦗→s⦘ State Machine '{stateflowsContext.Id.Name}:{stateflowsContext.Id.Instance}': unhandled exception '{exception.GetType().Name}' thrown with message '{exception.Message}' on entry to state '{context.State.Name}'");

            return false;
        }

        public override bool OnStateExitException(IStateActionContext context, Exception exception)
        {
            var stateflowsContext = ((BaseContext)context).Context.Context;
            Trace.WriteLine($"⦗→s⦘ State Machine '{stateflowsContext.Id.Name}:{stateflowsContext.Id.Instance}': unhandled exception '{exception.GetType().Name}' thrown with message '{exception.Message}' on exit from state '{context.State.Name}'");

            return false;
        }
    }
}

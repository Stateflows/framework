using System;
using System.Diagnostics;
using Stateflows.Common;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Engine
{
    internal class Notifications : StateMachinePlugin
    {
        public override void AfterStateEntry(IStateActionContext context)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}:{context.StateMachine.Id.Instance}': entered '{context.CurrentState.Name}'");
        }

        public override void AfterStateExit(IStateActionContext context)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}:{context.StateMachine.Id.Instance}': exited '{context.CurrentState.Name}'");
        }

        public override void AfterStateInitialize(IStateActionContext context)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}:{context.StateMachine.Id.Instance}': initialized '{context.CurrentState.Name}'");
        }

        public override void AfterStateFinalize(IStateActionContext context)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}:{context.StateMachine.Id.Instance}': finalized '{context.CurrentState.Name}'");
        }

        public override void AfterStateMachineInitialize(IStateMachineInitializationContext context, bool initialized)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}:{context.StateMachine.Id.Instance}': {(initialized ? "" : "not ")}initialized");

            var executor = context.StateMachine.GetExecutor();
            var notification = new BehaviorInfo()
            {
                BehaviorStatus = BehaviorStatus.Initialized,
                ExpectedEvents = executor.GetExpectedEventNames()
            };

            context.StateMachine.Publish(notification);
        }

        public override void AfterStateMachineFinalize(IStateMachineActionContext context)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}:{context.StateMachine.Id.Instance}': finalized");

            var notification = new BehaviorInfo() { BehaviorStatus = BehaviorStatus.Finalized };

            context.StateMachine.Publish(notification);
        }

        public override void BeforeTransitionEffect<TEvent>(ITransitionContext<TEvent> context)
        {
            var eventName = Event.GetName(context.Event.GetType());
            if (context.Target != null)
            {
                Trace.WriteLine(string.IsNullOrEmpty(eventName)
                    ? $"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}:{context.StateMachine.Id.Instance}': default transition from '{context.Source.Name}' to '{context.Target.Name}'"
                    : $"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}:{context.StateMachine.Id.Instance}': event '{eventName}' triggered transition from '{context.Source.Name}' to '{context.Target.Name}'");
            }
            else
            {
                Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}:{context.StateMachine.Id.Instance}': event '{eventName}' triggered internal transition in '{context.Source.Name}'");
            }
            
            return;
        }

        public override void AfterTransitionGuard<TEvent>(ITransitionContext<TEvent> context, bool guardResult)
        {
            if (guardResult) return;
            var eventName = Event.GetName(context.Event.GetType());
            if (context.Target != null)
            {
                Trace.WriteLine(string.IsNullOrEmpty(eventName)
                    ? $"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}:{context.StateMachine.Id.Instance}': guard stopped default transition from '{context.Source.Name}' to '{context.Target.Name}'"
                    : $"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}:{context.StateMachine.Id.Instance}': guard stopped event '{eventName}' from triggering transition from '{context.Source.Name}' to '{context.Target.Name}'");
            }
            else
            {
                Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}:{context.StateMachine.Id.Instance}': guard stopped event '{eventName}' from triggering internal transition in '{context.Source.Name}'");
            }
        }

        public override bool BeforeProcessEvent<TEvent>(IEventContext<TEvent> context)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}:{context.StateMachine.Id.Instance}': received event '{Event.GetName(context.Event.GetType())}', trying to process it");

            return true;
        }

        public override void AfterProcessEvent<TEvent>(IEventContext<TEvent> context, EventStatus eventStatus)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}:{context.StateMachine.Id.Instance}': processed event '{Event.GetName(context.Event.GetType())}'");

            var executor = context.StateMachine.GetExecutor();
            if (!executor.StateHasChanged) return;
            var notification = new StateMachineInfo()
            {
                BehaviorStatus = executor.BehaviorStatus,
                StatesTree = executor.GetStateTree(),
                ExpectedEvents = executor.GetExpectedEventNames(),
            };

            context.StateMachine.Publish(notification);
        }

        public override bool OnStateMachineInitializationException(IStateMachineInitializationContext context, Exception exception)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}:{context.StateMachine.Id.Instance}': unhandled exception '{exception.GetType().Name}' with message '{exception.Message}' on State Machine initialization");

            return false;
        }

        public override bool OnStateMachineFinalizationException(IStateMachineActionContext context, Exception exception)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}:{context.StateMachine.Id.Instance}': unhandled exception '{exception.GetType().Name}' with message '{exception.Message}' on State Machine finalization");

            return false;
        }

        public override bool OnTransitionGuardException<TEvent>(ITransitionContext<TEvent> context, Exception exception)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}:{context.StateMachine.Id.Instance}': unhandled exception '{exception.GetType().Name}' with message '{exception.Message}' on transition guard");

            return false;
        }

        public override bool OnTransitionEffectException<TEvent>(ITransitionContext<TEvent> context, Exception exception)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}:{context.StateMachine.Id.Instance}': unhandled exception '{exception.GetType().Name}' with message '{exception.Message}' on transition effect");

            return false;
        }

        public override bool OnStateInitializationException(IStateActionContext context, Exception exception)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}:{context.StateMachine.Id.Instance}': unhandled exception '{exception.GetType().Name}' with message '{exception.Message}' on initialization");

            return false;
        }

        public override bool OnStateFinalizationException(IStateActionContext context, Exception exception)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}:{context.StateMachine.Id.Instance}': unhandled exception '{exception.GetType().Name}' with message '{exception.Message}' on finalization");

            return false;
        }

        public override bool OnStateEntryException(IStateActionContext context, Exception exception)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}:{context.StateMachine.Id.Instance}': unhandled exception '{exception.GetType().Name}' with message '{exception.Message}' on entry");

            return false;
        }

        public override bool OnStateExitException(IStateActionContext context, Exception exception)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}:{context.StateMachine.Id.Instance}': unhandled exception '{exception.GetType().Name}' with message '{exception.Message}' on exit");

            return false;
        }
    }
}

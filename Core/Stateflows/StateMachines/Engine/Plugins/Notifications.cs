using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Engine
{
    internal class Notifications : StateMachinePlugin
    {
        public override Task AfterStateEntryAsync(IStateActionContext context)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}:{context.StateMachine.Id.Instance}': entered '{context.CurrentState.Name}'");

            return Task.CompletedTask;
        }

        public override Task AfterStateExitAsync(IStateActionContext context)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}:{context.StateMachine.Id.Instance}': exited '{context.CurrentState.Name}'");

            return Task.CompletedTask;
        }

        public override Task AfterStateInitializeAsync(IStateActionContext context)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}:{context.StateMachine.Id.Instance}': initialized '{context.CurrentState.Name}'");

            return Task.CompletedTask;
        }

        public override Task AfterStateFinalizeAsync(IStateActionContext context)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}:{context.StateMachine.Id.Instance}': finalized '{context.CurrentState.Name}'");

            return Task.CompletedTask;
        }

        public override Task AfterStateMachineInitializeAsync(IStateMachineInitializationContext context, bool initialized)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}:{context.StateMachine.Id.Instance}': {(initialized ? "" : "not ")}initialized");

            var executor = context.StateMachine.GetExecutor();
            var notification = new BehaviorInfo()
            {
                BehaviorStatus = BehaviorStatus.Initialized,
                ExpectedEvents = executor.GetExpectedEventNames()
            };

            context.StateMachine.Publish(notification);

            return Task.CompletedTask;
        }

        public override Task AfterStateMachineFinalizeAsync(IStateMachineActionContext context)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}:{context.StateMachine.Id.Instance}': finalized");

            var notification = new BehaviorInfo() { BehaviorStatus = BehaviorStatus.Finalized };

            context.StateMachine.Publish(notification);

            return Task.CompletedTask;
        }

        public override Task BeforeTransitionEffectAsync<TEvent>(ITransitionContext<TEvent> context)
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
            
            return Task.CompletedTask;
        }

        public override Task AfterTransitionGuardAsync<TEvent>(ITransitionContext<TEvent> context, bool guardResult)
        {
            if (guardResult) return Task.CompletedTask;
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

            return Task.CompletedTask;
        }

        public override Task<bool> BeforeProcessEventAsync<TEvent>(IEventActionContext<TEvent> context)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}:{context.StateMachine.Id.Instance}': received event '{Event.GetName(context.Event.GetType())}', trying to process it");

            return Task.FromResult(true);
        }

        public override Task AfterProcessEventAsync<TEvent>(IEventActionContext<TEvent> context)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}:{context.StateMachine.Id.Instance}': processed event '{Event.GetName(context.Event.GetType())}'");

            var executor = context.StateMachine.GetExecutor();
            if (!executor.StateHasChanged) return Task.CompletedTask;
            var notification = new StateMachineInfo()
            {
                BehaviorStatus = executor.BehaviorStatus,
                StatesTree = executor.GetStateTree(),
                ExpectedEvents = executor.GetExpectedEventNames(),
            };

            context.StateMachine.Publish(notification);

            return Task.CompletedTask;
        }

        public override Task<bool> OnStateMachineInitializationExceptionAsync(IStateMachineInitializationContext context, Exception exception)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}:{context.StateMachine.Id.Instance}': unhandled exception '{exception.GetType().Name}' with message '{exception.Message}' on State Machine initialization");

            return Task.FromResult(false);
        }

        public override Task<bool> OnStateMachineFinalizationExceptionAsync(IStateMachineActionContext context, Exception exception)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}:{context.StateMachine.Id.Instance}': unhandled exception '{exception.GetType().Name}' with message '{exception.Message}' on State Machine finalization");

            return Task.FromResult(false);
        }

        public override Task<bool> OnTransitionGuardExceptionAsync<TEvent>(ITransitionContext<TEvent> context, Exception exception)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}:{context.StateMachine.Id.Instance}': unhandled exception '{exception.GetType().Name}' with message '{exception.Message}' on transition guard");

            return Task.FromResult(false);
        }

        public override Task<bool> OnTransitionEffectExceptionAsync<TEvent>(ITransitionContext<TEvent> context, Exception exception)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}:{context.StateMachine.Id.Instance}': unhandled exception '{exception.GetType().Name}' with message '{exception.Message}' on transition effect");

            return Task.FromResult(false);
        }

        public override Task<bool> OnStateInitializationExceptionAsync(IStateActionContext context, Exception exception)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}:{context.StateMachine.Id.Instance}': unhandled exception '{exception.GetType().Name}' with message '{exception.Message}' on initialization");

            return Task.FromResult(false);
        }

        public override Task<bool> OnStateFinalizationExceptionAsync(IStateActionContext context, Exception exception)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}:{context.StateMachine.Id.Instance}': unhandled exception '{exception.GetType().Name}' with message '{exception.Message}' on finalization");

            return Task.FromResult(false);
        }

        public override Task<bool> OnStateEntryExceptionAsync(IStateActionContext context, Exception exception)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}:{context.StateMachine.Id.Instance}': unhandled exception '{exception.GetType().Name}' with message '{exception.Message}' on entry");

            return Task.FromResult(false);
        }

        public override Task<bool> OnStateExitExceptionAsync(IStateActionContext context, Exception exception)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}:{context.StateMachine.Id.Instance}': unhandled exception '{exception.GetType().Name}' with message '{exception.Message}' on exit");

            return Task.FromResult(false);
        }
    }
}

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Engine
{
    internal class Notifications : IStateMachinePlugin
    {
        public Task AfterStateEntryAsync(IStateActionContext context)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}:{context.StateMachine.Id.Instance}': entered state '{context.CurrentState.Name}'");

            return Task.CompletedTask;
        }

        public Task AfterStateExitAsync(IStateActionContext context)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}:{context.StateMachine.Id.Instance}': exited state '{context.CurrentState.Name}'");

            return Task.CompletedTask;
        }

        public Task AfterStateInitializeAsync(IStateActionContext context)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}:{context.StateMachine.Id.Instance}': initialized state '{context.CurrentState.Name}'");

            return Task.CompletedTask;
        }

        public Task AfterStateFinalizeAsync(IStateActionContext context)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}:{context.StateMachine.Id.Instance}': finalized state '{context.CurrentState.Name}'");

            return Task.CompletedTask;
        }

        public Task AfterStateMachineInitializeAsync(IStateMachineInitializationContext context, bool initialized)
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

        public Task AfterStateMachineFinalizeAsync(IStateMachineActionContext context)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}:{context.StateMachine.Id.Instance}': finalized");

            var notification = new BehaviorInfo() { BehaviorStatus = BehaviorStatus.Finalized };

            context.StateMachine.Publish(notification);

            return Task.CompletedTask;
        }

        public Task AfterTransitionEffectAsync<TEvent>(ITransitionContext<TEvent> context)
            => Task.CompletedTask;

        public Task AfterTransitionGuardAsync<TEvent>(ITransitionContext<TEvent> context, bool guardResult)
        {
            var eventName = EventInfo.GetName(context.Event.GetType());
            if (guardResult)
            {
                if (context.TargetState != null)
                {
                    if (string.IsNullOrEmpty(eventName))
                    {
                        Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}:{context.StateMachine.Id.Instance}': default transition from state '{context.SourceState.Name}' to state '{context.TargetState.Name}'");
                    }
                    else
                    {
                        Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}:{context.StateMachine.Id.Instance}': event '{eventName}' triggered transition from state '{context.SourceState.Name}' to state '{context.TargetState.Name}'");
                    }
                }
                else
                {
                    Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}:{context.StateMachine.Id.Instance}': event '{eventName}' triggered internal transition in state '{context.SourceState.Name}'");
                }
            }
            else
            {
                if (context.TargetState != null)
                {
                    if (string.IsNullOrEmpty(eventName))
                    {
                        Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}:{context.StateMachine.Id.Instance}': guard stopped default transition from state '{context.SourceState.Name}' to state '{context.TargetState.Name}'");
                    }
                    else
                    {
                        Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}:{context.StateMachine.Id.Instance}': guard stopped event '{eventName}' from triggering transition from state '{context.SourceState.Name}' to state '{context.TargetState.Name}'");
                    }
                }
                else
                {
                    Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}:{context.StateMachine.Id.Instance}': guard stopped event '{eventName}' from triggering internal transition in state '{context.SourceState.Name}'");
                }
            }

            return Task.CompletedTask;
        }

        public Task<bool> BeforeProcessEventAsync<TEvent>(IEventActionContext<TEvent> context)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}:{context.StateMachine.Id.Instance}': received event '{Event.GetName(context.Event.GetType())}', trying to process it");

            return Task.FromResult(true);
        }

        public Task AfterProcessEventAsync<TEvent>(IEventActionContext<TEvent> context)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}:{context.StateMachine.Id.Instance}': processed event '{Event.GetName(context.Event.GetType())}'");

            var executor = context.StateMachine.GetExecutor();
            if (executor.StateHasChanged)
            {
                var notification = new CurrentState()
                {
                    BehaviorStatus = executor.BehaviorStatus,
                    StatesStack = executor.GetStateStack(),
                    ExpectedEvents = executor.GetExpectedEventNames(),
                };

                context.StateMachine.Publish(notification);
            }

            return Task.CompletedTask;
        }

        public Task<bool> OnStateMachineInitializationExceptionAsync(IStateMachineInitializationContext context, Exception exception)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}:{context.StateMachine.Id.Instance}': unhandled exception '{exception.GetType().Name}' with message '{exception.Message}' on State Machine initialization");

            return Task.FromResult(false);
        }

        public Task<bool> OnStateMachineFinalizationExceptionAsync(IStateMachineActionContext context, Exception exception)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}:{context.StateMachine.Id.Instance}': unhandled exception '{exception.GetType().Name}' with message '{exception.Message}' on State Machine finalization");

            return Task.FromResult(false);
        }

        public Task<bool> OnTransitionGuardExceptionAsync<TEvent>(ITransitionContext<TEvent> context, Exception exception)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}:{context.StateMachine.Id.Instance}': unhandled exception '{exception.GetType().Name}' with message '{exception.Message}' on transition guard");

            return Task.FromResult(false);
        }

        public Task<bool> OnTransitionEffectExceptionAsync<TEvent>(ITransitionContext<TEvent> context, Exception exception)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}:{context.StateMachine.Id.Instance}': unhandled exception '{exception.GetType().Name}' with message '{exception.Message}' on transition effect");

            return Task.FromResult(false);
        }

        public Task<bool> OnStateInitializationExceptionAsync(IStateActionContext context, Exception exception)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}:{context.StateMachine.Id.Instance}': unhandled exception '{exception.GetType().Name}' with message '{exception.Message}' on state initialization");

            return Task.FromResult(false);
        }

        public Task<bool> OnStateFinalizationExceptionAsync(IStateActionContext context, Exception exception)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}:{context.StateMachine.Id.Instance}': unhandled exception '{exception.GetType().Name}' with message '{exception.Message}' on state finalization");

            return Task.FromResult(false);
        }

        public Task<bool> OnStateEntryExceptionAsync(IStateActionContext context, Exception exception)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}:{context.StateMachine.Id.Instance}': unhandled exception '{exception.GetType().Name}' with message '{exception.Message}' on state entry");

            return Task.FromResult(false);
        }

        public Task<bool> OnStateExitExceptionAsync(IStateActionContext context, Exception exception)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}:{context.StateMachine.Id.Instance}': unhandled exception '{exception.GetType().Name}' with message '{exception.Message}' on state exit");

            return Task.FromResult(false);
        }
    }
}

﻿using System.Diagnostics;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Context.Interfaces;
using System;

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
            var notification = new BehaviorStatusNotification()
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

            var notification = new BehaviorStatusNotification() { BehaviorStatus = BehaviorStatus.Finalized };

            context.StateMachine.Publish(notification);

            return Task.CompletedTask;
        }

        public Task AfterTransitionEffectAsync(ITransitionContext<Event> context)
            => Task.CompletedTask;

        public Task AfterTransitionGuardAsync(IGuardContext<Event> context, bool guardResult)
        {
            if (guardResult)
            {
                if (context.TargetState != null)
                {
                    Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}:{context.StateMachine.Id.Instance}': event '{context.Event.Name}' triggered transition from state '{context.SourceState.Name}' to state '{context.TargetState.Name}'");
                }
                else
                {
                    Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}:{context.StateMachine.Id.Instance}': event '{context.Event.Name}' triggered reaction in state '{context.SourceState.Name}'");
                }
            }
            else
            {
                if (context.TargetState != null)
                {
                    Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}:{context.StateMachine.Id.Instance}': guard stopped event '{context.Event.Name}' from triggering transition from state '{context.SourceState.Name}' to state '{context.TargetState.Name}'");
                }
                else
                {
                    Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}:{context.StateMachine.Id.Instance}': guard stopped event '{context.Event.Name}' from triggering reaction in state '{context.SourceState.Name}'");
                }
            }

            return Task.CompletedTask;
        }

        public Task<bool> BeforeProcessEventAsync(IEventActionContext<Event> context)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}:{context.StateMachine.Id.Instance}': received event '{context.Event.Name}', trying to process it");

            return Task.FromResult(true);
        }

        public Task AfterProcessEventAsync(IEventActionContext<Event> context)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}:{context.StateMachine.Id.Instance}': processed event '{context.Event.Name}'");

            var executor = context.StateMachine.GetExecutor();
            if (executor.StateHasChanged)
            {
                var notification = new CurrentStateNotification()
                {
                    BehaviorStatus = executor.BehaviorStatus,
                    StatesStack = executor.GetStateStack(),
                    ExpectedEvents = executor.GetExpectedEventNames(),
                };

                context.StateMachine.Publish(notification);
            }

            return Task.CompletedTask;
        }

        public Task OnStateMachineInitializationExceptionAsync(IStateMachineInitializationContext context, Exception exception)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}:{context.StateMachine.Id.Instance}': unhandled exception '{exception.GetType().Name}' with message '{exception.Message}' on State Machine initialization");

            return Task.CompletedTask;
        }

        public Task OnStateMachineFinalizationExceptionAsync(IStateMachineActionContext context, Exception exception)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}:{context.StateMachine.Id.Instance}': unhandled exception '{exception.GetType().Name}' with message '{exception.Message}' on State Machine finalization");

            return Task.CompletedTask;
        }

        public Task OnTransitionGuardExceptionAsync(IGuardContext<Event> context, Exception exception)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}:{context.StateMachine.Id.Instance}': unhandled exception '{exception.GetType().Name}' with message '{exception.Message}' on transition guard");

            return Task.CompletedTask;
        }

        public Task OnTransitionEffectExceptionAsync(ITransitionContext<Event> context, Exception exception)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}:{context.StateMachine.Id.Instance}': unhandled exception '{exception.GetType().Name}' with message '{exception.Message}' on transition effect");

            return Task.CompletedTask;
        }

        public Task OnStateInitializationExceptionAsync(IStateActionContext context, Exception exception)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}:{context.StateMachine.Id.Instance}': unhandled exception '{exception.GetType().Name}' with message '{exception.Message}' on state initialization");

            return Task.CompletedTask;
        }

        public Task OnStateFinalizationExceptionAsync(IStateActionContext context, Exception exception)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}:{context.StateMachine.Id.Instance}': unhandled exception '{exception.GetType().Name}' with message '{exception.Message}' on state finalization");

            return Task.CompletedTask;
        }

        public Task OnStateEntryExceptionAsync(IStateActionContext context, Exception exception)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}:{context.StateMachine.Id.Instance}': unhandled exception '{exception.GetType().Name}' with message '{exception.Message}' on state entry");

            return Task.CompletedTask;
        }

        public Task OnStateExitExceptionAsync(IStateActionContext context, Exception exception)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}:{context.StateMachine.Id.Instance}': unhandled exception '{exception.GetType().Name}' with message '{exception.Message}' on state exit");

            return Task.CompletedTask;
        }
    }
}

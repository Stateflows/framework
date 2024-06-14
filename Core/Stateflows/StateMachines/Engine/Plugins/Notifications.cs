using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Context.Interfaces;
using System.Diagnostics;

namespace Stateflows.StateMachines.Engine
{
    internal class Notifications : IStateMachinePlugin
    {
        public Task AfterStateEntryAsync(IStateActionContext context)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}': entered state '{context.CurrentState.Name}'");

            return Task.CompletedTask;
        }

        public Task AfterStateExitAsync(IStateActionContext context)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}': exited state '{context.CurrentState.Name}'");

            return Task.CompletedTask;
        }

        public Task AfterStateInitializeAsync(IStateActionContext context)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}': initialized state '{context.CurrentState.Name}'");

            return Task.CompletedTask;
        }

        public Task AfterStateFinalizeAsync(IStateActionContext context)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}': finalized state '{context.CurrentState.Name}'");

            return Task.CompletedTask;
        }

        public Task AfterStateMachineInitializeAsync(IStateMachineInitializationContext context)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}': initialized");

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
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}': finalized");

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
                    Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}': event '{context.Event.Name}' triggered transition from state '{context.SourceState.Name}' to state '{context.TargetState.Name}'");
                }
                else
                {
                    Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}': event '{context.Event.Name}' triggered reaction in state '{context.SourceState.Name}'");
                }
            }
            else
            {
                if (context.TargetState != null)
                {
                    Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}': guard stopped event '{context.Event.Name}' from triggering transition from state '{context.SourceState.Name}' to state '{context.TargetState.Name}'");
                }
                else
                {
                    Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}': guard stopped event '{context.Event.Name}' from triggering reaction in state '{context.SourceState.Name}'");
                }
            }

            return Task.CompletedTask;
        }

        public Task<bool> BeforeProcessEventAsync(IEventContext<Event> context)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}': received event '{context.Event.Name}', trying to process it");

            return Task.FromResult(true);
        }

        public Task AfterProcessEventAsync(IEventContext<Event> context)
        {
            Trace.WriteLine($"⦗→s⦘ State Machine '{context.StateMachine.Id.Name}': processed event '{context.Event.Name}'");

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
    }
}

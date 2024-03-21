using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Stateflows.Common;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Events;

namespace Stateflows.StateMachines.Engine
{
    internal class Notifications : IStateMachinePlugin
    {
        private Vertex[] PreviousStack = new Vertex[0];

        public Task AfterStateEntryAsync(IStateActionContext context)
            => Task.CompletedTask;

        public Task AfterStateExitAsync(IStateActionContext context)
            => Task.CompletedTask;

        public Task AfterStateInitializeAsync(IStateActionContext context)
            => Task.CompletedTask;

        public Task AfterStateFinalizeAsync(IStateActionContext context)
            => Task.CompletedTask;

        public Task AfterStateMachineInitializeAsync(IStateMachineInitializationContext context)
        {
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
            var notification = new BehaviorStatusNotification() { BehaviorStatus = BehaviorStatus.Finalized };

            context.StateMachine.Publish(notification);

            return Task.CompletedTask;
        }

        public Task AfterTransitionEffectAsync(ITransitionContext<Event> context)
            => Task.CompletedTask;

        public Task AfterTransitionGuardAsync(IGuardContext<Event> context, bool guardResult)
            => Task.CompletedTask;

        public Task<bool> BeforeProcessEventAsync(IEventContext<Event> context)
        {
            PreviousStack = context.StateMachine.GetExecutor().VerticesStack.ToArray();

            return Task.FromResult(true);
        }

        public Task AfterProcessEventAsync(IEventContext<Event> context)
        {
            var executor = context.StateMachine.GetExecutor();
            var currentStack = executor.VerticesStack.ToArray();

            if (currentStack.Except(PreviousStack).Any())
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

        public Task BeforeStateEntryAsync(IStateActionContext context)
            => Task.CompletedTask;

        public Task BeforeStateExitAsync(IStateActionContext context)
            => Task.CompletedTask;

        public Task BeforeStateInitializeAsync(IStateActionContext context)
            => Task.CompletedTask;

        public Task BeforeStateFinalizeAsync(IStateActionContext context)
            => Task.CompletedTask;

        public Task BeforeStateMachineInitializeAsync(IStateMachineInitializationContext context)
            => Task.CompletedTask;

        public Task BeforeStateMachineFinalizeAsync(IStateMachineActionContext context)
            => Task.CompletedTask;

        public Task BeforeTransitionEffectAsync(ITransitionContext<Event> context)
            => Task.CompletedTask;

        public Task BeforeTransitionGuardAsync(IGuardContext<Event> context)
            => Task.CompletedTask;

        public Task AfterHydrateAsync(IStateMachineActionContext context)
            => Task.CompletedTask;

        public Task BeforeDehydrateAsync(IStateMachineActionContext context)
            => Task.CompletedTask;

        public Task OnStateMachineInitializationExceptionAsync(IStateMachineInitializationContext context, Exception exception)
            => Task.CompletedTask;

        public Task OnStateMachineFinalizationExceptionAsync(IStateMachineActionContext context, Exception exception)
            => Task.CompletedTask;

        public Task OnTransitionGuardExceptionAsync(IGuardContext<Event> context, Exception exception)
            => Task.CompletedTask;

        public Task OnTransitionEffectExceptionAsync(ITransitionContext<Event> context, Exception exception)
            => Task.CompletedTask;

        public Task OnStateInitializationExceptionAsync(IStateActionContext context, Exception exception)
            => Task.CompletedTask;

        public Task OnStateFinalizationExceptionAsync(IStateActionContext context, Exception exception)
            => Task.CompletedTask;

        public Task OnStateEntryExceptionAsync(IStateActionContext context, Exception exception)
            => Task.CompletedTask;

        public Task OnStateExitExceptionAsync(IStateActionContext context, Exception exception)
            => Task.CompletedTask;
    }
}

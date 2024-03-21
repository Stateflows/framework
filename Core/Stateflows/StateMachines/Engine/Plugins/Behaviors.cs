using System;
using System.Linq;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Engine
{
    internal class Behaviors : IStateMachinePlugin
    {
        public Task AfterStateEntryAsync(IStateActionContext context)
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
                        var subscriptionRequest = new SubscriptionRequest()
                        {
                            BehaviorId = context.StateMachine.Id,
                            NotificationNames = vertex.BehaviorSubscriptions
                                .Select(t => EventInfo.GetName(t))
                                .ToList()
                        };

                        _ = behavior.SendAsync(subscriptionRequest);
                    }

                    var initializationRequest = vertex.BehaviorInitializationBuilder?.Invoke(context) ?? new InitializationRequest();
                    _ = behavior.InitializeAsync(initializationRequest);
                }
            }

            return Task.CompletedTask;
        }

        public Task AfterStateExitAsync(IStateActionContext context)
            => Task.CompletedTask;

        public Task AfterStateInitializeAsync(IStateActionContext context)
            => Task.CompletedTask;

        public Task AfterStateFinalizeAsync(IStateActionContext context)
            => Task.CompletedTask;

        public Task AfterStateMachineInitializeAsync(IStateMachineInitializationContext context)
            => Task.CompletedTask;

        public Task AfterStateMachineFinalizeAsync(IStateMachineActionContext context)
            => Task.CompletedTask;

        public Task AfterTransitionEffectAsync(ITransitionContext<Event> context)
            => Task.CompletedTask;

        public Task AfterTransitionGuardAsync(IGuardContext<Event> context, bool guardResult)
            => Task.CompletedTask;

        public Task<bool> BeforeProcessEventAsync(IEventContext<Event> context)
            => Task.FromResult(true);

        public Task AfterProcessEventAsync(IEventContext<Event> context)
            => Task.CompletedTask;

        public Task BeforeStateEntryAsync(IStateActionContext context)
            => Task.CompletedTask;

        public async Task BeforeStateExitAsync(IStateActionContext context)
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
                    await behavior.SendAsync(new FinalizationRequest());
                    stateValues.BehaviorId = null;
                }
            }
        }

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

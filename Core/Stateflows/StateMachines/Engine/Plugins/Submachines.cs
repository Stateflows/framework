using System.Linq;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Common.Events;
using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Engine
{
    internal class Submachines : IStateMachinePlugin
    {
        private StateMachineId GetSubmachineId(string submachineName, StateMachineId hostId, string stateName)
            => new StateMachineId(submachineName, $"__submachine:{hostId.Name}:{hostId.Instance}:{stateName}");

        public async Task AfterStateEntryAsync(IStateActionContext context)
        {
            var vertex = (context as StateActionContext).Vertex;

            var stateValues = (context as IRootContext).Context.GetStateValues(vertex.Name);

            if (vertex.SubmachineName != null)
            {
                var submachineId = GetSubmachineId(
                    vertex.SubmachineName,
                    context.StateMachine.Id,
                    context.CurrentState.Name
                );

                if (context.TryLocateStateMachine(submachineId, out var stateMachine))
                {
                    stateValues.SubmachineId = submachineId;
                    var initializationRequest = vertex.SubmachineInitializationBuilder?.Invoke(context) ?? new InitializationRequest();
                    await stateMachine.InitializeAsync(initializationRequest);
                }
            }
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

        private CurrentStateResponse SubmachineState = null;

        public async Task<bool> BeforeProcessEventAsync(IEventContext<Event> context)
        {
            var rootContext = (context as IRootContext).Context;
            var stateName = rootContext.StatesStack.LastOrDefault();
            if (
                stateName != null &&
                rootContext.Executor.Graph.AllVertices.TryGetValue(stateName, out var vertex) &&
                vertex.SubmachineName != null
            )
            {
                var stateValues = rootContext.GetStateValues(stateName);

                if (
                    stateValues.SubmachineId.HasValue &&
                    context.TryLocateStateMachine(stateValues.SubmachineId.Value, out var stateMachine)
                )
                {
                    var consumed = false;
                    if (context.Event.Name == EventInfo<CurrentStateRequest>.Name)
                    {
                        var @event = new CurrentStateRequest();
                        var result = await stateMachine.SendAsync(@event);
                        if (result.Status == EventStatus.Consumed && @event.Response != null)
                        {
                            SubmachineState = @event.Response;
                        }
                    }
                    else
                    {
                        var result = await stateMachine.SendAsync(context.Event);
                        consumed = result.Status == EventStatus.Consumed;
                    }

                    (context as IRootContext).Context.ForceConsumed = consumed;

                    return !consumed;
                }
            }

            return true;
        }

        public Task AfterProcessEventAsync(IEventContext<Event> context)
        {
            if (context.Event.Name == EventInfo<CurrentStateRequest>.Name && SubmachineState != null)
            {
                var currentState = (context.Event as CurrentStateRequest).Response;
                currentState.StatesStack = currentState.StatesStack.Concat(SubmachineState.StatesStack);
                currentState.ExpectedEvents = currentState.ExpectedEvents.Concat(SubmachineState.ExpectedEvents);
                SubmachineState = null;
            }

            return Task.CompletedTask;
        }

        public Task BeforeStateEntryAsync(IStateActionContext context)
            => Task.CompletedTask;

        public async Task BeforeStateExitAsync(IStateActionContext context)
        {
            var vertex = (context as StateActionContext).Vertex;

            if (vertex.SubmachineName != null)
            {
                var stateValues = (context as IRootContext).Context.GetStateValues(vertex.Name);

                if (
                    stateValues.SubmachineId.HasValue &&
                    context.TryLocateStateMachine(stateValues.SubmachineId.Value, out var stateMachine)
                )
                {
                    await stateMachine.SendAsync(new Exit());
                    stateValues.SubmachineId = null;
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
    }
}

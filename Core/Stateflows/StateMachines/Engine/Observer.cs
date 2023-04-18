using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Stateflows.Common;
using Stateflows.Common.Classes;
using Stateflows.Common.Interfaces;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Engine
{
    internal class Observer : IStateMachineObserver, IStateMachineInterceptor
    {
        private ITimeService TimeService { get; }

        private List<Vertex> Time_EnteredStates = new List<Vertex>();

        private List<Vertex> Time_ExitedStates = new List<Vertex>();

        private List<Vertex> Context_ExitedStates = new List<Vertex>();

        private Edge ConsumedInTransition { get; set; }

        private RootContext Context { get; set; }

        public Observer(ITimeService timeService)
        {
            TimeService = timeService;
        }

        public Task AfterHydrateAsync(IStateMachineActionContext context)
        {
            return Task.CompletedTask;
        }

        public async Task AfterProcessEventAsync(IEventContext<Event> context)
        {
            Context = (context as BaseContext).Context;

            await ClearTimeTokens(Time_ExitedStates);

            await RegisterTimeTokens(Time_EnteredStates);

            if (
                Time_ExitedStates.Count == 0 &&
                Time_EnteredStates.Count == 0 &&
                context.Event.GetType().IsSubclassOf(typeof(IntervalEvent)) &&
                ConsumedInTransition != null
            )
            {
                var timeEventIds = Context.GetStateValues(ConsumedInTransition.Source.Name).TimeEventIds;

                var token = new TimeToken()
                {
                    TargetId = context.StateMachine.Id.BehaviorId,
                    Event = Activator.CreateInstance(ConsumedInTransition.TriggerType) as TimeEvent,
                    CreatedAt = DateTime.Now
                };

                await TimeService.Register(new TimeToken[] { token });

                timeEventIds.Add(token.Id);
            }
        }

        public Task AfterStateEntryAsync(IStateActionContext context)
        {
            Time_EnteredStates.Add((context as StateActionContext).Vertex);

            return Task.CompletedTask;
        }

        public Task AfterStateExitAsync(IStateActionContext context)
        {
            var vertex = (context as StateActionContext).Vertex;
            Time_ExitedStates.Add(vertex);
            if (Time_EnteredStates.Contains(vertex))
            {
                Time_EnteredStates.Remove(vertex);
            }

            Context_ExitedStates.Add(vertex);

            return Task.CompletedTask;
        }

        public Task AfterStateInitializeAsync(IStateActionContext context)
        {
            return Task.CompletedTask;
        }

        public Task AfterStateMachineInitializeAsync(IStateMachineActionContext context)
        {
            return Task.CompletedTask;
        }

        public Task AfterTransitionEffectAsync(ITransitionContext<Event> context)
        {
            if (context.TargetState != null)
            {
                var ctx = (context as IRootContext).Context;
                foreach (var vertex in Context_ExitedStates)
                {
                    if (vertex.Name != context.TargetState.Name)
                    {
                        ctx.GetStateValues(vertex.Name).Values.Clear();
                    }
                }
                Context_ExitedStates.Clear();
            }

            return Task.CompletedTask;
        }

        public Task AfterTransitionGuardAsync(IGuardContext<Event> context, bool guardResult)
        {
            return Task.CompletedTask;
        }

        public Task BeforeDehydrateAsync(IStateMachineActionContext context)
        {
            return Task.CompletedTask;
        }

        public Task<bool> BeforeProcessEventAsync(IEventContext<Event> context)
        {
            return Task.FromResult(true);
        }

        public Task BeforeStateEntryAsync(IStateActionContext context)
        {
            return Task.CompletedTask;
        }

        public Task BeforeStateExitAsync(IStateActionContext context)
        {
            return Task.CompletedTask;
        }

        public Task BeforeStateInitializeAsync(IStateActionContext context)
        {
            return Task.CompletedTask;
        }

        public Task BeforeStateMachineInitializeAsync(IStateMachineActionContext context)
        {
            return Task.CompletedTask;
        }

        public Task BeforeTransitionEffectAsync(ITransitionContext<Event> context)
        {
            return Task.CompletedTask;
        }

        public Task BeforeTransitionGuardAsync(IGuardContext<Event> context)
        {
            ConsumedInTransition = (context as IEdgeContext).Edge;

            return Task.CompletedTask;
        }

        private async Task RegisterTimeTokens(List<Vertex> enteredVertices)
        {
            foreach (var currentVertex in enteredVertices)
            {
                var timeEventIds = Context.GetStateValues(currentVertex.Name).TimeEventIds;

                var tokens = currentVertex.Edges
                    .Where(edge => edge.TriggerType.IsSubclassOf(typeof(TimeEvent)))
                    .Select(edge => new TimeToken()
                    {
                        TargetId = Context.Id.BehaviorId,
                        Event = Activator.CreateInstance(edge.TriggerType) as TimeEvent,
                        CreatedAt = DateTime.Now
                    })
                    .ToArray();

                await TimeService.Register(tokens);

                timeEventIds.AddRange(tokens.Select(token => token.Id));
            }
        }

        private async Task ClearTimeTokens(List<Vertex> exitedVertices)
        {
            foreach (var exitedVertex in exitedVertices)
            {
                await TimeService.Clear(Context.Context.Id, Context.GetStateValues(exitedVertex.Name).TimeEventIds);
                Context.GetStateValues(exitedVertex.Name).TimeEventIds.Clear();
            }
        }
    }
}

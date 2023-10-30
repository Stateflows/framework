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
    internal class TimeEvents : IStateMachinePlugin
    {
        private IStateflowsScheduler Scheduler { get; }

        private readonly List<Vertex> Time_EnteredStates = new List<Vertex>();

        private readonly List<Vertex> Time_ExitedStates = new List<Vertex>();

        private readonly List<Vertex> Context_ExitedStates = new List<Vertex>();

        private Edge ConsumedInTransition { get; set; }

        private RootContext Context { get; set; }

        public TimeEvents(IStateflowsScheduler scheduler)
        {
            Scheduler = scheduler;
        }

        public Task AfterStateEntryAsync(IStateActionContext context)
        {
            var vertex = (context as StateActionContext).Vertex;

            Time_EnteredStates.Add(vertex);

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
            => Task.CompletedTask;

        public Task AfterStateFinalizeAsync(IStateActionContext context)
            => Task.CompletedTask;

        public Task AfterStateMachineInitializeAsync(IStateMachineInitializationContext context)
            => Task.CompletedTask;

        public Task AfterStateMachineFinalizeAsync(IStateMachineActionContext context)
            => Task.CompletedTask;

        public Task AfterTransitionEffectAsync(ITransitionContext<Event> context)
        {
            if (context.TargetState != null)
            {
                var ctx = (context as IRootContext).Context;
                foreach (var vertexName in Context_ExitedStates.Select(v => v.Name))
                {
                    if (vertexName != context.TargetState.Name)
                    {
                        ctx.GetStateValues(vertexName).Values.Clear();
                    }
                }
                Context_ExitedStates.Clear();
            }

            return Task.CompletedTask;
        }

        public Task AfterTransitionGuardAsync(IGuardContext<Event> context, bool guardResult)
            => Task.CompletedTask;

        public Task<bool> BeforeProcessEventAsync(IEventContext<Event> context)
            => Task.FromResult(true);

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

                await Scheduler.Register(new TimeToken[] { token });

                timeEventIds.Add(token.Id);
            }
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
        {
            ConsumedInTransition = (context as IEdgeContext).Edge;

            return Task.CompletedTask;
        }

        public Task AfterHydrateAsync(IStateMachineActionContext context)
            => Task.CompletedTask;

        public Task BeforeDehydrateAsync(IStateMachineActionContext context)
            => Task.CompletedTask;

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

                await Scheduler.Register(tokens);

                timeEventIds.AddRange(tokens.Select(token => token.Id));
            }
        }

        private async Task ClearTimeTokens(List<Vertex> exitedVertices)
        {
            foreach (var exitedVertexName in exitedVertices.Select(v => v.Name))
            {
                await Scheduler.Clear(Context.Context.Id, Context.GetStateValues(exitedVertexName).TimeEventIds);
                Context.GetStateValues(exitedVertexName).TimeEventIds.Clear();
            }
        }
    }
}

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Stateflows.Common;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Engine
{
    internal class TimeEvents : IStateMachinePlugin
    {
        private readonly List<Vertex> EnteredStates = new List<Vertex>();

        private readonly List<Guid> TimeEventIdsToClear = new List<Guid>();

        private Edge ConsumedInTransition { get; set; }

        private RootContext Context { get; set; }

        public Task AfterStateEntryAsync(IStateActionContext context)
        {
            var vertex = (context as StateActionContext).Vertex;
            EnteredStates.Add(vertex);

            return Task.CompletedTask;
        }

        public Task AfterStateExitAsync(IStateActionContext context)
        {
            var vertex = (context as StateActionContext).Vertex;
            var stateValues = Context.GetStateValues(vertex.Name);
            var timeEventIds = stateValues.TimeEventIds.Values.ToArray();
            TimeEventIdsToClear.AddRange(timeEventIds);

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
            => Task.CompletedTask;

        public Task AfterTransitionGuardAsync(IGuardContext<Event> context, bool guardResult)
            => Task.CompletedTask;

        public Task<bool> BeforeProcessEventAsync(IEventContext<Event> context)
        {
            var result = true;

            Context = (context as BaseContext).Context;

            if (context.Event is ResetRequest)
            {
                Context.Context.PendingTimeEvents.Clear();
            }

            if (context.Event is TimeEvent timeEvent)
            {
                if (Context.Context.PendingTimeEvents.TryGetValue(timeEvent.Id, out var pendingEvent))
                {
                    result = true;
                    ClearTimeEvent(pendingEvent.Id);
                }
                else
                {
                    result = false;
                }
            }

            return Task.FromResult(result);
        }

        public Task AfterProcessEventAsync(IEventContext<Event> context)
        {
            ClearTimeEvents(TimeEventIdsToClear);

            if (
                ConsumedInTransition != null && 
                Context.TryGetStateValues(ConsumedInTransition.Source.Name, out var stateValues) &&
                stateValues.TimeEventIds.TryGetValue(ConsumedInTransition.Identifier, out var timeEventId)
            )
            {
                ClearTimeEvent(timeEventId);

                stateValues.TimeEventIds.Remove(ConsumedInTransition.Identifier);
            }

            var currentStack = (context as BaseContext).Context.Executor
                .GetVerticesStack();

            var enteredStack = currentStack
                .Where(vertex => EnteredStates.Contains(vertex))
                .ToArray();

            RegisterTimeEvents(enteredStack);

            if (
                context.Event.GetType().IsSubclassOf(typeof(RecurringEvent)) &&
                ConsumedInTransition != null &&
                currentStack.Contains(ConsumedInTransition.Source) &&
                !enteredStack.Contains(ConsumedInTransition.Source)
            )
            {
                RegisterTimeEvent(ConsumedInTransition);
            }

            if (Context.Context.PendingTimeEvents.Any())
            {
                Context.Context.TriggerTime = Context.Context.PendingTimeEvents.Values
                    .OrderBy(timeEvent => timeEvent.TriggerTime)
                    .First()
                    .TriggerTime;
            }
            else
            {
                Context.Context.TriggerTime = null;
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
        {
            if (ConsumedInTransition == null && (context as IEdgeContext).Edge.Trigger == context.ExecutionTrigger.Name)
            {
                ConsumedInTransition = (context as IEdgeContext).Edge;
            }

            return Task.CompletedTask;
        }

        public Task AfterHydrateAsync(IStateMachineActionContext context)
            => Task.CompletedTask;

        public Task BeforeDehydrateAsync(IStateMachineActionContext context)
            => Task.CompletedTask;

        private void RegisterTimeEvent(Edge edge)
        {
            var timeEvent = Activator.CreateInstance(edge.TriggerType) as TimeEvent;
            timeEvent.SetTriggerTime(DateTime.Now);
            timeEvent.ConsumerIdentifier = edge.Identifier;
            Context.Context.PendingTimeEvents.Add(timeEvent.Id, timeEvent);
            var timeEventIds = Context.GetStateValues(edge.Source.Name).TimeEventIds;
            timeEventIds.Add(edge.Identifier, timeEvent.Id);
        }

        private void RegisterTimeEvents(IEnumerable<Edge> edges)
        {
            foreach (var edge in edges)
            {
                RegisterTimeEvent(edge);
            }
        }

        private void RegisterTimeEvents(IEnumerable<Vertex> vertices)
        {
            foreach (var currentVertex in vertices)
            {
                var timeEventIds = Context.GetStateValues(currentVertex.Name).TimeEventIds;

                var edges = currentVertex.Edges.Values
                    .Where(edge => edge.TriggerType.IsSubclassOf(typeof(TimeEvent)))
                    .Where(edge => !timeEventIds.ContainsKey(edge.Identifier))
                    .ToArray();

                RegisterTimeEvents(edges);
            }
        }

        private void ClearTimeEvents(IEnumerable<Guid> timeEventIds)
        {
            foreach (var timeEventId in timeEventIds)
            {
                ClearTimeEvent(timeEventId);
            }
        }

        private void ClearTimeEvent(Guid timeEventId)
            => Context.Context.PendingTimeEvents.Remove(timeEventId);
    }
}

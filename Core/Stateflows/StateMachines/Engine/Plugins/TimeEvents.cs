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
    internal class TimeEvents : IStateMachinePlugin, IEqualityComparer<Edge>
    {
        private readonly List<Vertex> EnteredStates = new List<Vertex>();

        private readonly List<Guid> TimeEventIdsToClear = new List<Guid>();

        private readonly List<Guid> StartupEventIdsToClear = new List<Guid>();

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

            var startupEventIds = stateValues.StartupEventIds.Values.ToArray();
            StartupEventIdsToClear.AddRange(startupEventIds);

            return Task.CompletedTask;
        }

        public Task<bool> BeforeProcessEventAsync(IEventActionContext<Event> context)
        {
            var result = true;

            Context = (context as BaseContext).Context;

            if (context.Event is ResetRequest)
            {
                Context.Context.PendingTimeEvents.Clear();
            }

            if (context.Event is TimeEvent timeEvent)
            {
                result = Context.Context.PendingTimeEvents.ContainsKey(timeEvent.Id);
            }

            return Task.FromResult(result);
        }

        public Task AfterProcessEventAsync(IEventActionContext<Event> context)
        {
            ClearTimeEvents(TimeEventIdsToClear);
            ClearStartupEvents(StartupEventIdsToClear);

            if (
                ConsumedInTransition != null && 
                Context.TryGetStateValues(ConsumedInTransition.Source.Name, out var stateValues) &&
                stateValues.TimeEventIds.TryGetValue(ConsumedInTransition.Identifier, out var timeEventId)
            )
            {
                ClearTimeEvent(timeEventId);

                stateValues.TimeEventIds.Remove(ConsumedInTransition.Identifier);
            }

            var currentStack = (context as BaseContext).Context.Executor.VerticesStack;

            var enteredStack = currentStack
                .Where(vertex => EnteredStates.Contains(vertex))
                .ToArray();

            RegisterTimeEvents(enteredStack);
            RegisterStartupEvents(enteredStack);

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

            Context.Context.TriggerOnStartup = Context.Context.PendingStartupEvents.Any();

            return Task.CompletedTask;
        }

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

        private void RegisterStartupEvent(Edge edge)
        {
            var startupEventIds = Context.GetStateValues(edge.Source.Name).StartupEventIds;
            if (startupEventIds.ContainsKey(edge.Identifier))
            {
                return;
            }

            var startupEvent = Activator.CreateInstance(edge.TriggerType) as Startup;
            startupEvent.ConsumerSignature = edge.Identifier;
            Context.Context.PendingStartupEvents.Add(startupEvent.Id, startupEvent);
            startupEventIds.Add(edge.Identifier, startupEvent.Id);
        }

        private void RegisterStartupEvents(IEnumerable<Edge> edges)
        {
            foreach (var edge in edges)
            {
                RegisterStartupEvent(edge);
            }
        }

        private void RegisterStartupEvents(IEnumerable<Vertex> vertices)
        {
            foreach (var currentVertex in vertices)
            {
                var timeEventIds = Context.GetStateValues(currentVertex.Name).StartupEventIds;

                var edges = currentVertex.Edges.Values
                    .Where(edge =>
                        !edge.IsElse &&
                        edge.TriggerType == typeof(Startup) &&
                        !timeEventIds.ContainsKey(edge.Identifier)
                    )
                    .Distinct(this)
                    .ToArray();

                RegisterStartupEvents(edges);
            }
        }

        private void ClearStartupEvents(IEnumerable<Guid> startupEventIds)
        {
            foreach (var startupEventId in startupEventIds)
            {
                ClearStartupEvent(startupEventId);
            }
        }

        private void ClearStartupEvent(Guid startupEventId)
            => Context.Context.PendingStartupEvents.Remove(startupEventId);

        private void RegisterTimeEvent(Edge edge)
        {
            var timeEventIds = Context.GetStateValues(edge.Source.Name).TimeEventIds;
            if (timeEventIds.ContainsKey(edge.Identifier))
            {
                return;
            }

            var timeEvent = Activator.CreateInstance(edge.TriggerType) as TimeEvent;
            timeEvent.SetTriggerTime(DateTime.Now);
            timeEvent.ConsumerSignature = edge.Signature;
            Context.Context.PendingTimeEvents.Add(timeEvent.Id, timeEvent);
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
                    .Where(edge =>
                        !edge.IsElse &&
                        edge.TriggerType.IsSubclassOf(typeof(TimeEvent)) &&
                        !timeEventIds.ContainsKey(edge.Identifier)
                    )
                    .Distinct(this)
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

        public bool Equals(Edge x, Edge y)
            => x.Identifier == y.Identifier;

        public int GetHashCode(Edge obj)
            => obj.Identifier.GetHashCode();
    }
}

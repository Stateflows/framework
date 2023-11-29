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

        private readonly List<Vertex> EnteredStates = new List<Vertex>();

        private readonly List<string> TimeEventIdsToClear = new List<string>();

        private Edge ConsumedInTransition { get; set; }

        private RootContext Context { get; set; }

        public TimeEvents(IStateflowsScheduler scheduler)
        {
            Scheduler = scheduler;
        }

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
            var timeEventIds = stateValues.TimeEventIds.Values;
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

        public async Task<bool> BeforeProcessEventAsync(IEventContext<Event> context)
        {
            Context = (context as BaseContext).Context;

            if (context.Event is ResetRequest)
            {
                await Scheduler.Clear(context.StateMachine.Id.BehaviorId, Context.StateValues.Values.SelectMany(v => v.TimeEventIds.Values));
            }

            return true;
        }

        public async Task AfterProcessEventAsync(IEventContext<Event> context)
        {
            await ClearTimeTokens(TimeEventIdsToClear);

            if (ConsumedInTransition != null)
            {
                var timeEventIds = Context.GetStateValues(ConsumedInTransition.Source.Name).TimeEventIds;

                if (timeEventIds.ContainsKey(ConsumedInTransition.Identifier))
                {
                    await ClearTimeToken(timeEventIds[ConsumedInTransition.Identifier]);

                    timeEventIds.Remove(ConsumedInTransition.Identifier);
                }
            }

            var currentStack = (context as BaseContext).Context.Executor
                .GetVerticesStack();

            var enteredStack = currentStack
                .Where(vertex => EnteredStates.Contains(vertex))
                .ToArray();

            await RegisterTimeTokens(enteredStack);

            if (
                context.Event.GetType().IsSubclassOf(typeof(RecurringEvent)) &&
                ConsumedInTransition != null &&
                currentStack.Contains(ConsumedInTransition.Source) &&
                !enteredStack.Contains(ConsumedInTransition.Source)
            )
            {
                var timeEventIds = Context.GetStateValues(ConsumedInTransition.Source.Name).TimeEventIds;

                var token = new TimeToken()
                {
                    TargetId = context.StateMachine.Id.BehaviorId,
                    Event = Activator.CreateInstance(ConsumedInTransition.TriggerType) as TimeEvent,
                    CreatedAt = DateTime.Now,
                    EdgeIdentifier = ConsumedInTransition.Identifier
                };

                await Scheduler.Register(new TimeToken[] { token });

                timeEventIds.Add(token.EdgeIdentifier, token.Id);
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
            if (ConsumedInTransition == null && (context as IEdgeContext).Edge.Trigger == context.ExecutionTrigger.EventName)
            {
                ConsumedInTransition = (context as IEdgeContext).Edge;
            }

            return Task.CompletedTask;
        }

        public Task AfterHydrateAsync(IStateMachineActionContext context)
            => Task.CompletedTask;

        public Task BeforeDehydrateAsync(IStateMachineActionContext context)
            => Task.CompletedTask;

        private async Task RegisterTimeTokens(IEnumerable<Vertex> vertices)
        {
            foreach (var currentVertex in vertices)
            {
                var timeEventIds = Context.GetStateValues(currentVertex.Name).TimeEventIds;

                var tokens = currentVertex.Edges.Values
                    .Where(edge => edge.TriggerType.IsSubclassOf(typeof(TimeEvent)))
                    .Where(edge => !timeEventIds.ContainsValue(edge.Identifier))
                    .Select(edge => new TimeToken()
                    {
                        TargetId = Context.Id.BehaviorId,
                        Event = Activator.CreateInstance(edge.TriggerType) as TimeEvent,
                        CreatedAt = DateTime.Now,
                        EdgeIdentifier = edge.Identifier
                    })
                    .ToArray();

                await Scheduler.Register(tokens);

                foreach (var token in tokens)
                {
                    timeEventIds.Add(token.EdgeIdentifier, token.Id);
                }
            }
        }

        private Task ClearTimeTokens(IEnumerable<string> timeEventIds)
            => Scheduler.Clear(Context.Context.Id, timeEventIds);

        private Task ClearTimeToken(string timeEventId)
            => ClearTimeTokens(new string[] { timeEventId });
    }
}

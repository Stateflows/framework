using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Context;
using Stateflows.StateMachines.Engine;
using Stateflows.StateMachines.Registration;

namespace Stateflows.StateMachines.Context.Classes
{
    internal class RootContext
    {
        public readonly StateMachineId Id;

        internal readonly StateflowsContext Context;

        internal readonly Executor Executor;

        public RootContext(StateflowsContext context, Executor executor, Event @event)
        {
            Context = context;
            Id = Context.Id;
            Executor = executor;
            SetEvent(@event);
        }

        public Dictionary<string, string> GlobalValues => Context.GlobalValues;

        private List<Event> deferredEvents = null;
        public List<Event> DeferredEvents
        {
            get
            {
                if (deferredEvents == null)
                {
                    if (!Context.Values.TryGetValue(Constants.DeferredEvents, out var deferredEventsObj))
                    {
                        deferredEvents = new List<Event>();
                        Context.Values[Constants.DeferredEvents] = deferredEvents;
                    }
                    else
                    {
                        deferredEvents = deferredEventsObj as List<Event>;
                    }
                }

                return deferredEvents;
            }
        }

        private Dictionary<BehaviorId, EmbeddedBehaviorStatus> embeddedBehaviorStatuses = null;
        public Dictionary<BehaviorId, EmbeddedBehaviorStatus> EmbeddedBehaviorStatuses
        {
            get
            {
                if (embeddedBehaviorStatuses == null)
                {
                    if (!Context.Values.TryGetValue(Constants.EmbeddedBehaviorStatuses, out var stateContextsObj))
                    {
                        embeddedBehaviorStatuses = new Dictionary<BehaviorId, EmbeddedBehaviorStatus>();
                        Context.Values[Constants.EmbeddedBehaviorStatuses] = embeddedBehaviorStatuses;
                    }
                    else
                    {
                        embeddedBehaviorStatuses = stateContextsObj as Dictionary<BehaviorId, EmbeddedBehaviorStatus>;
                    }
                }

                return embeddedBehaviorStatuses;
            }
        }

        private Dictionary<string, StateValues> stateValues = null;
        public Dictionary<string, StateValues> StateValues
        {
            get
            {
                if (stateValues == null)
                {
                    if (!Context.Values.TryGetValue(Constants.StateValues, out var stateContextsObj))
                    {
                        stateValues = new Dictionary<string, StateValues>();
                        Context.Values[Constants.StateValues] = stateValues;
                    }
                    else
                    {
                        stateValues = stateContextsObj as Dictionary<string, StateValues>;
                    }
                }

                return stateValues;
            }
        }

        public void ClearStateValues(string stateName)
        {
            StateValues.Remove(stateName);
        }

        public bool TryGetStateValues(string stateName, out StateValues values)
            => StateValues.TryGetValue(stateName, out values);

        public StateValues GetStateValues(string stateName)
        {
            if (!StateValues.TryGetValue(stateName, out var values))
            {
                values = new StateValues();
                StateValues[stateName] = values;
            }

            return values;
        }

        private List<string> statesStack = null;
        public List<string> StatesStack
        {
            get
            {
                if (statesStack == null)
                {
                    if (!Context.Values.TryGetValue(Constants.StatesStack, out var statesStackObj))
                    {
                        statesStack = new List<string>();
                        Context.Values[Constants.StatesStack] = statesStack;
                    }
                    else
                    {
                        statesStack = statesStackObj as List<string>;
                    }
                }

                return statesStack;
            }
        }

        public string State { get; set; } = string.Empty;

        private readonly Stack<Event> EventsStack = new Stack<Event>();

        public void SetEvent(Event @event)
        {
            EventsStack.Push(@event);
        }

        public void ClearEvent()
        {
            EventsStack.Pop();
        }

        public Event Event => EventsStack.Any()
            ? EventsStack.Peek()
            : null;

        public readonly List<Exception> Exceptions = new List<Exception>();

        public EventStatus? ForceStatus { get; set; } = null;

        public async Task Send<TEvent>(TEvent @event)
            where TEvent : Event, new()
        {
            var locator = Executor.ServiceProvider.GetService<IBehaviorLocator>();
            if (locator != null && locator.TryLocateBehavior(Id, out var behavior))
            {
                await behavior.SendAsync(@event);
            }
        }
    }
}

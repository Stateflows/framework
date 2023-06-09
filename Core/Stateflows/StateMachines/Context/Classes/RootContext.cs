﻿using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Context;
using Stateflows.Common.Interfaces;
using Stateflows.StateMachines.Engine;
using Stateflows.StateMachines.Registration;

namespace Stateflows.StateMachines.Context.Classes
{
    internal class RootContext
    {
        public StateMachineId Id { get; }

        internal StateflowsContext Context { get; set; }

        internal Executor Executor { get; set; }

        public RootContext(StateflowsContext context)
        {
            Context = context;
            Id = new StateMachineId(Context.Id);
        }

        private Dictionary<string, string> globalValues = null;
        public Dictionary<string, string> GlobalValues
        {
            get
            {
                if (globalValues == null)
                {
                    if (!Context.Values.TryGetValue(Constants.GlobalValues, out var globalValuesObj))
                    {
                        globalValues = new Dictionary<string, string>();
                        Context.Values[Constants.GlobalValues] = globalValues;
                    }
                    else
                    {
                        globalValues = globalValuesObj as Dictionary<string, string>;
                    }
                }

                return globalValues;
            }
        }

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

        public StateValues GetStateValues(string stateName)
        {
            if (!StateValues.TryGetValue(stateName, out var stateContext))
            {
                stateContext = new StateValues();
                StateValues[stateName] = stateContext;
            }

            return stateContext;
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

        public bool ForceConsumed
        {
            get => Context.Values.TryGetValue(Constants.ForceConsumed, out var consumed) && (bool)consumed;
            set => Context.Values[Constants.ForceConsumed] = value;
        }

        internal void ClearTemporaryInternalValues()
        {
            Context.Values.Remove(Constants.State);
            Context.Values.Remove(Constants.Event);
            Context.Values.Remove(Constants.SourceState);
            Context.Values.Remove(Constants.TargetState);
            Context.Values.Remove(Constants.ForceConsumed);
        }

        public async Task Send<TEvent>(TEvent @event)
            where TEvent : Event, new()
        {
            var locator = Executor.ServiceProvider.GetService<IBehaviorLocator>();
            if (locator != null && locator.TryLocateBehavior(Id.BehaviorId, out var behavior))
            {
                await behavior.SendAsync(@event);
            }
        }
    }
}

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Context;
using Stateflows.StateMachines.Engine;
using Stateflows.StateMachines.Registration;
using System.Net.Http.Headers;
using Stateflows.Common.Utilities;

namespace Stateflows.StateMachines.Context.Classes
{
    internal class RootContext
    {
        public readonly StateMachineId Id;

        private StateflowsContext stateflowsContext;

        internal StateflowsContext Context => stateflowsContext;

        internal Executor Executor { get; set; }

        public RootContext(StateflowsContext context, Executor executor, EventHolder @event)
        {
            stateflowsContext = context;
            Id = stateflowsContext.Id;
            Executor = executor;
            SetEvent(@event);
        }

        public void SetContext(StateflowsContext context)
        {
            stateflowsContext = context;
            deferredEvents = null;
            embeddedBehaviorStatuses = null;
            stateValues = null;
            // statesStack = null;
        }

        public Dictionary<string, string> GlobalValues => stateflowsContext.GlobalValues;

        private List<EventHolder> deferredEvents = null;
        public List<EventHolder> DeferredEvents
        {
            get
            {
                if (deferredEvents == null)
                {
                    if (!stateflowsContext.Values.TryGetValue(Constants.DeferredEvents, out var deferredEventsObj))
                    {
                        deferredEvents = new List<EventHolder>();
                        stateflowsContext.Values[Constants.DeferredEvents] = deferredEvents;
                    }
                    else
                    {
                        deferredEvents = deferredEventsObj as List<EventHolder>;
                        if (deferredEvents == null)
                        {
                            deferredEvents = new List<EventHolder>();
                            stateflowsContext.Values[Constants.DeferredEvents] = deferredEvents;
                        }
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
                    if (!stateflowsContext.Values.TryGetValue(Constants.EmbeddedBehaviorStatuses, out var stateContextsObj))
                    {
                        embeddedBehaviorStatuses = new Dictionary<BehaviorId, EmbeddedBehaviorStatus>();
                        stateflowsContext.Values[Constants.EmbeddedBehaviorStatuses] = embeddedBehaviorStatuses;
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
                    if (!stateflowsContext.Values.TryGetValue(Constants.StateValues, out var stateContextsObj))
                    {
                        stateValues = new Dictionary<string, StateValues>();
                        stateflowsContext.Values[Constants.StateValues] = stateValues;
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

        private Tree<string> statesTree = null;
        public Tree<string> StatesTree
        {
            get
            {
                if (statesTree == null)
                {
                    if (!stateflowsContext.Values.TryGetValue(Constants.StatesTree, out var statesTreeObj))
                    {
                        statesTree = new Tree<string>();

                        // import historical, linear entries
                        if (stateflowsContext.Values.TryGetValue(Constants.StatesStack, out var stackObj))
                        {
                            var stack = stackObj as List<string>;
                            TreeNode<string> tree = null;
                            foreach (var state in stack)
                            {
                                tree = tree == null
                                    ? new TreeNode<string>(state)
                                    : tree.Add(state);
                            }
                            statesTree.Root = tree;
                            stateflowsContext.Values.Remove(Constants.StatesStack);
                        }

                        stateflowsContext.Values[Constants.StatesTree] = statesTree;
                    }
                    else
                    {
                        statesTree = statesTreeObj as Tree<string>;
                    }
                }

                return statesTree;
            }
        }

        public string State { get; set; } = string.Empty;

        private readonly Stack<EventHolder> EventsStack = new Stack<EventHolder>();

        public void SetEvent(EventHolder eventHolder)
        {
            EventsStack.Push(eventHolder);
        }

        public void ClearEvent()
        {
            EventsStack.Pop();
        }

        public EventHolder EventHolder => EventsStack.Any()
            ? EventsStack.Peek()
            : null;

        public EventHolder ExecutionTriggerHolder => EventsStack.Any()
            ? EventsStack.Last()
            : null;

        private readonly List<IExecutionStep> executionSteps = new List<IExecutionStep>();

        public IEnumerable<IExecutionStep> ExecutionSteps => executionSteps;

        public void AddExecutionStep(string sourceStateName, string targetStateName, object transitionTrigger)
            => executionSteps.Add(new ExecutionStep(sourceStateName, targetStateName, transitionTrigger));

        private readonly List<Exception> exceptions = new List<Exception>();

        public IEnumerable<Exception> Exceptions => exceptions;

        public void AddException(Exception exception)
            => exceptions.Add(exception);

        public void RemoveException(Exception exception)
            => exceptions.Remove(exception);

        public void ClearExceptions()
            => exceptions.Clear();

        public EventStatus? ForceStatus { get; set; } = null;

        public async Task SendAsync<TEvent>(TEvent @event, IEnumerable<EventHeader> headers = null)
        {
            var locator = Executor.ServiceProvider.GetService<IBehaviorLocator>();
            if (locator != null && locator.TryLocateBehavior(Id, out var behavior))
            {
                await behavior.SendAsync(@event, headers).ConfigureAwait(false);
            }
        }
    }
}

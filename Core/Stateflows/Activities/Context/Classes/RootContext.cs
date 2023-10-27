using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Context;
using Stateflows.Common.Interfaces;
using Stateflows.Activities.Models;
using Stateflows.Activities.Engine;
using Stateflows.Activities.Streams;
using Stateflows.Activities.Registration;

namespace Stateflows.Activities.Context.Classes
{
    public class RootContext
    {
        public ActivityId Id { get; }

        internal StateflowsContext Context { get; set; }

        internal Executor Executor { get; set; }

        public RootContext(StateflowsContext context)
        {
            Context = context;
            Id = new ActivityId(Context.Id);
        }

        public Dictionary<string, string> GlobalValues => Context.GlobalValues;

        private Dictionary<string, ActionValues> actionValues = null;
        public Dictionary<string, ActionValues> ActionValues
        {
            get
            {
                lock (Context.Values)
                {
                    if (actionValues == null)
                    {
                        if (!Context.Values.TryGetValue(Constants.ActionValues, out var actionValuesObj))
                        {
                            actionValues = new Dictionary<string, ActionValues>();
                            Context.Values[Constants.ActionValues] = actionValues;
                        }
                        else
                        {
                            actionValues = actionValuesObj as Dictionary<string, ActionValues>;
                        }
                    }
                }

                return actionValues;
            }
        }

        public ActionValues GetActionValues(string nodeName)
        {
            if (!ActionValues.TryGetValue(nodeName, out var activityContext))
            {
                activityContext = new ActionValues();
                ActionValues[nodeName] = activityContext;
            }

            return activityContext;
        }

        private Dictionary<string, Dictionary<string, Stream>> streams = null;
        public Dictionary<string, Dictionary<string, Stream>> Streams
        {
            get
            {
                lock (Context.Values)
                {
                    if (streams == null)
                    {
                        if (!Context.Values.TryGetValue(Constants.Streams, out var streamsObj))
                        {
                            streams = new Dictionary<string, Dictionary<string, Stream>>();
                            Context.Values[Constants.Streams] = streams;
                        }
                        else
                        {
                            streams = streamsObj as Dictionary<string, Dictionary<string, Stream>>;
                        }
                    }
                }

                return streams;
            }
        }

        public Stream GetStream(string edgeIdentifier, string threadId)
        {
            Stream stream;

            lock (Streams)
            {
                if (!Streams.TryGetValue(threadId, out var edges))
                {
                    edges = new Dictionary<string, Stream>();
                    Streams.Add(threadId, edges);
                }

                if (!edges.TryGetValue(edgeIdentifier, out stream))
                {
                    stream = new Stream() { EdgeIdentifier = edgeIdentifier };
                    edges.Add(edgeIdentifier, stream);
                }
            }

            return stream;
        }

        private Dictionary<string, Dictionary<string, List<Token>>> outputTokens = null;
        public Dictionary<string, Dictionary<string, List<Token>>> OutputTokens
        {
            get
            {
                lock (Context.Values)
                {
                    if (outputTokens == null)
                    {
                        if (!Context.Values.TryGetValue(Constants.OutputTokens, out var outputTokensObj))
                        {
                            outputTokens = new Dictionary<string, Dictionary<string, List<Token>>>();
                            Context.Values[Constants.OutputTokens] = outputTokens;
                        }
                        else
                        {
                            outputTokens = outputTokensObj as Dictionary<string, Dictionary<string, List<Token>>>;
                        }
                    }
                }

                return outputTokens;
            }
        }

        public List<Token> GetOutputTokens(string nodeName, string threadId)
        {
            List<Token> tokens;

            lock (OutputTokens)
            {
                if (!OutputTokens.TryGetValue(threadId, out var nodes))
                {
                    nodes = new Dictionary<string, List<Token>>();
                    OutputTokens[threadId] = nodes;
                }

                if (!nodes.TryGetValue(nodeName, out tokens))
                {
                    tokens = new List<Token>();
                    nodes[nodeName] = tokens;
                }
            }

            return tokens;
        }

        //private List<string> nodesStack = null;
        //public List<string> NodesStack
        //{
        //    get
        //    {
        //        if (nodesStack == null)
        //        {
        //            if (!Context.Values.TryGetValue(Constants.NodesStack, out var statesStackObj))
        //            {
        //                nodesStack = new List<string>();
        //                Context.Values[Constants.NodesStack] = nodesStack;
        //            }
        //            else
        //            {
        //                nodesStack = statesStackObj as List<string>;
        //            }
        //        }

        //        return nodesStack;
        //    }
        //}

        public bool Initialized
        {
            get => Context.Values.TryGetValue(Constants.Initialized, out var consumed) && (bool)consumed;
            set => Context.Values[Constants.Initialized] = value;
        }

        public bool ForceConsumed
        {
            get => Context.Values.TryGetValue(Constants.ForceConsumed, out var consumed) && (bool)consumed;
            set => Context.Values[Constants.ForceConsumed] = value;
        }

        public Event Event { get; set; }

        internal Exception Exception { get; set; }

        internal Node NodeOfOrigin { get; set; }

        internal void ClearTemporaryInternalValues()
        {
            Context.Values.Remove(Constants.Node);
            Context.Values.Remove(Constants.Event);
            Context.Values.Remove(Constants.SourceNode);
            Context.Values.Remove(Constants.TargetNode);
            Context.Values.Remove(Constants.ForceConsumed);
        }

        public async Task Send<TEvent>(TEvent @event)
            where TEvent : Event, new()
        {
            var locator = Executor.NodeScope.ServiceProvider.GetService<IBehaviorLocator>();
            if (locator != null && locator.TryLocateBehavior(Id.BehaviorId, out var behavior))
            {
                await behavior.SendAsync(@event);
            }
        }
    }
}

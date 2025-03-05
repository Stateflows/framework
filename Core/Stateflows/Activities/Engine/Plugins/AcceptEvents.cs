using System;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using Stateflows.Common;
using Stateflows.Activities.Models;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities.Engine
{
    internal class AcceptEvents : ActivityPlugin
    {
        private RootContext Context { get; set; }

        private void RegisterTimeEvent(Node node)
        {
            if (Context.NodeTimeEvents.ContainsKey(node.Identifier))
            {
                return;
            }

            foreach (var eventType in node.ActualEventTypes)
            {
                if (!eventType.IsSubclassOf(typeof(TimeEvent)))
                {
                    continue;
                }

                var timeEvent = Activator.CreateInstance(eventType) as TimeEvent;
                timeEvent.SetTriggerTime(DateTime.Now);
                timeEvent.ConsumerSignature = node.Identifier;
                Context.Context.PendingTimeEvents.Add(timeEvent.Id, timeEvent);
                Context.NodeTimeEvents[node.Identifier] = timeEvent.Id;
            }
        }

        private void ClearTimeEvent(Node node)
        {
            if (Context.NodeTimeEvents.TryGetValue(node.Identifier, out var timeEventId))
            {
                Context.Context.PendingTimeEvents.Remove(timeEventId);
                Context.NodeTimeEvents.Remove(node.Identifier);
            }
        }

        private void RegisterStartupEvent(Node node)
        {
            if (Context.NodeStartupEvents.ContainsKey(node.Identifier))
            {
                return;
            }

            var startupEvent = new Startup()
            {
                ConsumerSignature = node.Identifier
            };

            Context.Context.PendingStartupEvents.Add(startupEvent.Id, startupEvent);
            Context.NodeStartupEvents[node.Identifier] = startupEvent.Id;
        }

        private void ClearStartupEvent(Node node)
        {
            if (Context.NodeStartupEvents.TryGetValue(node.Identifier, out var startupEventId))
            {
                Context.Context.PendingStartupEvents.Remove(startupEventId);
                Context.NodeStartupEvents.Remove(node.Identifier);
            }
        }

        public void RegisterAcceptEventNodes(IEnumerable<(Node Node, Guid ThreadId)> nodes)
        {
            lock (Context)
            {
                foreach ((var node, var threadId) in nodes)
                {
                    if (Context.ActiveNodes.Keys.Contains(node.Identifier))
                    {
                        continue;
                    }

                    Context.ActiveNodes.Add(node.Identifier, threadId);

                    if (node.ActualEventTypes.Any(type => type.IsSubclassOf(typeof(TimeEvent))))
                    {
                        RegisterTimeEvent(node);
                    }

                    if (node.ActualEventTypes.Any(type => type == typeof(TimeEvent)))
                    {
                        RegisterStartupEvent(node);
                    }
                }
            }
        }

        public void RegisterAcceptEventNode(Node node, Guid threadId)
        {
            lock (Context)
            {
                if (Context.ActiveNodes.Keys.Contains(node.Identifier))
                {
                    return;
                }

                Context.ActiveNodes.Add(node.Identifier, threadId);

                if (node.ActualEventTypes.Any(type => type.IsSubclassOf(typeof(TimeEvent))))
                {
                    RegisterTimeEvent(node);
                }

                if (node.ActualEventTypes.Any(type => type == typeof(TimeEvent)))
                {
                    RegisterStartupEvent(node);
                }
            }
        }

        public void UnregisterAcceptEventNodes(IEnumerable<Node> nodes)
        {
            lock (Context)
            {
                foreach (var node in nodes)
                {
                    Context.ActiveNodes.Remove(node.Identifier);

                    ClearTimeEvent(node);

                    ClearStartupEvent(node);
                }
            }
        }

        public void UnregisterAcceptEventNode(Node node)
        {
            lock (Context)
            {
                Context.ActiveNodes.Remove(node.Identifier);

                ClearTimeEvent(node);

                ClearStartupEvent(node);
            }
        }

        public override Task AfterActivityInitializeAsync(IActivityInitializationContext context, bool initialized)
            => Task.CompletedTask;

        public override Task AfterActivityFinalizeAsync(IActivityFinalizationContext context)
        {
            UnregisterAcceptEventNodes((context as IRootContext).Context.Executor.Graph.DanglingTimeEventActionNodes);

            return Task.CompletedTask;
        }

        public override Task AfterNodeFinalizeAsync(IActivityNodeContext context)
        {
            UnregisterAcceptEventNodes((context as ActionContext).Node.DanglingTimeEventActionNodes);

            return Task.CompletedTask;
        }

        public override Task AfterNodeInitializeAsync(IActivityNodeContext context)
            => Task.CompletedTask;

        public override Task AfterProcessEventAsync<TEvent>(IEventContext<TEvent> context)
        {
            Trace.WriteLine($"⦗→s⦘ Activity '{context.Behavior.Id.Name}:{context.Behavior.Id.Instance}': processed event '{Event.GetName(context.Event.GetType())}'");

            Context = ((BaseContext)context).Context;

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

        public override Task BeforeActivityInitializeAsync(IActivityInitializationContext context)
        {
            RegisterAcceptEventNodes(
                (context as IRootContext).Context.Executor.Graph.AcceptEventActionNodes.Select(node => (node, Guid.NewGuid()))
            );

            return Task.CompletedTask;
        }

        public override Task BeforeNodeInitializeAsync(IActivityNodeContext context)
        {
            RegisterAcceptEventNodes(
                (context as ActionContext).Node.DanglingTimeEventActionNodes.Select(node => (node, Guid.NewGuid()))
            );

            return Task.CompletedTask;
        }

        public override Task BeforeNodeFinalizeAsync(IActivityNodeContext context)
            => Task.CompletedTask;

        public override Task<bool> BeforeProcessEventAsync<TEvent>(IEventContext<TEvent> context)
        {
            var result = true;

            Context = (context as BaseContext).Context;

            if (context.Event is TimeEvent timeEvent)
            {
                result = Context.Context.PendingTimeEvents.ContainsKey(timeEvent.Id);
            }

            if (context.Event is Startup startupEvent)
            {
                result = Context.Context.PendingStartupEvents.ContainsKey(startupEvent.Id);
            }

            if (result)
            {
                Trace.WriteLine($"⦗→s⦘ Activity '{context.Behavior.Id.Name}:{context.Behavior.Id.Instance}': received event '{Event.GetName(context.Event.GetType())}', trying to process it");
            }

            return Task.FromResult(result);
        }
    }
}

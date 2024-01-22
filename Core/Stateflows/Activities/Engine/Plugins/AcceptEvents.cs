using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Stateflows.Common;
using Stateflows.Activities.Models;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities.Engine
{
    internal class AcceptEvents : IActivityPlugin
    {
        private RootContext Context { get; set; }

        private void RegisterTimeEvent(Node node)
        {
            var timeEvent = Activator.CreateInstance(node.EventType) as TimeEvent;
            timeEvent.SetTriggerTime(DateTime.Now);
            timeEvent.ConsumerSignature = node.Identifier;
            Context.Context.PendingTimeEvents.Add(timeEvent.Id, timeEvent);
            Context.NodeTimeEvents[node.Identifier] = timeEvent.Id;
        }

        private void ClearTimeEvent(Node node)
        {
            if (Context.NodeTimeEvents.TryGetValue(node.Identifier, out var timeEventId))
            {
                Context.Context.PendingTimeEvents.Remove(timeEventId);
                Context.NodeTimeEvents.Remove(node.Identifier);
            }
        }

        public void RegisterAcceptEventNodes(IEnumerable<(Node Node, Guid ThreadId)> nodes)
        {
            lock (Context)
            {
                foreach ((var node, var threadId) in nodes)
                {
                    Context.ActiveNodes.Add(node.Identifier, threadId);

                    if (node.EventType.IsSubclassOf(typeof(TimeEvent)))
                    {
                        RegisterTimeEvent(node);
                    }
                }
            }
        }

        public void RegisterAcceptEventNode(Node node, Guid threadId)
        {
            lock (Context)
            {
                Context.ActiveNodes.Add(node.Identifier, threadId);

                if (node.EventType.IsSubclassOf(typeof(TimeEvent)))
                {
                    RegisterTimeEvent(node);
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
                }
            }
        }

        public void UnregisterAcceptEventNode(Node node)
        {
            lock (Context)
            {
                Context.ActiveNodes.Remove(node.Identifier);

                ClearTimeEvent(node);
            }
        }

        Task IActivityObserver.AfterActivityInitializationAsync(IActivityInitializationContext context)
            => Task.CompletedTask;

        Task IActivityObserver.AfterActivityFinalizationAsync(IActivityFinalizationContext context)
        {
            UnregisterAcceptEventNodes((context as IRootContext).Context.Executor.Graph.AcceptEventActionNodes);

            return Task.CompletedTask;
        }

        Task IActivityObserver.AfterFlowGuardAsync(IGuardContext<Token> context, bool guardResult)
            => Task.CompletedTask;

        Task IActivityObserver.AfterFlowTransformationAsync(ITransformationContext<Token> context)
            => Task.CompletedTask;

        Task IActivityInterceptor.AfterHydrateAsync(IActivityActionContext context)
            => Task.CompletedTask;

        Task IActivityObserver.AfterNodeExecuteAsync(IActivityNodeContext context)
            => Task.CompletedTask;

        Task IActivityObserver.AfterNodeFinalizationAsync(IActivityNodeContext context)
        {
            UnregisterAcceptEventNodes((context as ActionContext).Node.AcceptEventActionNodes);

            return Task.CompletedTask;
        }

        Task IActivityObserver.AfterNodeInitializationAsync(IActivityNodeContext context)
            => Task.CompletedTask;

        Task IActivityInterceptor.AfterProcessEventAsync(IEventContext<Event> context)
        {
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

        Task IActivityObserver.BeforeActivityInitializationAsync(IActivityInitializationContext context)
        {
            RegisterAcceptEventNodes(
                (context as IRootContext).Context.Executor.Graph.AcceptEventActionNodes
                    .Where(node => !node.IncomingEdges.Any())
                    .Select(node => (node, Guid.NewGuid()))
            );

            return Task.CompletedTask;
        }

        Task IActivityObserver.BeforeActivityFinalizationAsync(IActivityFinalizationContext context)
            => Task.CompletedTask;

        Task IActivityInterceptor.BeforeDehydrateAsync(IActivityActionContext context)
            => Task.CompletedTask;

        Task IActivityObserver.BeforeFlowGuardAsync(IGuardContext<Token> context)
            => Task.CompletedTask;

        Task IActivityObserver.BeforeFlowTransformationAsync(ITransformationContext<Token> context)
            => Task.CompletedTask;

        Task IActivityObserver.BeforeNodeExecuteAsync(IActivityNodeContext context)
            => Task.CompletedTask;

        Task IActivityObserver.BeforeNodeInitializationAsync(IActivityNodeContext context)
        {
            RegisterAcceptEventNodes(
                (context as ActionContext).Node.AcceptEventActionNodes
                    .Where(node => !node.IncomingEdges.Any())
                    .Select(node => (node, Guid.NewGuid()))
            );

            return Task.CompletedTask;
        }

        Task IActivityObserver.BeforeNodeFinalizationAsync(IActivityNodeContext context)
            => Task.CompletedTask;

        Task<bool> IActivityInterceptor.BeforeProcessEventAsync(IEventContext<Event> context)
        {
            var result = true;

            Context = (context as BaseContext).Context;

            if (context.Event is TimeEvent timeEvent)
            {
                result = Context.Context.PendingTimeEvents.ContainsKey(timeEvent.Id);
            }

            return Task.FromResult(result);
        }
    }
}

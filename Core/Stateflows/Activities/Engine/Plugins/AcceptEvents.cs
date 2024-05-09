using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Stateflows.Common;
using Stateflows.Activities.Models;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Context.Interfaces;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Stateflows.Activities.Engine
{
    internal class AcceptEvents : IActivityPlugin
    {
        private RootContext Context { get; set; }

        private void RegisterTimeEvent(Node node)
        {
            if (Context.NodeTimeEvents.ContainsKey(node.Identifier))
            {
                return;
            }

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
                    if (Context.ActiveNodes.Keys.Contains(node.Identifier))
                    {
                        continue;
                    }

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
                if (Context.ActiveNodes.Keys.Contains(node.Identifier))
                {
                    return;
                }

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
            UnregisterAcceptEventNodes((context as IRootContext).Context.Executor.Graph.DanglingTimeEventActionNodes);

            return Task.CompletedTask;
        }

        Task IActivityObserver.AfterFlowGuardAsync<TToken>(IGuardContext<TToken> context, bool guardResult)
            => Task.CompletedTask;

        Task IActivityObserver.AfterFlowTransformationAsync<TToken>(ITransformationContext<TToken> context)
            => Task.CompletedTask;

        Task IActivityInterceptor.AfterHydrateAsync(IActivityActionContext context)
            => Task.CompletedTask;

        Task IActivityObserver.AfterNodeExecuteAsync(IActivityNodeContext context)
            => Task.CompletedTask;

        Task IActivityObserver.AfterNodeFinalizationAsync(IActivityNodeContext context)
        {
            UnregisterAcceptEventNodes((context as ActionContext).Node.DanglingTimeEventActionNodes);

            return Task.CompletedTask;
        }

        Task IActivityObserver.AfterNodeInitializationAsync(IActivityNodeContext context)
            => Task.CompletedTask;

        Task IActivityInterceptor.AfterProcessEventAsync(IEventContext<Event> context)
        {
            Context = (context as BaseContext).Context;

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
                (context as IRootContext).Context.Executor.Graph.AcceptEventActionNodes.Select(node => (node, Guid.NewGuid()))
            );

            return Task.CompletedTask;
        }

        Task IActivityObserver.BeforeActivityFinalizationAsync(IActivityFinalizationContext context)
            => Task.CompletedTask;

        Task IActivityInterceptor.BeforeDehydrateAsync(IActivityActionContext context)
            => Task.CompletedTask;

        Task IActivityObserver.BeforeFlowGuardAsync<TToken>(IGuardContext<TToken> context)
            => Task.CompletedTask;

        Task IActivityObserver.BeforeFlowTransformationAsync<TToken>(ITransformationContext<TToken> context)
            => Task.CompletedTask;

        Task IActivityObserver.BeforeNodeExecuteAsync(IActivityNodeContext context)
            => Task.CompletedTask;

        Task IActivityObserver.BeforeNodeInitializationAsync(IActivityNodeContext context)
        {
            RegisterAcceptEventNodes(
                (context as ActionContext).Node.DanglingTimeEventActionNodes.Select(node => (node, Guid.NewGuid()))
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

        public Task OnActivityInitializationExceptionAsync(IActivityInitializationContext context, Exception exception)
            => Task.CompletedTask;

        public Task OnActivityFinalizationExceptionAsync(IActivityFinalizationContext context, Exception exception)
            => Task.CompletedTask;

        public Task OnNodeInitializationExceptionAsync(IActivityNodeContext context, Exception exception)
            => Task.CompletedTask;

        public Task OnNodeFinalizationExceptionAsync(IActivityNodeContext context, Exception exception)
            => Task.CompletedTask;

        public Task OnNodeExecutionExceptionAsync(IActivityNodeContext context, Exception exception)
            => Task.CompletedTask;

        public Task OnFlowGuardExceptionAsync<TToken>(IGuardContext<TToken> context, Exception exception)
            => Task.CompletedTask;

        public Task OnFlowTransformationExceptionAsync<TToken>(ITransformationContext<TToken> context, Exception exception)
            => Task.CompletedTask;
    }
}

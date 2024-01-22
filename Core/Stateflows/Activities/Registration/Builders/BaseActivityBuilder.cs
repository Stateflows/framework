﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Activities.Models;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.Exceptions;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Activities.Registration.Builders;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Registration
{
    internal class BaseActivityBuilder : IInternal
    {
        public Graph Result => Node is Graph
            ? Node as Graph
            : Node.Graph;

        public Node Node { get; set; }

        public IServiceCollection Services { get; }

        public BaseActivityBuilder(Node parentNode, IServiceCollection services)
        {
            Services = services;
            Node = parentNode;
        }

        public BaseActivityBuilder AddNode(NodeType type, string nodeName, ActionDelegateAsync actionAsync, NodeBuilderAction buildAction = null, Type exceptionOrEventType = null)
        {
            if (Node.Type != NodeType.Activity)
            {
                nodeName = $"{Node.Name}.{nodeName}";
            }

            var namedNodeTypes = new NodeType[] {
                NodeType.Action,
                NodeType.Join,
                NodeType.Merge,
                NodeType.Fork,
                NodeType.Decision,
                NodeType.ExceptionHandler,
                NodeType.StructuredActivity,
                NodeType.ParallelActivity,
                NodeType.IterativeActivity,
                NodeType.AcceptEventAction,
                NodeType.Output,
                NodeType.Final,
                NodeType.DataStore
            };

            if (namedNodeTypes.Contains(type))
            {
                if (string.IsNullOrEmpty(nodeName))
                {
                    throw new NodeDefinitionException(nodeName, $"Node name cannot be empty");
                }

                if (Result.AllNamedNodes.ContainsKey(nodeName))
                {
                    throw new NodeDefinitionException(nodeName, $"Node '{nodeName}' is already registered");
                }
            }

            var node = new Node()
            {
                Type = type,
                Name = nodeName,
                Parent = Node,
                Graph = Result,
                Level = Node.Level + 1
            };

            if (type == NodeType.ExceptionHandler)
            {
                if (exceptionOrEventType is null)
                {
                    throw new ExceptionHandlerDefinitionException(nodeName, "Exception type not provided");
                }

                node.ExceptionType = exceptionOrEventType;
            }

            if (type == NodeType.AcceptEventAction)
            {
                if (exceptionOrEventType is null)
                {
                    throw new AcceptEventActionDefinitionException(nodeName, "Event type not provided");
                }

                node.EventType = exceptionOrEventType;
            }

            buildAction?.Invoke(new NodeBuilder(node, this, Services));

            node.Action.Actions.Add(async c =>
            {
                try
                {
                    var observer = c.Activity.GetExecutor().Inspector;
                    await observer.BeforeNodeExecuteAsync(c as ActionContext);
                    await actionAsync(c);
                    await observer.AfterNodeExecuteAsync(c as ActionContext);

                    c.Output(new ControlToken());
                }
                catch (Exception e)
                {
                    c.OutputRange(await node.HandleExceptionAsync(e, c as BaseContext));
                }
            }
            );

            Node.Nodes.Add(node.Identifier, node);
            Result.AllNodes.Add(node.Identifier, node);

            if (namedNodeTypes.Contains(type))
            {
                Node.NamedNodes.Add(node.Name, node);
                Result.AllNamedNodes.Add(node.Name, node);
            }

            return this;
        }

        public BaseActivityBuilder AddAction(string actionNodeName, ActionDelegateAsync actionAsync, NodeBuilderAction buildAction = null)
            => AddNode(NodeType.Action, actionNodeName, actionAsync, buildAction);

        public BaseActivityBuilder AddSendEventAction<TEvent>(
            string actionNodeName,
            SendEventActionDelegateAsync<TEvent> actionAsync,
            BehaviorIdSelectorAsync targetSelectorAsync,
            SendEventActionBuilderAction buildAction = null
        )
            where TEvent : Event, new()
            => AddAction(
                actionNodeName,
                async c =>
                {
                    var @event = await actionAsync(c);
                    var id = await targetSelectorAsync(c);
                    if (c.TryLocateBehavior(id, out var behavior))
                    {
                        await behavior.SendAsync(@event);
                    }
                },
                b => buildAction?.Invoke(b as ISendEventActionBuilder)
            );

        public BaseActivityBuilder AddAcceptEventAction<TEvent>(
            string actionNodeName,
            AcceptEventActionDelegateAsync<TEvent> actionAsync,
            AcceptEventActionBuilderAction buildAction = null
        )
            where TEvent : Event, new()
            => AddNode(
                NodeType.AcceptEventAction,
                actionNodeName,
                c => actionAsync(new AcceptEventActionContext<TEvent>(c as ActionContext)),
                b => buildAction?.Invoke(b),
                typeof(TEvent)
            );

        public BaseActivityBuilder AddInitial(InitialBuilderAction buildAction)
            => AddNode(
                NodeType.Initial,
                nameof(NodeType.Initial),
                c => Task.CompletedTask,
                b => buildAction(b)
            );

        public BaseActivityBuilder AddFinal()
            => AddNode(
                NodeType.Final,
                Final.Name,
                c =>
                {
                    (c as ActionContext).NodeScope.Terminate();
                    return Task.CompletedTask;
                },
                b => b.SetOptions(NodeOptions.ControlNodeDefault)
            );

        public BaseActivityBuilder AddInput(InputBuilderAction buildAction)
            => AddNode(
                NodeType.Input,
                nameof(NodeType.Input),
                c =>
                {
                    c.PassAllOn();
                    return Task.CompletedTask;
                },
                b => buildAction(b)
            );

        public BaseActivityBuilder AddOutput()
            => AddNode(
                NodeType.Output,
                Output.Name,
                c =>
                {
                    c.PassAllOn();
                    return Task.CompletedTask;
                },
                b => b.SetOptions(NodeOptions.None)
            );

        public BaseActivityBuilder AddStructuredActivity(string structuredActivityNodeName, ReactiveStructuredActivityBuilderAction builderAction = null)
            => AddNode(
                NodeType.StructuredActivity,
                structuredActivityNodeName,
                async c =>
                {
                    var executor = c.Activity.GetContext().Executor;
                    var node = c.GetNode();

                    await executor.DoInitializeNodeAsync(node, c as ActionContext);
                    (var output, var finalized) = await executor.DoExecuteStructuredNodeAsync(node, c.Activity.GetNodeScope(), c.Input);
                    c.OutputRange(output);
                    if (finalized)
                    {
                        await executor.DoFinalizeNodeAsync(node, c as ActionContext);
                    }
                },
                b => builderAction?.Invoke(new StructuredActivityBuilder(b.Node, this, Services))
            );

        public BaseActivityBuilder AddParallelActivity<TToken>(string parallelActivityNodeName, ParallelActivityBuilderAction builderAction = null)
            where TToken : Token, new()
            => AddNode(
                NodeType.ParallelActivity,
                parallelActivityNodeName,
                async c =>
                {
                    var executor = c.Activity.GetContext().Executor;
                    var node = c.GetNode();

                    await executor.DoInitializeNodeAsync(node, c as ActionContext);
                    c.OutputRange(await executor.DoExecuteParallelNodeAsync<TToken>(c as ActionContext));
                    await executor.DoFinalizeNodeAsync(node, c as ActionContext);
                },
                b => builderAction?.Invoke(new StructuredActivityBuilder(b.Node, this, Services))
            );

        public BaseActivityBuilder AddIterativeActivity<TToken>(string parallelActivityNodeName, IterativeActivityBuilderAction builderAction = null)
            where TToken : Token, new()
            => AddNode(
                NodeType.IterativeActivity,
                parallelActivityNodeName,
                async c =>
                {
                    var executor = c.Activity.GetContext().Executor;
                    var node = c.GetNode();

                    await executor.DoInitializeNodeAsync(node, c as ActionContext);
                    c.OutputRange(await executor.DoExecuteIterativeNodeAsync<TToken>(c as ActionContext));
                    await executor.DoFinalizeNodeAsync(node, c as ActionContext);
                },
                b => builderAction?.Invoke(new StructuredActivityBuilder(b.Node, this, Services))
            );

        public BaseActivityBuilder AddOnFinalize(Func<IActivityNodeContext, Task> actionAsync)
        {
            actionAsync.ThrowIfNull(nameof(actionAsync));

            Node.Finalize.Actions.Add(async c =>
            {
                var context = new ActivityNodeContext(c, Node);
                try
                {
                    await actionAsync(context);
                }
                catch (Exception e)
                {
                    await context.Context.Executor.Inspector.OnNodeFinalizationExceptionAsync(context, e);
                }
            });

            return this;
        }

        public BaseActivityBuilder AddOnInitialize(Func<IActivityNodeContext, Task> actionAsync)
        {
            actionAsync.ThrowIfNull(nameof(actionAsync));

            Node.Initialize.Actions.Add(async c =>
            {
                var context = new ActivityNodeContext(c, Node);
                try
                {
                    await actionAsync(context);
                }
                catch (Exception e)
                {
                    await context.Context.Executor.Inspector.OnNodeInitializationExceptionAsync(context, e);
                }
            });

            return this;
        }
    }
}
﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Exceptions;
using Stateflows.Activities.Enums;
using Stateflows.Activities.Models;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.Exceptions;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Activities.Registration.Builders;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.Activities.Registration.Interfaces.Internal;

namespace Stateflows.Activities.Registration
{
    internal class BaseActivityBuilder :
        IInternal,
        IGraphBuilder
    {
        public Graph Graph => Node as Graph ?? Node.Graph;

        public Node Node { get; set; }

        public IServiceCollection Services { get; }

        public BaseActivityBuilder(Node parentNode, IServiceCollection services)
        {
            Services = services;
            Node = parentNode;
        }

        [DebuggerHidden]
        public BaseActivityBuilder AddNode(NodeType type, string nodeName, Func<IActionContext, Task> actionAsync, NodeBuildAction buildAction = null, Type exceptionOrEventType = null, int chunkSize = 1)
        {
            var ownName = nodeName;
            if (Node.Type != NodeType.Activity)
            {
                nodeName = $"{Node.Name}.{nodeName}";
            }

            var namedNodeTypes = new NodeType[]
            {
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
                NodeType.TimeEventAction,
                NodeType.Output,
                NodeType.Final,
                NodeType.DataStore
            };

            var interactiveNodeTypes = new NodeType[]
            {
                NodeType.AcceptEventAction,
                NodeType.TimeEventAction
            };

            if (namedNodeTypes.Contains(type))
            {
                if (string.IsNullOrEmpty(nodeName))
                {
                    throw new NodeDefinitionException(nodeName, $"Node name cannot be empty", Graph.Class);
                }

                if (Graph.AllNamedNodes.ContainsKey(nodeName))
                {
                    throw new NodeDefinitionException(nodeName, $"Node '{nodeName}' is already registered", Graph.Class);
                }
            }

            if (interactiveNodeTypes.Contains(type))
            {
                Graph.Interactive = true;
            }

            var node = new Node()
            {
                Type = type,
                OwnName = ownName,
                Name = nodeName,
                Parent = Node,
                Graph = Graph,
                Level = Node.Level + 1,
                Anchored = type != NodeType.ParallelActivity && Node.Anchored,
                Identifier = !(Node is null)
                    ? $"{type}:{Node.Name}:{nodeName}"
                    : $"{type}:{nodeName}"
            };

            if (type == NodeType.ExceptionHandler)
            {
                if (exceptionOrEventType is null)
                {
                    throw new ExceptionHandlerDefinitionException(nodeName, "Exception type not provided", Graph.Class);
                }

                node.ExceptionType = exceptionOrEventType;
            }

            if (interactiveNodeTypes.Contains(type))
            {
                if (exceptionOrEventType is null)
                {
                    throw new AcceptEventActionDefinitionException(nodeName, "Event type not provided", Graph.Class);
                }

                node.EventType = exceptionOrEventType;
                node.ActualEventTypes = Graph.StateflowsBuilder.TypeMapper.GetMappedTypes(exceptionOrEventType).ToHashSet();
            }

            node.ChunkSize = chunkSize;

            buildAction?.Invoke(new NodeBuilder(node, this, Services));
            
            node.Action.Actions.Add(async c =>
            {
                var context = (ActionContext)c;
                var faulty = false;
                try
                {
                    var inspector = c.Behavior.GetExecutor().Inspector;
                    inspector.BeforeNodeExecute(context);
                    await actionAsync(c);
                    inspector.AfterNodeExecute(context);
                }
                catch (Exception e)
                {
                    if (e is StateflowsException)
                    {
                        throw;
                    }
                    else
                    {
                        var executor = context.Context.Executor;
                        var result = await executor.HandleExceptionAsync(node, e, context);
                        if (result == ExceptionHandlingResult.NotHandled)
                        {
                            faulty = true;
                            throw;
                        }
                        else
                        {
                            faulty = result == ExceptionHandlingResult.HandledIndirectly;
                        }
                    }
                }
                finally
                {
                    if (!faulty && !context.Context.Executor.StructuralTypes.Contains(node.Type))
                    {
                        c.Output(new ControlToken());
                    }
                }
            });

            Node.Nodes.Add(node.Identifier, node);
            Graph.AllNodes.Add(node.Identifier, node);

            if (namedNodeTypes.Contains(type))
            {
                Node.NamedNodes.Add(node.Name, node);
                Graph.AllNamedNodes.Add(node.Name, node);
            }
            
            Graph.VisitingTasks.Add(visitor => visitor.NodeAddedAsync(Graph.Name, Graph.Version, node.Name, node.Type));

            return this;
        }

        public BaseActivityBuilder AddAction(string actionNodeName, Func<IActionContext, Task> actionAsync, NodeBuildAction buildAction = null)
            => AddNode(NodeType.Action, actionNodeName, actionAsync, buildAction);

        public BaseActivityBuilder AddSendEventAction<TEvent>(
            string actionNodeName,
            SendEventActionDelegateAsync<TEvent> actionAsync,
            BehaviorIdSelectorAsync targetSelectorAsync,
            SendEventActionBuildAction buildAction = null
        )
        {
            var result = AddAction(
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
                b => buildAction?.Invoke(b)
            );

            var graph = ((IGraphBuilder)this).Graph;
            graph.VisitingTasks.Add(visitor => visitor.SendEventNodeAddedAsync<TEvent>(graph.Name, graph.Version, actionNodeName));

            return result;
        }

        public BaseActivityBuilder AddAcceptEventAction<TEvent>(
            string actionNodeName,
            AcceptEventActionDelegateAsync<TEvent> actionAsync,
            AcceptEventActionBuildAction buildAction = null
        )
        {
            var result = AddNode(
                NodeType.AcceptEventAction,
                actionNodeName,
                c => actionAsync(new AcceptEventActionContext<TEvent>(c as ActionContext)),
                b => buildAction?.Invoke(b),
                typeof(TEvent)
            );

            var graph = ((IGraphBuilder)this).Graph;
            graph.VisitingTasks.Add(visitor => visitor.AcceptEventNodeAddedAsync<TEvent>(graph.Name, graph.Version, actionNodeName));

            return result;
        }

        public BaseActivityBuilder AddTimeEventAction<TTimeEvent>(
            string actionNodeName,
            TimeEventActionDelegateAsync actionAsync,
            AcceptEventActionBuildAction buildAction = null
        )
            where TTimeEvent : TimeEvent, new()
            => AddNode(
                NodeType.TimeEventAction,
                actionNodeName,
                c => actionAsync(new AcceptEventActionContext<TTimeEvent>(c as ActionContext)),
                b => buildAction?.Invoke(b),
                typeof(TTimeEvent)
            );

        public BaseActivityBuilder AddInitial(InitialBuildAction buildAction)
            => AddNode(
                NodeType.Initial,
                $"{nameof(NodeType.Initial)}Node",
                c => Task.CompletedTask,
                b => buildAction(b)
            );

        public BaseActivityBuilder AddFinal()
            => AddNode(
                NodeType.Final,
                FinalNode.Name,
                c =>
                {
                    (c as ActionContext).NodeScope.Terminate();
                    return Task.CompletedTask;
                },
                b => b.SetOptions(NodeOptions.ControlNodeDefault)
            );

        public BaseActivityBuilder AddInput(InputBuildAction buildAction)
            => AddNode(
                NodeType.Input,
                $"{nameof(NodeType.Input)}Node",
                c =>
                {
                    c.PassAllTokensOn();
                    return Task.CompletedTask;
                },
                b => buildAction(b)
            );

        public BaseActivityBuilder AddOutput()
            => AddNode(
                NodeType.Output,
                OutputNode.Name,
                c =>
                {
                    c.PassAllTokensOn();
                    return Task.CompletedTask;
                },
                b => b.SetOptions(NodeOptions.None)
            );

        public BaseActivityBuilder AddStructuredActivity(string structuredActivityNodeName, ReactiveStructuredActivityBuildAction buildAction = null)
            => AddNode(
                NodeType.StructuredActivity,
                structuredActivityNodeName,
                async c =>
                {
                    var executor = c.Behavior.GetContext().Executor;
                    var node = c.GetNode();
                    var contextObj = c as ActionContext;

                    if (!contextObj.Context.NodesToExecute.Contains(node))
                    {
                        await executor.DoInitializeNodeAsync(node, c as ActionContext);
                    }

                    (var output, var finalized) = await executor.DoExecuteStructuredNodeAsync(node, c.Behavior.GetNodeScope(), contextObj.InputTokens);

                    contextObj.OutputTokens.AddRange(output);

                    if (finalized)
                    {
                        await executor.DoFinalizeNodeAsync(node, c as ActionContext);
                    }
                },
                b => buildAction?.Invoke(new StructuredActivityBuilder(b.Node, this, Services))
            );

        public BaseActivityBuilder AddParallelActivity<TToken>(string parallelActivityNodeName, ParallelActivityBuildAction buildAction = null, int chunkSize = 1)
            => AddNode(
                NodeType.ParallelActivity,
                parallelActivityNodeName,
                async c =>
                {
                    var executor = c.Behavior.GetContext().Executor;
                    var node = c.GetNode();

                    await executor.DoInitializeNodeAsync(node, c as ActionContext);
                    var edge = c.Node.TryGetCurrentFlow(out var currentFlow)
                        ? ((FlowContext)currentFlow).Edge
                        : null;
                    
                    c.OutputRange(await executor.DoExecuteParallelNodeAsync<TToken>(node, edge, c.Behavior.GetNodeScope(), ((ActionContext)c).InputTokens));
                    await executor.DoFinalizeNodeAsync(node, c as ActionContext);
                },
                b => buildAction?.Invoke(new StructuredActivityBuilder(b.Node, this, Services)),
                null,
                chunkSize
            );

        public BaseActivityBuilder AddIterativeActivity<TToken>(string parallelActivityNodeName, IterativeActivityBuildAction buildAction = null, int chunkSize = 1)
            => AddNode(
                NodeType.IterativeActivity,
                parallelActivityNodeName,
                async c =>
                {
                    var executor = c.Behavior.GetContext().Executor;
                    var node = c.GetNode();

                    await executor.DoInitializeNodeAsync(node, c as ActionContext);
                    c.OutputRange(await executor.DoExecuteIterativeNodeAsync<TToken>(c as ActionContext));
                    await executor.DoFinalizeNodeAsync(node, c as ActionContext);
                },
                b => buildAction?.Invoke(new StructuredActivityBuilder(b.Node, this, Services)),
                null,
                chunkSize
            );

        public BaseActivityBuilder AddOnFinalize(Func<IActivityNodeContext, Task> actionAsync)
        {
            actionAsync.ThrowIfNull(nameof(actionAsync));

            Node.Finalize.Actions.Add(async c =>
            {
                var context = new ActivityNodeContext(c, Node, c.NodeScope.Edge);
                try
                {
                    await actionAsync(context);
                }
                catch (Exception e)
                {
                    if (e is StateflowsDefinitionException)
                    {
                        throw;
                    }
                    else
                    {
                        var inspector = c.Context.Executor.Inspector;

                        Trace.WriteLine($"⦗→s⦘ Activity '{c.Context.Id.Name}:{c.Context.Id.Instance}': exception '{e.GetType().FullName}' thrown with message '{e.Message}'");
                        if (!inspector.OnNodeFinalizationException(context, e))
                        {
                            throw;
                        }
                        else
                        {
                            throw new BehaviorExecutionException(e);
                        }
                    }
                }
            });

            return this;
        }

        public BaseActivityBuilder AddOnInitialize(Func<IActivityNodeContext, Task> actionAsync)
        {
            actionAsync.ThrowIfNull(nameof(actionAsync));

            Node.Initialize.Actions.Add(async c =>
            {
                var context = new ActivityNodeContext(c, Node, c.NodeScope.Edge);
                try
                {
                    await actionAsync(context);
                }
                catch (Exception e)
                {
                    if (e is StateflowsDefinitionException)
                    {
                        throw;
                    }
                    else
                    {
                        var inspector = c.Context.Executor.Inspector;

                        Trace.WriteLine($"⦗→s⦘ Activity '{c.Context.Id.Name}:{c.Context.Id.Instance}': exception '{e.GetType().FullName}' thrown with message '{e.Message}'");
                        if (!inspector.OnNodeInitializationException(context, e))
                        {
                            throw;
                        }
                        else
                        {
                            throw new BehaviorExecutionException(e);
                        }
                    }
                }
            });

            return this;
        }
    }
}
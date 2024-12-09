using System;
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

        public BaseActivityBuilder AddNode(NodeType type, string nodeName, ActionDelegateAsync actionAsync, NodeBuildAction buildAction = null, Type exceptionOrEventType = null, int chunkSize = 1)
        {
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
                    throw new NodeDefinitionException(nodeName, $"Node name cannot be empty", Result.Class);
                }

                if (Result.AllNamedNodes.ContainsKey(nodeName))
                {
                    throw new NodeDefinitionException(nodeName, $"Node '{nodeName}' is already registered", Result.Class);
                }
            }

            if (interactiveNodeTypes.Contains(type))
            {
                Result.Interactive = true;
            }

            var node = new Node()
            {
                Type = type,
                Name = nodeName,
                Parent = Node,
                Graph = Result,
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
                    throw new ExceptionHandlerDefinitionException(nodeName, "Exception type not provided", Result.Class);
                }

                node.ExceptionType = exceptionOrEventType;
            }

            if (interactiveNodeTypes.Contains(type))
            {
                if (exceptionOrEventType is null)
                {
                    throw new AcceptEventActionDefinitionException(nodeName, "Event type not provided", Result.Class);
                }

                node.EventType = exceptionOrEventType;
                node.ActualEventTypes = Result.StateflowsBuilder.GetMappedTypes(exceptionOrEventType).ToHashSet();
            }

            node.ChunkSize = chunkSize;

            buildAction?.Invoke(new NodeBuilder(node, this, Services));

            node.Action.Actions.Add(async c =>
            {
                var context = c as ActionContext;
                var faulty = false;
                try
                {
                    var inspector = c.Activity.GetExecutor().Inspector;
                    await inspector.BeforeNodeExecuteAsync(context);
                    await actionAsync(c);
                    await inspector.AfterNodeExecuteAsync(context);
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
            Result.AllNodes.Add(node.Identifier, node);

            if (namedNodeTypes.Contains(type))
            {
                Node.NamedNodes.Add(node.Name, node);
                Result.AllNamedNodes.Add(node.Name, node);
            }

            return this;
        }

        public BaseActivityBuilder AddAction(string actionNodeName, ActionDelegateAsync actionAsync, NodeBuildAction buildAction = null)
            => AddNode(NodeType.Action, actionNodeName, actionAsync, buildAction);

        public BaseActivityBuilder AddSendEventAction<TEvent>(
            string actionNodeName,
            SendEventActionDelegateAsync<TEvent> actionAsync,
            BehaviorIdSelectorAsync targetSelectorAsync,
            SendEventActionBuildAction buildAction = null
        )

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
            AcceptEventActionBuildAction buildAction = null
        )

            => AddNode(
                NodeType.AcceptEventAction,
                actionNodeName,
                c => actionAsync(new AcceptEventActionContext<TEvent>(c as ActionContext)),
                b => buildAction?.Invoke(b),
                typeof(TEvent)
            );

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
                    var executor = c.Activity.GetContext().Executor;
                    var node = c.GetNode();
                    var contextObj = c as ActionContext;

                    if (!contextObj.Context.NodesToExecute.Contains(node))
                    {
                        await executor.DoInitializeNodeAsync(node, c as ActionContext);
                    }

                    (var output, var finalized) = await executor.DoExecuteStructuredNodeAsync(node, c.Activity.GetNodeScope(), contextObj.InputTokens);

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
                    var executor = c.Activity.GetContext().Executor;
                    var node = c.GetNode();

                    await executor.DoInitializeNodeAsync(node, c as ActionContext);
                    c.OutputRange(await executor.DoExecuteParallelNodeAsync<TToken>(node, c.Activity.GetNodeScope(), (c as ActionContext).InputTokens));
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
                    var executor = c.Activity.GetContext().Executor;
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
                var context = new ActivityNodeContext(c, Node);
                try
                {
                    await actionAsync(context);
                }
                catch (Exception e)
                {
                    if (e is StateflowsException)
                    {
                        throw;
                    }
                    else
                    {
                        if (!await context.Context.Executor.Inspector.OnNodeFinalizationExceptionAsync(context, e))
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
                var context = new ActivityNodeContext(c, Node);
                try
                {
                    await actionAsync(context);
                }
                catch (Exception e)
                {
                    if (e is StateflowsException)
                    {
                        throw;
                    }
                    else
                    {
                        if (!await context.Context.Executor.Inspector.OnNodeInitializationExceptionAsync(context, e))
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
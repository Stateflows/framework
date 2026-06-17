using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Common.Utilities;
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
    internal class BaseActivityBuilder : IGraphBuilder
    {
        public Graph Graph => Node as Graph ?? Node.Graph;

        internal Node Node { get; set; }

        internal BaseActivityBuilder(Node parentNode)
        {
            Node = parentNode;
        }

        [DebuggerHidden]
        internal BaseActivityBuilder AddNode(NodeType type, string nodeName, Func<IActionContext, Task> actionAsync, NodeBuildAction buildAction = null, Type exceptionOrEventType = null, int chunkSize = 1)
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
                NodeType.SendEventAction,
                NodeType.PublishEventAction,
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
                    : $"{type}:{nodeName}",
                EventType = exceptionOrEventType
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

            buildAction?.Invoke(new NodeBuilder(node, this));
            
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

        internal BaseActivityBuilder AddAction(string actionNodeName, Func<IActionContext, Task> actionAsync, NodeBuildAction buildAction = null, Type exceptionOrEventType = null)
            => AddNode(NodeType.Action, actionNodeName, actionAsync, buildAction, exceptionOrEventType);

        internal BaseActivityBuilder AddSendEventAction<TEvent>(
            string actionNodeName,
            SendEventActionDelegateAsync<TEvent> actionAsync,
            BehaviorIdSelectorAsync targetSelectorAsync,
            SendEventActionBuildAction buildAction = null
        )
        {
            var result = AddNode(
                NodeType.SendEventAction,
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
                b => buildAction?.Invoke(b),
                typeof(TEvent)
            );

            var graph = ((IGraphBuilder)this).Graph;
            graph.VisitingTasks.Add(visitor => visitor.SendEventNodeAddedAsync<TEvent>(graph.Name, graph.Version, actionNodeName));

            return result;
        }

        internal BaseActivityBuilder AddPublishEventAction<TEvent>(
            string actionNodeName,
            PublishEventActionDelegateAsync<TEvent> actionAsync,
            PublishEventActionBuildAction buildAction = null
        )
        {
            var result = AddNode(
                NodeType.PublishEventAction,
                actionNodeName,
                async c =>
                {
                    var @event = await actionAsync(c);
                    c.Behavior.Publish(@event);
                },
                b => buildAction?.Invoke(b),
                typeof(TEvent)
            );

            var graph = ((IGraphBuilder)this).Graph;
            graph.VisitingTasks.Add(visitor => visitor.SendEventNodeAddedAsync<TEvent>(graph.Name, graph.Version, actionNodeName));

            return result;
        }

        internal BaseActivityBuilder AddAcceptEventAction<TEvent>(
            string actionNodeName,
            AcceptEventActionDelegateAsync<TEvent> actionAsync,
            AcceptEventActionBuildAction<TEvent> buildAction = null
        )
        {
            var result = AddNode(
                NodeType.AcceptEventAction,
                actionNodeName,
                c => actionAsync(new AcceptEventActionContext<TEvent>(c as ActionContext)),
                b => buildAction?.Invoke(new AcceptEventNodeBuilder<TEvent>(b.Node, b.ActivityBuilder)),
                typeof(TEvent)
            );

            var graph = ((IGraphBuilder)this).Graph;
            graph.VisitingTasks.Add(visitor => visitor.AcceptEventNodeAddedAsync<TEvent>(graph.Name, graph.Version, actionNodeName));

            return result;
        }

        internal BaseActivityBuilder AddTimeEventAction<TTimeEvent>(
            string actionNodeName,
            TimeEventActionDelegateAsync actionAsync,
            TimeEventNodeBuildAction buildAction = null
        )
            where TTimeEvent : TimeEvent, new()
            => AddNode(
                NodeType.TimeEventAction,
                actionNodeName,
                c => actionAsync(new AcceptEventActionContext<TTimeEvent>(c as ActionContext)),
                b => buildAction?.Invoke(b),
                typeof(TTimeEvent)
            );

        internal BaseActivityBuilder AddInitial(InitialBuildAction buildAction)
            => AddNode(
                NodeType.Initial,
                $"{nameof(NodeType.Initial)}Node",
                c => Task.CompletedTask,
                b => buildAction(b)
            );

        internal BaseActivityBuilder AddFinal()
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

        internal BaseActivityBuilder AddInput(InputBuildAction buildAction)
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

        internal BaseActivityBuilder AddOutput()
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

        internal BaseActivityBuilder AddStructuredActivity(string structuredActivityNodeName, ReactiveStructuredActivityBuildAction buildAction = null)
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
                b => buildAction?.Invoke(new StructuredActivityBuilder(b.Node, this))
            );

        internal BaseActivityBuilder AddParallelActivity<TToken>(string parallelActivityNodeName, ParallelActivityBuildAction buildAction = null, int chunkSize = 1)
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

                    var outputTokenHolders = await executor.DoExecuteParallelNodeAsync<TToken>(
                        node,
                        edge,
                        c.Behavior.GetNodeScope(),
                        ((ActionContext)c).InputTokens
                    );
                    ((ActionContext)c).OutputTokens.AddRange(outputTokenHolders);
                    await executor.DoFinalizeNodeAsync(node, c as ActionContext);
                },
                b => buildAction?.Invoke(new StructuredActivityBuilder(b.Node, this)),
                typeof(TToken),
                chunkSize
            );

        internal BaseActivityBuilder AddIterativeActivity<TToken>(string parallelActivityNodeName, IterativeActivityBuildAction buildAction = null, int chunkSize = 1)
            => AddNode(
                NodeType.IterativeActivity,
                parallelActivityNodeName,
                async c =>
                {
                    var executor = c.Behavior.GetContext().Executor;
                    var node = c.GetNode();

                    await executor.DoInitializeNodeAsync(node, c as ActionContext);
                    var outputTokenHolders = await executor.DoExecuteIterativeNodeAsync<TToken>(c as ActionContext);
                    ((ActionContext)c).OutputTokens.AddRange(outputTokenHolders);
                    await executor.DoFinalizeNodeAsync(node, c as ActionContext);
                },
                b => buildAction?.Invoke(new StructuredActivityBuilder(b.Node, this)),
                typeof(TToken),
                chunkSize
            );

        internal BaseActivityBuilder AddOnFinalize(Func<IActivityNodeContext, Task> actionAsync)
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

        internal BaseActivityBuilder AddOnInitialize(Func<IActivityNodeContext, Task> actionAsync)
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
        public BaseActivityBuilder UseAction(string actionNodeName, OverridenActionBuildAction buildAction = null)
        {
            var fullActionNodeName = Node.Type != NodeType.Activity
                ? $"{Node.Name}.{actionNodeName}"
                : actionNodeName;
            
            if (
                !Node.Nodes.TryGetValue($"{NodeType.Action}:{Node.Name}:{fullActionNodeName}", out var node) ||
                node.Type != NodeType.Action || 
                node.OriginActivityName == null
            )
            {
                throw new ActivityOverrideException($"Action '{actionNodeName}' not found in overriden activity", Graph.Class);
            }
            
            buildAction?.Invoke(new NodeBuilder(node, this));

            return this;
        }

        internal BaseActivityBuilder UseStructuredActivity(string structuredActivityNodeName, OverridenStructuredActivityBuildAction buildAction)
        {
            if (
                !Node.Nodes.TryGetValue($"{NodeType.StructuredActivity}:{Node.Name}:{structuredActivityNodeName}", out var node) ||
                node.Type != NodeType.StructuredActivity || 
                node.OriginActivityName == null
            )
            {
                throw new ActivityOverrideException($"Structured activity '{structuredActivityNodeName}' not found in overriden activity", Graph.Class);
            }
            
            buildAction?.Invoke(new StructuredActivityBuilder(node, this));

            return this;
        }
        
        internal BaseActivityBuilder UseParallelActivity<TParallelizationToken>(string parallelActivityNodeName,
            OverridenParallelActivityBuildAction buildAction)
        {
            if (
                !Node.Nodes.TryGetValue($"{NodeType.ParallelActivity}:{Node.Name}:{parallelActivityNodeName}", out var node) ||
                node.Type != NodeType.ParallelActivity ||
                node.EventType != typeof(TParallelizationToken) ||
                node.OriginActivityName == null
            )
            {
                throw new ActivityOverrideException($"Parallel activity '{parallelActivityNodeName}' not found in overriden activity", Graph.Class);
            }
            
            buildAction?.Invoke(new StructuredActivityBuilder(node, this));

            return this;
        }

        internal BaseActivityBuilder UseIterativeActivity<TIterationToken>(string iterativeActivityNodeName, OverridenIterativeActivityBuildAction buildAction)
        {
            if (
                !Node.Nodes.TryGetValue($"{NodeType.IterativeActivity}:{Node.Name}:{iterativeActivityNodeName}", out var node) ||
                node.Type != NodeType.IterativeActivity ||
                node.EventType != typeof(TIterationToken) ||
                node.OriginActivityName == null
            )
            {
                throw new ActivityOverrideException($"Parallel activity '{iterativeActivityNodeName}' not found in overriden activity", Graph.Class);
            }
            
            buildAction?.Invoke(new StructuredActivityBuilder(node, this));

            return this;
        }
        
        internal BaseActivityBuilder UseInitial(OverridenInitialBuildAction buildAction)
        {
            var node = Node.Nodes.Values.FirstOrDefault(node => node.Type == NodeType.Initial);
            if (node?.OriginActivityName == null)
            {
                throw new ActivityOverrideException($"Initial not found in overriden activity", Graph.Class);
            }
            
            buildAction?.Invoke(new NodeBuilder(node, this));

            return this;
        }

        internal BaseActivityBuilder UseInput(OverridenInputBuildAction buildAction)
        {
            var node = Node.Nodes.Values.FirstOrDefault(node => node.Type == NodeType.Input);
            if (node?.OriginActivityName == null)
            {
                throw new ActivityOverrideException($"Input not found in overriden activity", Graph.Class);
            }
            
            buildAction?.Invoke(new NodeBuilder(node, this));

            return this;
        }

        internal BaseActivityBuilder UseAcceptEventAction<TEvent>(string actionNodeName,
            OverridenAcceptEventActionBuildAction<TEvent> buildAction)
        {
            if (
                !Node.Nodes.TryGetValue($"{NodeType.AcceptEventAction}:{Node.Name}:{actionNodeName}", out var node) ||
                node.Type != NodeType.AcceptEventAction ||
                node.EventType != typeof(TEvent) ||
                node.OriginActivityName == null
            )
            {
                throw new ActivityOverrideException($"Accept event action '{actionNodeName}' not found in overriden activity", Graph.Class);
            }
            
            buildAction?.Invoke(new AcceptEventNodeBuilder<TEvent>(node, this));

            return this;
        }

        internal BaseActivityBuilder UseTimeEventAction<TTimeEvent>(string actionNodeName,
            OverridenTimeEventNodeBuildAction buildAction) where TTimeEvent : TimeEvent, new()
        {
            if (
                !Node.Nodes.TryGetValue($"{NodeType.TimeEventAction}:{Node.Name}:{actionNodeName}", out var node) ||
                node.Type != NodeType.TimeEventAction ||
                node.EventType != typeof(TTimeEvent) ||
                node.OriginActivityName == null
            )
            {
                throw new ActivityOverrideException($"Time event action '{actionNodeName}' not found in overriden activity", Graph.Class);
            }
            
            buildAction?.Invoke(new NodeBuilder(node, this));

            return this;
        }

        internal BaseActivityBuilder UseJoin(string joinNodeName, OverridenJoinBuildAction buildAction)
        {
            if (
                !Node.Nodes.TryGetValue($"{NodeType.Join}:{Node.Name}:{joinNodeName}", out var node) ||
                node.Type != NodeType.Join ||
                node.OriginActivityName == null
            )
            {
                throw new ActivityOverrideException($"Join '{joinNodeName}' not found in overriden activity", Graph.Class);
            }
            
            buildAction?.Invoke(new NodeBuilder(node, this));

            return this;
        }

        internal BaseActivityBuilder UseFork(string forkNodeName, OverridenForkBuildAction buildAction)
        {
            if (
                !Node.Nodes.TryGetValue($"{NodeType.Fork}:{Node.Name}:{forkNodeName}", out var node) ||
                node.Type != NodeType.Fork ||
                node.OriginActivityName == null
            )
            {
                throw new ActivityOverrideException($"Fork '{forkNodeName}' not found in overriden activity", Graph.Class);
            }
            
            buildAction?.Invoke(new NodeBuilder(node, this));

            return this;
        }

        internal BaseActivityBuilder UseMerge(string mergeNodeName, OverridenMergeBuildAction buildAction)
        {
            if (
                !Node.Nodes.TryGetValue($"{NodeType.Merge}:{Node.Name}:{mergeNodeName}", out var node) ||
                node.Type != NodeType.Merge ||
                node.OriginActivityName == null
            )
            {
                throw new ActivityOverrideException($"Merge '{mergeNodeName}' not found in overriden activity", Graph.Class);
            }
            
            buildAction?.Invoke(new NodeBuilder(node, this));

            return this;
        }

        internal BaseActivityBuilder UseControlDecision(string decisionNodeName, OverridenDecisionBuildAction buildAction)
        {
            if (
                !Node.Nodes.TryGetValue($"{NodeType.Decision}:{Node.Name}:{decisionNodeName}", out var node) ||
                node.Type != NodeType.Decision ||
                node.EventType != typeof(ControlToken) ||
                node.OriginActivityName == null
            )
            {
                throw new ActivityOverrideException($"Control decision '{decisionNodeName}' not found in overriden activity", Graph.Class);
            }
            
            buildAction?.Invoke(new NodeBuilder(node, this));

            return this;
        }

        internal BaseActivityBuilder UseDecision<TToken>(string decisionNodeName, OverridenDecisionBuildAction<TToken> decisionBuildAction)
        {
            if (
                !Node.Nodes.TryGetValue($"{NodeType.Decision}:{Node.Name}:{decisionNodeName}", out var node) ||
                node.Type != NodeType.Decision ||
                node.EventType != typeof(TToken) ||
                node.OriginActivityName == null
            )
            {
                throw new ActivityOverrideException($"Decision '{decisionNodeName}' not found in overriden activity", Graph.Class);
            }
            
            decisionBuildAction?.Invoke(new DecisionBuilder<TToken>(new NodeBuilder(node, this)));

            return this;
        }

        internal BaseActivityBuilder UseDataStore(string dataStoreNodeName, OverridenDataStoreBuildAction buildAction)
        {
            if (
                !Node.Nodes.TryGetValue($"{NodeType.DataStore}:{Node.Name}:{dataStoreNodeName}", out var node) ||
                node.Type != NodeType.DataStore ||
                node.OriginActivityName == null
            )
            {
                throw new ActivityOverrideException($"Data store '{dataStoreNodeName}' not found in overriden activity", Graph.Class);
            }
            
            buildAction?.Invoke(new NodeBuilder(node, this));

            return this;
        }

        internal BaseActivityBuilder UseSendEventAction<TEvent>(string actionNodeName, OverridenSendEventActionBuildAction buildAction)
        {
            if (
                !Node.Nodes.TryGetValue($"{NodeType.SendEventAction}:{Node.Name}:{actionNodeName}", out var node) ||
                node.Type != NodeType.SendEventAction ||
                node.EventType != typeof(TEvent) ||
                node.OriginActivityName == null
            )
            {
                throw new ActivityOverrideException($"Send event action '{actionNodeName}' not found in overriden activity", Graph.Class);
            }
            
            buildAction?.Invoke(new NodeBuilder(node, this));

            return this;
        }

        internal BaseActivityBuilder UsePublishEventAction<TEvent>(string actionNodeName, OverridenPublishEventActionBuildAction buildAction)
        {
            if (
                !Node.Nodes.TryGetValue($"{NodeType.PublishEventAction}:{Node.Name}:{actionNodeName}", out var node) ||
                node.Type != NodeType.PublishEventAction ||
                node.EventType != typeof(TEvent) ||
                node.OriginActivityName == null
            )
            {
                throw new ActivityOverrideException($"Publish event action '{actionNodeName}' not found in overriden activity", Graph.Class);
            }
            
            buildAction?.Invoke(new NodeBuilder(node, this));

            return this;
        }
    }
}
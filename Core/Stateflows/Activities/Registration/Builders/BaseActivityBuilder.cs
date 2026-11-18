using System;
using System.Collections.Generic;
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
using Stateflows.Activities.Registration.Interfaces.Base;
using Stateflows.Entities.Enums;

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
                NodeType.OutputPin,
                NodeType.Final,
                NodeType.DataStore
            };

            var interactiveNodeTypes = new NodeType[]
            {
                NodeType.AcceptEventAction,
                NodeType.TimeEventAction
            };

            if (Graph.AllNodes.Values.Any(existing => existing.Name == nodeName))
            {
                throw new NodeDefinitionException(nodeName, $"Node '{nodeName}' is already registered", Graph.Class);
            }

            if (namedNodeTypes.Contains(type))
            {
                if (string.IsNullOrEmpty(nodeName))
                {
                    throw new NodeDefinitionException(nodeName, $"Node name cannot be empty", Graph.Class);
                }
            }

            if (interactiveNodeTypes.Contains(type))
            {
                Graph.Interactive = true;
            }

            var identifier = $"{type}:{Node.Name}:{nodeName}";
            if (Node.Nodes.ContainsKey(identifier))
            {
                throw new NodeDefinitionException(nodeName, $"Node '{nodeName}' is already registered in this scope", Graph.Class);
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
                Identifier = identifier,
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
            
            var actionOrder = node.Action.Actions.Count;
            node.ObservedActionBodies.Add(actionOrder);
            node.Action.Actions.Add(async c =>
            {
                var context = (ActionContext)c;
                var faulty = false;
                try
                {
                    var inspector = c.Behavior.GetExecutor().Inspector;
                    await inspector.OnNodeExecuteAsync(context, () => inspector.OnActionAsync(
                        new ActionObservationContext(context, context.Context.Executor.Graph.OperationRegistrations[node.Action][actionOrder]),
                        () => actionAsync(c)));
                }
                catch (Stateflows.Activities.Streams.ActivityCheckpointPauseException)
                {
                    // A suspended structural invocation is not a failure; its continuation is recorded by the engine.
                    faulty = true;
                    throw;
                }
                catch (Exception e)
                {
                    if (e is StateflowsException ||
                        context.Context.Context.ExecutionExceptions.Any(exception => ReferenceEquals(exception, e)))
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
                    if (!faulty && !context.Context.Executor.PayloadTypes.Contains(node.Type))
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

            return result;
        }

        internal BaseActivityBuilder AddPublishEventAction<TEvent>(
            string actionNodeName,
            PublishEventActionDelegateAsync<TEvent> actionAsync,
            PublishEventActionBuildAction buildAction = null
        )
            => AddPublishEventAction<TEvent>(actionNodeName,
                async c => new[] { await actionAsync(c) }.AsEnumerable(), buildAction);

        internal BaseActivityBuilder AddPublishEventAction<TEvent>(
            string actionNodeName,
            Func<IActionContext, Task<IEnumerable<TEvent>>> actionAsync,
            PublishEventActionBuildAction buildAction = null)
        {
            Node publicationNode = null;
            var result = AddNode(
                NodeType.PublishEventAction,
                actionNodeName,
                async c =>
                {
                    var events = (await actionAsync(c)).ToArray();
                    var headers = new Dictionary<string, EventHeader>
                    {
                        [nameof(TimeToLive)] = new TimeToLive(publicationNode.TimeToLive)
                    };
                    if (publicationNode.Retain)
                        headers[nameof(Retain)] = new Retain();
                    if (publicationNode.ImmediatePublish)
                        headers[nameof(ImmediatePublish)] = new ImmediatePublish();
                    if (publicationNode.PublishScope.HasFlag(PublishScope.Self))
                        c.Behavior.PublishRange(events, headers);
                    if (publicationNode.PublishScope.HasFlag(PublishScope.Parent) &&
                        c.TryGetParentBehaviorContext(out var parent))
                        parent.PublishRange(events, headers);
                    if (publicationNode.PublishScope.HasFlag(PublishScope.Owner) &&
                        c.TryGetOwnerBehaviorContext(out var owner))
                        owner.PublishRange(events, headers);
                },
                b =>
                {
                    publicationNode = b.Node;
                    buildAction?.Invoke(b);
                },
                typeof(TEvent)
            );

            var graph = ((IGraphBuilder)this).Graph;

            return result;
        }

        internal BaseActivityBuilder AddAcceptEventAction<TEvent>(
            string actionNodeName,
            AcceptEventActionDelegateAsync<TEvent> actionAsync,
            AcceptEventActionBuildAction<TEvent> buildAction = null,
            IReadOnlyList<string> payloadNames = null
        )
        {
            var result = AddNode(
                NodeType.AcceptEventAction,
                actionNodeName,
                c => actionAsync(new AcceptEventActionContext<TEvent>(c as ActionContext)),
                b =>
                {
                    b.Node.PayloadNames = payloadNames;
                    buildAction?.Invoke(new AcceptEventNodeBuilder<TEvent>(b.Node, b.ActivityBuilder));
                },
                typeof(TEvent)
            );


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
                $"{Node.Name}.initial",
                c => Task.CompletedTask,
                b => buildAction(b)
            );

        internal BaseActivityBuilder AddFinal(string name = null)
            => AddNode(
                NodeType.Final,
                name ?? FinalNode.Name,
                c =>
                {
                    (c as ActionContext).NodeScope.Terminate();
                    return Task.CompletedTask;
                },
                b => b.SetOptions(NodeOptions.ControlNodeDefault)
            );

        /// <summary>
        /// Takes the payload token names supplied by a name-first overload (see <see cref="PayloadFlowScope"/>). Must be
        /// called before any nested build action runs. A payload type without names is rejected (V4).
        /// </summary>
        internal IReadOnlyList<string> TakeTokenNames<TToken>(string declaration)
        {
            var names = PayloadFlowScope.Take();
            if (names == null && Stateflows.Payloads.PayloadNameSet.IsPayloadType(typeof(TToken)))
            {
                throw new ActivityDefinitionException(
                    $"{declaration} of '{typeof(TToken).Name}' requires payload token names; use the overload that takes token names.",
                    Graph.Class);
            }
            if (names != null && !Stateflows.Payloads.PayloadNameSet.IsPayloadType(typeof(TToken)))
            {
                throw new ActivityDefinitionException(
                    $"{declaration} of '{typeof(TToken).Name}' cannot declare payload token names; use a payload token declaration.",
                    Graph.Class);
            }

            return names;
        }

        private static Func<IActionContext, Task> PassTokens<TToken>(IReadOnlyList<string> names)
        {
            if (names == null)
            {
                return c =>
                {
                    c.PassTokensOfTypeOn<TToken>();
                    return Task.CompletedTask;
                };
            }

            var key = TokenKey.OfNames(names);
            return c =>
            {
                ((ActionContext)c).PassTokensOn(key);
                return Task.CompletedTask;
            };
        }

        internal BaseActivityBuilder AddInputPin<TToken>(System.Action<IInputPinBuilder<TToken>> buildAction)
        {
            var names = TakeTokenNames<TToken>("Input pin");
            ArgumentNullException.ThrowIfNull(buildAction);
            var pinName = names == null ? InputPin<TToken>.Name : InputPin.GetName(names);
            AddNode(NodeType.InputPin, $"{Node.Name}.{pinName}", PassTokens<TToken>(names), builder =>
            {
                builder.Node.PinTokenType = typeof(TToken);
                builder.Node.PinTokenNames = names;
                buildAction(new InputPinBuilder<TToken>(builder.Node, this));
            });
            return this;
        }

        internal BaseActivityBuilder AddOutputPin<TToken>(string name = null)
        {
            var names = TakeTokenNames<TToken>("Output pin");
            AddNode(NodeType.OutputPin, name ?? (names == null ? OutputPin<TToken>.Name : OutputPin.GetName(names)), PassTokens<TToken>(names), builder =>
            {
                builder.Node.PinTokenType = typeof(TToken);
                builder.Node.PinTokenNames = names;
                builder.SetOptions(NodeOptions.None);
            });
            return this;
        }

        internal BaseActivityBuilder AddRootOutputPin<TToken>(string name, PublishEventActionBuildAction buildAction)
        {
            var names = TakeTokenNames<TToken>("Output pin");
            var pinName = name ?? (names == null ? OutputPin<TToken>.Name : OutputPin.GetName(names));
            ((IPublishEventBase<IActivityBuilder>)this)
                .AddPublishEventAction<TToken, OutputPinNode<TToken>>(pinName, builder =>
                {
                    var node = ((NodeBuilder)builder).Node;
                    node.PinTokenType = typeof(TToken);
                    node.PinTokenNames = names;
                    node.Options = NodeOptions.None;
                    buildAction?.Invoke(builder);
                });
            var pin = Node.OutputPins.Single(n => n.Name == pinName);
            pin.ImplementationType = null;
            pin.ImplementationEventType = null;
            pin.OperationTypes.Remove((ActivityOperationKind.Execute, pin.Action.Actions.Count - 1));
            if (typeof(TToken) == typeof(bool))
                pin.Action.Actions.Add(c =>
                {
                    var context = (ActionContext)c;
                    if (context.Context.EventHolder?.Headers.Values.OfType<TransitionGuardRequest>().Any() == true)
                    {
                        // Embedded guard results also return through the delegation protocol.
                        var output = context.Context.GetOutputTokens(pin.Identifier, context.NodeScope.ThreadId);
                        lock (output)
                            output.AddRange(context.InputTokens.Where(token => token.PayloadType == typeof(bool)));
                    }
                    return Task.CompletedTask;
                });
            return this;
        }

        internal BaseActivityBuilder AddStructuredActivity(string structuredActivityNodeName, ReactiveStructuredActivityBuildAction buildAction = null)
            => AddNode(
                NodeType.StructuredActivity,
                structuredActivityNodeName,
                c => c.Behavior.GetContext().Executor.ExecuteStructuredActivityAsync((ActionContext)c),
                b => buildAction?.Invoke(new StructuredActivityBuilder(b.Node, this))
            );

        internal BaseActivityBuilder AddParallelActivity<TToken>(string parallelActivityNodeName, ParallelActivityBuildAction buildAction = null, int chunkSize = 1, int maxConcurrency = -1)
        {
            var names = TakeTokenNames<TToken>("Parallel activity");
            return AddNode(
                NodeType.ParallelActivity,
                parallelActivityNodeName,
                c => c.Behavior.GetContext().Executor.ExecuteParallelActivityAsync<TToken>((ActionContext)c),
                b =>
                {
                    b.Node.TokenNames = names;
                    b.Node.MaxConcurrency = maxConcurrency;
                    buildAction?.Invoke(new StructuredActivityBuilder(b.Node, this));
                },
                typeof(TToken),
                chunkSize
            );
        }

        internal BaseActivityBuilder AddIterativeActivity<TToken>(string parallelActivityNodeName, IterativeActivityBuildAction buildAction = null, int chunkSize = 1)
        {
            var names = TakeTokenNames<TToken>("Iterative activity");
            return AddNode(
                NodeType.IterativeActivity,
                parallelActivityNodeName,
                c => c.Behavior.GetContext().Executor.ExecuteIterativeActivityAsync<TToken>((ActionContext)c),
                b =>
                {
                    b.Node.TokenNames = names;
                    buildAction?.Invoke(new StructuredActivityBuilder(b.Node, this));
                },
                typeof(TToken),
                chunkSize
            );
        }

        internal BaseActivityBuilder AddDecision<TToken>(string decisionNodeName, DecisionBuildAction<TToken> decisionBuildAction)
        {
            var names = TakeTokenNames<TToken>("Decision");
            return AddNode(
                NodeType.Decision,
                decisionNodeName,
                c =>
                {
                    c.PassAllTokensOn();
                    return Task.CompletedTask;
                },
                b =>
                {
                    b.Node.TokenNames = names;
                    decisionBuildAction(new DecisionBuilder<TToken>(b.SetOptions(NodeOptions.DecisionDefault) as NodeBuilder));
                },
                typeof(TToken)
            );
        }

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
                        await inspector.OnNodeFinalizationExceptionAsync(context, e);
                        throw;
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
                        await inspector.OnNodeInitializationExceptionAsync(context, e);
                        throw;
                    }
                }
            });

            return this;
        }
        public BaseActivityBuilder UseAction(string actionNodeName, OverridenActionBuildAction buildAction = null)
        {
            if (
                !Node.Nodes.TryGetValue($"{NodeType.Action}:{Node.Name}:{actionNodeName}", out var node) ||
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
            var names = TakeTokenNames<TParallelizationToken>("Parallel activity");
            if (
                !Node.Nodes.TryGetValue($"{NodeType.ParallelActivity}:{Node.Name}:{parallelActivityNodeName}", out var node) ||
                node.Type != NodeType.ParallelActivity ||
                !Equals(node.SelectionKey, TokenKey.Of(typeof(TParallelizationToken), names)) ||
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
            var names = TakeTokenNames<TIterationToken>("Iterative activity");
            if (
                !Node.Nodes.TryGetValue($"{NodeType.IterativeActivity}:{Node.Name}:{iterativeActivityNodeName}", out var node) ||
                node.Type != NodeType.IterativeActivity ||
                !Equals(node.SelectionKey, TokenKey.Of(typeof(TIterationToken), names)) ||
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

        internal BaseActivityBuilder UseInputPin<TToken>(System.Action<IOverridenInputPinBuilder<TToken>> buildAction)
        {
            var names = TakeTokenNames<TToken>("Input pin");
            var key = TokenKey.Of(typeof(TToken), names);
            var node = Node.InputPins.FirstOrDefault(pin => key.Equals(pin.PinKey));
            if (node?.OriginActivityName == null)
            {
                throw new ActivityOverrideException($"Input pin '{(names == null ? InputPin<TToken>.Name : InputPin.GetName(names))}' not found in overriden activity", Graph.Class);
            }
            
            buildAction?.Invoke(new InputPinBuilder<TToken>(node, this));

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
            var names = TakeTokenNames<TToken>("Decision");
            if (
                !Node.Nodes.TryGetValue($"{NodeType.Decision}:{Node.Name}:{decisionNodeName}", out var node) ||
                node.Type != NodeType.Decision ||
                !Equals(node.SelectionKey, TokenKey.Of(typeof(TToken), names)) ||
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
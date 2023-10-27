using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Activities.Models;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.Exceptions;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.Activities.Registration.Builders;

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

        public BaseActivityBuilder AddNode(NodeType type, string nodeName, ActionDelegateAsync actionAsync, NodeBuilderAction buildAction = null, Type exceptionType = null)
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
                NodeType.Output,
                NodeType.Final
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
            };

            if (type == NodeType.ExceptionHandler)
            {
                if (exceptionType is null)
                {
                    throw new ExceptionHandlerDefinitionException(nodeName, "Exception type not provided");
                }

                node.ExceptionType = exceptionType;
            }

            buildAction?.Invoke(new NodeBuilder(node, this, Services));

            node.Action.Actions.Add(async c =>
            {
                try
                {
                    await actionAsync(c);

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

        public BaseActivityBuilder AddSignalAction<TEvent>(
            string signalActionNodeName,
            SignalActionDelegateAsync<TEvent> actionAsync,
            BehaviorIdSelectorAsync targetSelectorAsync,
            SignalActionBuilderAction signalActionBuildAction = null
        )
            where TEvent : Event, new()
            => AddAction(
                signalActionNodeName,
                async c =>
                {
                    var @event = await actionAsync(c);
                    var id = await targetSelectorAsync(c);
                    if (c.TryLocateBehavior(id, out var behavior))
                    {
                        await behavior.SendAsync(@event);
                    }
                },
                b => signalActionBuildAction(b as ISignalActionBuilder)
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
                    c.GetContext().Executor.Cancel();
                    return Task.CompletedTask;
                }
            );

        public BaseActivityBuilder AddInput(InputBuilderAction buildAction)
            => AddNode(
                NodeType.Input,
                nameof(NodeType.Input),
                c =>
                {
                    c.PassAll();
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
                    c.PassAll();
                    return Task.CompletedTask;
                },
                b => b.SetOptions(NodeOptions.None)
            );

        public BaseActivityBuilder AddStructuredActivity(string structuredActivityNodeName, StructuredActivityBuilderAction builderAction = null)
            => AddNode(
                NodeType.StructuredActivity,
                structuredActivityNodeName,
                async c => c.OutputRange(await c.GetContext().Executor.DoInitializeStructuredNodeAsync(c.GetNode(), c.Input)),
                b => builderAction?.Invoke(new StructuredActivityBuilder(b.Node, this, Services))
            );

        public BaseActivityBuilder AddParallelActivity<TToken>(string parallelActivityNodeName, StructuredActivityBuilderAction builderAction = null)
            where TToken : Token, new()
            => AddNode(
                NodeType.ParallelActivity,
                parallelActivityNodeName,
                async c => c.OutputRange(await c.GetContext().Executor.DoInitializeParallelNodeAsync<TToken>(c.GetNode(), c.Input)),
                b => builderAction?.Invoke(new StructuredActivityBuilder(b.Node, this, Services))
            );

        public BaseActivityBuilder AddIterativeActivity<TToken>(string parallelActivityNodeName, StructuredActivityBuilderAction builderAction = null)
            where TToken : Token, new()
            => AddNode(
                NodeType.IterativeActivity,
                parallelActivityNodeName,
                async c => c.OutputRange(await c.GetContext().Executor.DoInitializeIterativeNodeAsync<TToken>(c.GetNode(), c.Input)),
                b => builderAction?.Invoke(new StructuredActivityBuilder(b.Node, this, Services))
            );

        public BaseActivityBuilder AddOnFinalize(Func<IActivityActionContext, Task> actionAsync)
        {
            actionAsync.ThrowIfNull(nameof(actionAsync));

            Node.Finalize.Actions.Add(async c =>
            {
                var context = new ActivityActionContext(c.Context, c.NodeScope);
                try
                {
                    await actionAsync(context);
                }
                catch (Exception e)
                {
                    //await c.Executor.Observer.OnActivityFinalizeExceptionAsync(context, e);
                }
            });

            return this;
        }

        //public BaseActivityBuilder AddExceptionHandler<TException>(ExceptionHandlerDelegateAsync<TException> exceptionHandler)
        //    where TException : Exception
        //{
        //    var targetNodeName = $"{typeof(TException).FullName}.ExceptionHandler";

        //    AddNode(
        //        NodeType.ExceptionHandler,
        //        targetNodeName,
        //        c =>
        //        {
        //            var contextObj = c as ActionContext;
        //            var context = new ExceptionHandlerContext<TException>(contextObj.Context, contextObj.Node, Node, contextObj.Input);

        //            exceptionHandler?.Invoke(context);

        //            c.OutputRange(context.OutputTokens);

        //            return Task.CompletedTask;
        //        },
        //        null,
        //        typeof(TException)
        //    );

        //    return this;
        //}
    }
}
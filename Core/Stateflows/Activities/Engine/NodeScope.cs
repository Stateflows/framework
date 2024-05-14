using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Activities.Models;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Common.Classes;

namespace Stateflows.Activities.Engine
{
    internal class NodeScope : IDisposable
    {
        public readonly NodeScope BaseNodeScope = null;

        private IServiceProvider baseServiceProvider = null;
        private IServiceProvider BaseServiceProvider
            => baseServiceProvider ??= BaseNodeScope.ServiceProvider;

        private IServiceScope scope = null;
        private IServiceScope Scope
            => scope ??= BaseServiceProvider.CreateScope();

        public IServiceProvider ServiceProvider
            => Scope.ServiceProvider;

        private CancellationTokenSource cancellationTokenSource = null;
        private CancellationTokenSource CancellationTokenSource
            => cancellationTokenSource ??= BaseNodeScope != null
                ? CancellationTokenSource.CreateLinkedTokenSource(BaseNodeScope.CancellationToken)
                : new CancellationTokenSource();

        public CancellationToken CancellationToken
            => CancellationTokenSource.Token;

        public Guid ThreadId { get; set; }

        public bool IsTerminated { get; private set; }

        public Node Node { get; private set; }

        public void Terminate()
        {
            CancellationTokenSource.Cancel();
            var currentScope = this;
            do
            {
                currentScope.IsTerminated = true;
                currentScope = currentScope.BaseNodeScope;
                if (currentScope.Node == Node.Parent)
                {
                    break;
                }
            } while (currentScope != null);
        }

        public NodeScope(IServiceProvider serviceProvider, Node node, Guid threadId)
        {
            baseServiceProvider = serviceProvider;
            Node = node;
            ThreadId = threadId;
        }

        private NodeScope(NodeScope nodeScope, Node node, Guid threadId)
        {
            BaseNodeScope = nodeScope;
            Node = node;
            ThreadId = threadId;
        }

        public NodeScope ChildScope { get; private set; }

        public NodeScope CreateChildScope(Node node = null, Guid? threadId = null)
            => ChildScope = new NodeScope(this, node ?? Node, threadId ?? ThreadId);

        public TAction GetAction<TAction>(IActionContext context)
            where TAction : ActionNode
        {
            ValuesHolder.GlobalValues.Value = context.Activity.Values;
            ValuesHolder.StateValues.Value = null;
            ValuesHolder.SourceStateValues.Value = null;
            ValuesHolder.TargetStateValues.Value = null;

            var action = ServiceProvider.GetService<TAction>();
            action.Context = context;

            return action;
        }

        public TAcceptEventAction GetAcceptEventAction<TEvent, TAcceptEventAction>(IAcceptEventActionContext<TEvent> context)
            where TEvent : Event, new()
            where TAcceptEventAction : AcceptEventActionNode<TEvent>
        {
            ValuesHolder.GlobalValues.Value = context.Activity.Values;
            ValuesHolder.StateValues.Value = null;
            ValuesHolder.SourceStateValues.Value = null;
            ValuesHolder.TargetStateValues.Value = null;

            var acceptEventAction = ServiceProvider.GetService<TAcceptEventAction>();
            acceptEventAction.Context = context;

            return acceptEventAction;
        }

        public TSendEventAction GetSendEventAction<TEvent, TSendEventAction>(IActionContext context)
            where TEvent : Event, new()
            where TSendEventAction : SendEventActionNode<TEvent>
        {
            ValuesHolder.GlobalValues.Value = context.Activity.Values;
            ValuesHolder.StateValues.Value = null;
            ValuesHolder.SourceStateValues.Value = null;
            ValuesHolder.TargetStateValues.Value = null;

            var acceptEventAction = ServiceProvider.GetService<TSendEventAction>();
            acceptEventAction.Context = context;

            return acceptEventAction;
        }

        public TStructuredActivity GetStructuredActivity<TStructuredActivity>(IActionContext context)
            where TStructuredActivity : StructuredActivityNode
        {
            ValuesHolder.GlobalValues.Value = context.Activity.Values;
            ValuesHolder.StateValues.Value = null;
            ValuesHolder.SourceStateValues.Value = null;
            ValuesHolder.TargetStateValues.Value = null;

            var structuredActivity = ServiceProvider.GetService<TStructuredActivity>();
            structuredActivity.Context = context;

            return structuredActivity;
        }

        public TExceptionHandler GetExceptionHandler<TException, TExceptionHandler>(IExceptionHandlerContext<TException> context)
            where TException : Exception
            where TExceptionHandler : ExceptionHandlerNode<TException>
        {
            ValuesHolder.GlobalValues.Value = context.Activity.Values;
            ValuesHolder.StateValues.Value = null;
            ValuesHolder.SourceStateValues.Value = null;
            ValuesHolder.TargetStateValues.Value = null;

            var exceptionHandler = ServiceProvider.GetService<TExceptionHandler>();
            exceptionHandler.Context = context;

            return exceptionHandler;
        }

        public TFlow GetFlow<TFlow>(IFlowContext context)
            where TFlow : BaseControlFlow
        {
            ValuesHolder.GlobalValues.Value = context.Activity.Values;
            ValuesHolder.StateValues.Value = null;
            ValuesHolder.SourceStateValues.Value = null;
            ValuesHolder.TargetStateValues.Value = null;

            var flow = ServiceProvider.GetService<TFlow>();
            flow.Context = context;

            return flow;
        }

        public TControlFlow GetControlFlow<TControlFlow>(IFlowContext context)
            where TControlFlow : ControlFlow
            => GetFlow<TControlFlow>(context);

        public TFlow GetObjectFlow<TFlow, TToken>(IFlowContext<TToken> context)
            where TFlow : Flow<TToken>
            // where TToken : Token, new()
            => GetFlow<TFlow>(context);

        public TFlow GetObjectTransformationFlow<TFlow, TToken, TTransformedToken>(IFlowContext<TToken> context)
            where TFlow : TransformationFlow<TToken, TTransformedToken>
            // where TToken : Token, new()
            ////where TTransformedToken : Token, new()
            => GetFlow<TFlow>(context);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            scope?.Dispose();
        }
    }
}

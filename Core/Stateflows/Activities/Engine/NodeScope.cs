using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Classes;
using Stateflows.Activities.Models;
using Stateflows.Activities.Context.Interfaces;

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
            ContextValuesHolder.GlobalValues.Value = context.Activity.Values;
            ContextValuesHolder.StateValues.Value = null;
            ContextValuesHolder.SourceStateValues.Value = null;
            ContextValuesHolder.TargetStateValues.Value = null;

            return ServiceProvider.GetService<TAction>();
        }

        public TAcceptEventAction GetAcceptEventAction<TEvent, TAcceptEventAction>(IAcceptEventActionContext<TEvent> context)
            where TEvent : Event, new()
            where TAcceptEventAction : AcceptEventActionNode<TEvent>
        {
            ContextValuesHolder.GlobalValues.Value = context.Activity.Values;
            ContextValuesHolder.StateValues.Value = null;
            ContextValuesHolder.SourceStateValues.Value = null;
            ContextValuesHolder.TargetStateValues.Value = null;

            return ServiceProvider.GetService<TAcceptEventAction>();
        }

        public TSendEventAction GetSendEventAction<TEvent, TSendEventAction>(IActionContext context)
            where TEvent : Event, new()
            where TSendEventAction : SendEventActionNode<TEvent>
        {
            ContextValuesHolder.GlobalValues.Value = context.Activity.Values;
            ContextValuesHolder.StateValues.Value = null;
            ContextValuesHolder.SourceStateValues.Value = null;
            ContextValuesHolder.TargetStateValues.Value = null;

            return ServiceProvider.GetService<TSendEventAction>();
        }

        public TStructuredActivity GetStructuredActivity<TStructuredActivity>(IActionContext context)
            where TStructuredActivity : StructuredActivityNode
        {
            ContextValuesHolder.GlobalValues.Value = context.Activity.Values;
            ContextValuesHolder.StateValues.Value = null;
            ContextValuesHolder.SourceStateValues.Value = null;
            ContextValuesHolder.TargetStateValues.Value = null;

            return ServiceProvider.GetService<TStructuredActivity>();
        }

        public TExceptionHandler GetExceptionHandler<TException, TExceptionHandler>(IExceptionHandlerContext<TException> context)
            where TException : Exception
            where TExceptionHandler : ExceptionHandlerNode<TException>
        {
            ContextValuesHolder.GlobalValues.Value = context.Activity.Values;
            ContextValuesHolder.StateValues.Value = null;
            ContextValuesHolder.SourceStateValues.Value = null;
            ContextValuesHolder.TargetStateValues.Value = null;

            return ServiceProvider.GetService<TExceptionHandler>();
        }

        public TFlow GetFlow<TFlow>(IActivityFlowContext context)
            where TFlow : BaseControlFlow
        {
            ContextValuesHolder.GlobalValues.Value = context.Activity.Values;
            ContextValuesHolder.StateValues.Value = null;
            ContextValuesHolder.SourceStateValues.Value = null;
            ContextValuesHolder.TargetStateValues.Value = null;

            return ServiceProvider.GetService<TFlow>();
        }

        public TControlFlow GetControlFlow<TControlFlow>(IActivityFlowContext context)
            where TControlFlow : ControlFlow
            => GetFlow<TControlFlow>(context);

        public TFlow GetObjectFlow<TFlow, TToken>(IActivityFlowContext<TToken> context)
            where TFlow : Flow<TToken>
            => GetFlow<TFlow>(context);

        public TTransformationFlow GetObjectTransformationFlow<TTransformationFlow, TToken, TTransformedToken>(IActivityFlowContext<TToken> context)
            where TTransformationFlow : TransformationFlow<TToken, TTransformedToken>
            => GetFlow<TTransformationFlow>(context);

        public TElseTransformationFlow GetElseObjectTransformationFlow<TElseTransformationFlow, TToken, TTransformedToken>(IActivityFlowContext<TToken> context)
            where TElseTransformationFlow : ElseTransformationFlow<TToken, TTransformedToken>
            => GetFlow<TElseTransformationFlow>(context);

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

using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Classes;
using Stateflows.Activities.Models;
using Stateflows.Activities.Context;
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

        public TDefaultInitializer GetDefaultInitializer<TDefaultInitializer>(IActivityInitializationContext context)
            where TDefaultInitializer : class, IDefaultInitializer
        {
            ContextValues.GlobalValuesHolder.Value = context.Activity.Values;
            ContextValues.StateValuesHolder.Value = null;
            ContextValues.SourceStateValuesHolder.Value = null;
            ContextValues.TargetStateValuesHolder.Value = null;

            ContextHolder.NodeContext.Value = null;
            ContextHolder.FlowContext.Value = null;
            ContextHolder.ActivityContext.Value = context.Activity;
            ContextHolder.ExecutionContext.Value = context;
            ContextHolder.ExceptionContext.Value = null;

            var initializer = ServiceProvider.GetService<TDefaultInitializer>();

            return initializer;
        }

        public TInitializer GetInitializer<TInitializer, TInitializationEvent>(IActivityInitializationContext<TInitializationEvent> context)
            where TInitializer : class, IInitializer<TInitializationEvent>
            where TInitializationEvent : EventHolder, new()
        {
            ContextValues.GlobalValuesHolder.Value = context.Activity.Values;
            ContextValues.StateValuesHolder.Value = null;
            ContextValues.SourceStateValuesHolder.Value = null;
            ContextValues.TargetStateValuesHolder.Value = null;

            ContextHolder.NodeContext.Value = null;
            ContextHolder.FlowContext.Value = null;
            ContextHolder.ActivityContext.Value = context.Activity;
            ContextHolder.ExecutionContext.Value = context;
            ContextHolder.ExceptionContext.Value = null;

            var initializer = ServiceProvider.GetService<TInitializer>();

            return initializer;
        }

        public TFinalizer GetFinalizer<TFinalizer>(IActivityActionContext context)
            where TFinalizer : class, IFinalizer
        {
            ContextValues.GlobalValuesHolder.Value = context.Activity.Values;
            ContextValues.StateValuesHolder.Value = null;
            ContextValues.SourceStateValuesHolder.Value = null;
            ContextValues.TargetStateValuesHolder.Value = null;

            ContextHolder.NodeContext.Value = null;
            ContextHolder.FlowContext.Value = null;
            ContextHolder.ActivityContext.Value = context.Activity;
            ContextHolder.ExecutionContext.Value = context;
            ContextHolder.ExceptionContext.Value = null;

            var initializer = ServiceProvider.GetService<TFinalizer>();

            return initializer;
        }

        public TAction GetAction<TAction>(IActionContext context)
            where TAction : class, IActionNode
        {
            ContextValues.GlobalValuesHolder.Value = context.Activity.Values;
            ContextValues.StateValuesHolder.Value = null;
            ContextValues.SourceStateValuesHolder.Value = null;
            ContextValues.TargetStateValuesHolder.Value = null;

            ContextHolder.NodeContext.Value = context.CurrentNode;
            ContextHolder.FlowContext.Value = null;
            ContextHolder.ActivityContext.Value = context.Activity;
            ContextHolder.ExecutionContext.Value = context;
            ContextHolder.ExceptionContext.Value = null;

            return ServiceProvider.GetService<TAction>();
        }

        public TAcceptEventAction GetAcceptEventAction<TEvent, TAcceptEventAction>(IAcceptEventActionContext<TEvent> context)
            where TAcceptEventAction : class, IAcceptEventActionNode<TEvent>
        {
            ContextValues.GlobalValuesHolder.Value = context.Activity.Values;
            ContextValues.StateValuesHolder.Value = null;
            ContextValues.SourceStateValuesHolder.Value = null;
            ContextValues.TargetStateValuesHolder.Value = null;

            ContextHolder.NodeContext.Value = context.CurrentNode;
            ContextHolder.FlowContext.Value = null;
            ContextHolder.ActivityContext.Value = context.Activity;
            ContextHolder.ExecutionContext.Value = context;
            ContextHolder.ExceptionContext.Value = null;

            return ServiceProvider.GetService<TAcceptEventAction>();
        }

        public TTimeEventAction GetTimeEventAction<TTimeEventAction>(IActionContext context)
            where TTimeEventAction : class, ITimeEventActionNode
        {
            ContextValues.GlobalValuesHolder.Value = context.Activity.Values;
            ContextValues.StateValuesHolder.Value = null;
            ContextValues.SourceStateValuesHolder.Value = null;
            ContextValues.TargetStateValuesHolder.Value = null;

            ContextHolder.NodeContext.Value = context.CurrentNode;
            ContextHolder.FlowContext.Value = null;
            ContextHolder.ActivityContext.Value = context.Activity;
            ContextHolder.ExecutionContext.Value = context;
            ContextHolder.ExceptionContext.Value = null;

            return ServiceProvider.GetService<TTimeEventAction>();
        }

        public TSendEventAction GetSendEventAction<TEvent, TSendEventAction>(IActionContext context)
            where TSendEventAction : class, ISendEventActionNode<TEvent>
        {
            ContextValues.GlobalValuesHolder.Value = context.Activity.Values;
            ContextValues.StateValuesHolder.Value = null;
            ContextValues.SourceStateValuesHolder.Value = null;
            ContextValues.TargetStateValuesHolder.Value = null;

            ContextHolder.NodeContext.Value = context.CurrentNode;
            ContextHolder.FlowContext.Value = null;
            ContextHolder.ActivityContext.Value = context.Activity;
            ContextHolder.ExecutionContext.Value = context;
            ContextHolder.ExceptionContext.Value = null;

            return ServiceProvider.GetService<TSendEventAction>();
        }

        public TStructuredActivity GetStructuredActivity<TStructuredActivity>(IActionContext context)
            where TStructuredActivity : class, IStructuredActivityNode
        {
            ContextValues.GlobalValuesHolder.Value = context.Activity.Values;
            ContextValues.StateValuesHolder.Value = null;
            ContextValues.SourceStateValuesHolder.Value = null;
            ContextValues.TargetStateValuesHolder.Value = null;

            ContextHolder.NodeContext.Value = context.CurrentNode;
            ContextHolder.FlowContext.Value = null;
            ContextHolder.ActivityContext.Value = context.Activity;
            ContextHolder.ExecutionContext.Value = context;
            ContextHolder.ExceptionContext.Value = null;

            return ServiceProvider.GetService<TStructuredActivity>();
        }

        public TExceptionHandler GetExceptionHandler<TException, TExceptionHandler>(IExceptionHandlerContext<TException> context)
            where TException : Exception
            where TExceptionHandler : class, IExceptionHandlerNode<TException>
        {
            ContextValues.GlobalValuesHolder.Value = context.Activity.Values;
            ContextValues.StateValuesHolder.Value = null;
            ContextValues.SourceStateValuesHolder.Value = null;
            ContextValues.TargetStateValuesHolder.Value = null;

            ContextHolder.NodeContext.Value = context.CurrentNode;
            ContextHolder.FlowContext.Value = null;
            ContextHolder.ActivityContext.Value = context.Activity;
            ContextHolder.ExecutionContext.Value = context;
            ContextHolder.ExceptionContext.Value = context;

            return ServiceProvider.GetService<TExceptionHandler>();
        }

        public TFlow GetFlow<TFlow>(IActivityFlowContext context)
            where TFlow : IEdge
        {
            ContextValues.GlobalValuesHolder.Value = context.Activity.Values;
            ContextValues.StateValuesHolder.Value = null;
            ContextValues.SourceStateValuesHolder.Value = null;
            ContextValues.TargetStateValuesHolder.Value = null;

            ContextHolder.NodeContext.Value = null;
            ContextHolder.FlowContext.Value = context;
            ContextHolder.ActivityContext.Value = context.Activity;
            ContextHolder.ExecutionContext.Value = context;
            ContextHolder.ExceptionContext.Value = null;

            return ServiceProvider.GetService<TFlow>();
        }

        public TControlFlow GetControlFlow<TControlFlow>(IActivityFlowContext context)
            where TControlFlow : class, IControlFlow
            => GetFlow<TControlFlow>(context);

        public TFlow GetObjectFlow<TFlow, TToken>(IActivityFlowContext<TToken> context)
            where TFlow : class, IFlow<TToken>
            => GetFlow<TFlow>(context);

        public TTransformationFlow GetObjectTransformationFlow<TTransformationFlow, TToken, TTransformedToken>(IActivityFlowContext<TToken> context)
            where TTransformationFlow : class, IFlowTransformation<TToken, TTransformedToken>
            => GetFlow<TTransformationFlow>(context);

        public TElseTransformationFlow GetElseObjectTransformationFlow<TElseTransformationFlow, TToken, TTransformedToken>(IActivityFlowContext<TToken> context)
            where TElseTransformationFlow : class, IFlowTransformation<TToken, TTransformedToken>
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

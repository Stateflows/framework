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
            => baseServiceProvider ??= new StateflowsServiceProvider(BaseNodeScope.ServiceProvider);

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
            baseServiceProvider = new StateflowsServiceProvider(serviceProvider);
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

            ActivitiesContextHolder.NodeContext.Value = null;
            ActivitiesContextHolder.FlowContext.Value = null;
            ActivitiesContextHolder.ActivityContext.Value = context.Activity;
            ActivitiesContextHolder.ExecutionContext.Value = context;
            ActivitiesContextHolder.ExceptionContext.Value = null;

            var initializer = ActivatorUtilities.CreateInstance<TDefaultInitializer>(ServiceProvider);

            return initializer;
        }

        public TInitializer GetInitializer<TInitializer, TInitializationEvent>(IActivityInitializationContext<TInitializationEvent> context)
            where TInitializer : class, IInitializer<TInitializationEvent>
        {
            ContextValues.GlobalValuesHolder.Value = context.Activity.Values;
            ContextValues.StateValuesHolder.Value = null;
            ContextValues.SourceStateValuesHolder.Value = null;
            ContextValues.TargetStateValuesHolder.Value = null;

            ActivitiesContextHolder.NodeContext.Value = null;
            ActivitiesContextHolder.FlowContext.Value = null;
            ActivitiesContextHolder.ActivityContext.Value = context.Activity;
            ActivitiesContextHolder.ExecutionContext.Value = context;
            ActivitiesContextHolder.ExceptionContext.Value = null;

            var initializer = ActivatorUtilities.CreateInstance<TInitializer>(ServiceProvider);

            return initializer;
        }

        public TFinalizer GetFinalizer<TFinalizer>(IActivityActionContext context)
            where TFinalizer : class, IFinalizer
        {
            ContextValues.GlobalValuesHolder.Value = context.Activity.Values;
            ContextValues.StateValuesHolder.Value = null;
            ContextValues.SourceStateValuesHolder.Value = null;
            ContextValues.TargetStateValuesHolder.Value = null;

            ActivitiesContextHolder.NodeContext.Value = null;
            ActivitiesContextHolder.FlowContext.Value = null;
            ActivitiesContextHolder.ActivityContext.Value = context.Activity;
            ActivitiesContextHolder.ExecutionContext.Value = context;
            ActivitiesContextHolder.ExceptionContext.Value = null;

            var initializer = ActivatorUtilities.CreateInstance<TFinalizer>(ServiceProvider);

            return initializer;
        }

        public TAction GetAction<TAction>(IActionContext context)
            where TAction : class, IActionNode
        {
            ContextValues.GlobalValuesHolder.Value = context.Activity.Values;
            ContextValues.StateValuesHolder.Value = null;
            ContextValues.SourceStateValuesHolder.Value = null;
            ContextValues.TargetStateValuesHolder.Value = null;

            ActivitiesContextHolder.NodeContext.Value = context.CurrentNode;
            ActivitiesContextHolder.FlowContext.Value = null;
            ActivitiesContextHolder.ActivityContext.Value = context.Activity;
            ActivitiesContextHolder.ExecutionContext.Value = context;
            ActivitiesContextHolder.ExceptionContext.Value = null;

            return ActivatorUtilities.CreateInstance<TAction>(ServiceProvider);
        }

        public TAcceptEventAction GetAcceptEventAction<TEvent, TAcceptEventAction>(IAcceptEventActionContext<TEvent> context)

            where TAcceptEventAction : class, IAcceptEventActionNode<TEvent>
        {
            ContextValues.GlobalValuesHolder.Value = context.Activity.Values;
            ContextValues.StateValuesHolder.Value = null;
            ContextValues.SourceStateValuesHolder.Value = null;
            ContextValues.TargetStateValuesHolder.Value = null;

            ActivitiesContextHolder.NodeContext.Value = context.CurrentNode;
            ActivitiesContextHolder.FlowContext.Value = null;
            ActivitiesContextHolder.ActivityContext.Value = context.Activity;
            ActivitiesContextHolder.ExecutionContext.Value = context;
            ActivitiesContextHolder.ExceptionContext.Value = null;

            return ActivatorUtilities.CreateInstance<TAcceptEventAction>(ServiceProvider);
        }

        public TTimeEventAction GetTimeEventAction<TTimeEventAction>(IActionContext context)
            where TTimeEventAction : class, ITimeEventActionNode
        {
            ContextValues.GlobalValuesHolder.Value = context.Activity.Values;
            ContextValues.StateValuesHolder.Value = null;
            ContextValues.SourceStateValuesHolder.Value = null;
            ContextValues.TargetStateValuesHolder.Value = null;

            ActivitiesContextHolder.NodeContext.Value = context.CurrentNode;
            ActivitiesContextHolder.FlowContext.Value = null;
            ActivitiesContextHolder.ActivityContext.Value = context.Activity;
            ActivitiesContextHolder.ExecutionContext.Value = context;
            ActivitiesContextHolder.ExceptionContext.Value = null;

            return ActivatorUtilities.CreateInstance<TTimeEventAction>(ServiceProvider);
        }

        public TSendEventAction GetSendEventAction<TEvent, TSendEventAction>(IActionContext context)
            where TSendEventAction : class, ISendEventActionNode<TEvent>
        {
            ContextValues.GlobalValuesHolder.Value = context.Activity.Values;
            ContextValues.StateValuesHolder.Value = null;
            ContextValues.SourceStateValuesHolder.Value = null;
            ContextValues.TargetStateValuesHolder.Value = null;

            ActivitiesContextHolder.NodeContext.Value = context.CurrentNode;
            ActivitiesContextHolder.FlowContext.Value = null;
            ActivitiesContextHolder.ActivityContext.Value = context.Activity;
            ActivitiesContextHolder.ExecutionContext.Value = context;
            ActivitiesContextHolder.ExceptionContext.Value = null;

            return ActivatorUtilities.CreateInstance<TSendEventAction>(ServiceProvider);
        }

        public TStructuredActivity GetStructuredActivity<TStructuredActivity>(IActionContext context)
            where TStructuredActivity : class, IStructuredActivityNode
        {
            ContextValues.GlobalValuesHolder.Value = context.Activity.Values;
            ContextValues.StateValuesHolder.Value = null;
            ContextValues.SourceStateValuesHolder.Value = null;
            ContextValues.TargetStateValuesHolder.Value = null;

            ActivitiesContextHolder.NodeContext.Value = context.CurrentNode;
            ActivitiesContextHolder.FlowContext.Value = null;
            ActivitiesContextHolder.ActivityContext.Value = context.Activity;
            ActivitiesContextHolder.ExecutionContext.Value = context;
            ActivitiesContextHolder.ExceptionContext.Value = null;

            return ActivatorUtilities.CreateInstance<TStructuredActivity>(ServiceProvider);
        }

        public TExceptionHandler GetExceptionHandler<TException, TExceptionHandler>(IExceptionHandlerContext<TException> context)
            where TException : Exception
            where TExceptionHandler : class, IExceptionHandlerNode<TException>
        {
            ContextValues.GlobalValuesHolder.Value = context.Activity.Values;
            ContextValues.StateValuesHolder.Value = null;
            ContextValues.SourceStateValuesHolder.Value = null;
            ContextValues.TargetStateValuesHolder.Value = null;

            ActivitiesContextHolder.NodeContext.Value = context.CurrentNode;
            ActivitiesContextHolder.FlowContext.Value = null;
            ActivitiesContextHolder.ActivityContext.Value = context.Activity;
            ActivitiesContextHolder.ExecutionContext.Value = context;
            ActivitiesContextHolder.ExceptionContext.Value = context;

            return ActivatorUtilities.CreateInstance<TExceptionHandler>(ServiceProvider);
        }

        public TFlow GetFlow<TFlow>(IActivityFlowContext context)
            where TFlow : IEdge
        {
            ContextValues.GlobalValuesHolder.Value = context.Activity.Values;
            ContextValues.StateValuesHolder.Value = null;
            ContextValues.SourceStateValuesHolder.Value = null;
            ContextValues.TargetStateValuesHolder.Value = null;

            ActivitiesContextHolder.NodeContext.Value = null;
            ActivitiesContextHolder.FlowContext.Value = context;
            ActivitiesContextHolder.ActivityContext.Value = context.Activity;
            ActivitiesContextHolder.ExecutionContext.Value = context;
            ActivitiesContextHolder.ExceptionContext.Value = null;

            return ActivatorUtilities.CreateInstance<TFlow>(ServiceProvider);
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
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
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

        [DebuggerHidden]
        public Task<TDefaultInitializer> GetDefaultInitializerAsync<TDefaultInitializer>(IActivityInitializationContext context)
            where TDefaultInitializer : class, IDefaultInitializer
        {
            ContextValues.GlobalValuesHolder.Value = context.Activity.Values;
            ContextValues.StateValuesHolder.Value = null;
            ContextValues.ParentStateValuesHolder.Value = null;
            ContextValues.SourceStateValuesHolder.Value = null;
            ContextValues.TargetStateValuesHolder.Value = null;

            ActivitiesContextHolder.NodeContext.Value = null;
            ActivitiesContextHolder.FlowContext.Value = null;
            ActivitiesContextHolder.ActivityContext.Value = context.Activity;
            ActivitiesContextHolder.ExecutionContext.Value = context;
            ActivitiesContextHolder.ExceptionContext.Value = null;

            return StateflowsActivator.CreateInstanceAsync<TDefaultInitializer>(ServiceProvider);
        }

        [DebuggerHidden]
        public Task<TInitializer> GetInitializerAsync<TInitializer, TInitializationEvent>(IActivityInitializationContext<TInitializationEvent> context)
            where TInitializer : class, IInitializer<TInitializationEvent>
        {
            ContextValues.GlobalValuesHolder.Value = context.Activity.Values;
            ContextValues.StateValuesHolder.Value = null;
            ContextValues.ParentStateValuesHolder.Value = null;
            ContextValues.SourceStateValuesHolder.Value = null;
            ContextValues.TargetStateValuesHolder.Value = null;

            ActivitiesContextHolder.NodeContext.Value = null;
            ActivitiesContextHolder.FlowContext.Value = null;
            ActivitiesContextHolder.ActivityContext.Value = context.Activity;
            ActivitiesContextHolder.ExecutionContext.Value = context;
            ActivitiesContextHolder.ExceptionContext.Value = null;

            return StateflowsActivator.CreateInstanceAsync<TInitializer>(ServiceProvider);
        }
        
        [DebuggerHidden]
        public Task<TFinalizer> GetFinalizerAsync<TFinalizer>(IActivityActionContext context)
            where TFinalizer : class, IFinalizer
        {
            ContextValues.GlobalValuesHolder.Value = context.Activity.Values;
            ContextValues.StateValuesHolder.Value = null;
            ContextValues.ParentStateValuesHolder.Value = null;
            ContextValues.SourceStateValuesHolder.Value = null;
            ContextValues.TargetStateValuesHolder.Value = null;

            ActivitiesContextHolder.NodeContext.Value = null;
            ActivitiesContextHolder.FlowContext.Value = null;
            ActivitiesContextHolder.ActivityContext.Value = context.Activity;
            ActivitiesContextHolder.ExecutionContext.Value = context;
            ActivitiesContextHolder.ExceptionContext.Value = null;

            return StateflowsActivator.CreateInstanceAsync<TFinalizer>(ServiceProvider);
        }
        
        [DebuggerHidden]
        public Task<TAction> GetActionAsync<TAction>(Context.Interfaces.IActionContext context)
            where TAction : class, IActionNode
        {
            ContextValues.GlobalValuesHolder.Value = context.Activity.Values;
            ContextValues.StateValuesHolder.Value = null;
            ContextValues.ParentStateValuesHolder.Value = null;
            ContextValues.SourceStateValuesHolder.Value = null;
            ContextValues.TargetStateValuesHolder.Value = null;

            ActivitiesContextHolder.NodeContext.Value = context.CurrentNode;
            ActivitiesContextHolder.FlowContext.Value = null;
            ActivitiesContextHolder.ActivityContext.Value = context.Activity;
            ActivitiesContextHolder.ExecutionContext.Value = context;
            ActivitiesContextHolder.ExceptionContext.Value = null;

            return StateflowsActivator.CreateInstanceAsync<TAction>(ServiceProvider);
        }

        [DebuggerHidden]
        public Task<TAcceptEventAction> GetAcceptEventActionAsync<TEvent, TAcceptEventAction>(IAcceptEventActionContext<TEvent> context)

            where TAcceptEventAction : class, IAcceptEventActionNode<TEvent>
        {
            ContextValues.GlobalValuesHolder.Value = context.Activity.Values;
            ContextValues.StateValuesHolder.Value = null;
            ContextValues.ParentStateValuesHolder.Value = null;
            ContextValues.SourceStateValuesHolder.Value = null;
            ContextValues.TargetStateValuesHolder.Value = null;

            ActivitiesContextHolder.NodeContext.Value = context.CurrentNode;
            ActivitiesContextHolder.FlowContext.Value = null;
            ActivitiesContextHolder.ActivityContext.Value = context.Activity;
            ActivitiesContextHolder.ExecutionContext.Value = context;
            ActivitiesContextHolder.ExceptionContext.Value = null;

            return StateflowsActivator.CreateInstanceAsync<TAcceptEventAction>(ServiceProvider);
        }

        [DebuggerHidden]
        public Task<TTimeEventAction> GetTimeEventActionAsync<TTimeEventAction>(Context.Interfaces.IActionContext context)
            where TTimeEventAction : class, ITimeEventActionNode
        {
            ContextValues.GlobalValuesHolder.Value = context.Activity.Values;
            ContextValues.StateValuesHolder.Value = null;
            ContextValues.ParentStateValuesHolder.Value = null;
            ContextValues.SourceStateValuesHolder.Value = null;
            ContextValues.TargetStateValuesHolder.Value = null;

            ActivitiesContextHolder.NodeContext.Value = context.CurrentNode;
            ActivitiesContextHolder.FlowContext.Value = null;
            ActivitiesContextHolder.ActivityContext.Value = context.Activity;
            ActivitiesContextHolder.ExecutionContext.Value = context;
            ActivitiesContextHolder.ExceptionContext.Value = null;

            return StateflowsActivator.CreateInstanceAsync<TTimeEventAction>(ServiceProvider);
        }

        [DebuggerHidden]
        public Task<TSendEventAction> GetSendEventActionAsync<TEvent, TSendEventAction>(Context.Interfaces.IActionContext context)
            where TSendEventAction : class, ISendEventActionNode<TEvent>
        {
            ContextValues.GlobalValuesHolder.Value = context.Activity.Values;
            ContextValues.StateValuesHolder.Value = null;
            ContextValues.ParentStateValuesHolder.Value = null;
            ContextValues.SourceStateValuesHolder.Value = null;
            ContextValues.TargetStateValuesHolder.Value = null;

            ActivitiesContextHolder.NodeContext.Value = context.CurrentNode;
            ActivitiesContextHolder.FlowContext.Value = null;
            ActivitiesContextHolder.ActivityContext.Value = context.Activity;
            ActivitiesContextHolder.ExecutionContext.Value = context;
            ActivitiesContextHolder.ExceptionContext.Value = null;

            return StateflowsActivator.CreateInstanceAsync<TSendEventAction>(ServiceProvider);
        }

        [DebuggerHidden]
        public Task<TStructuredActivity> GetStructuredActivityAsync<TStructuredActivity>(Context.Interfaces.IActionContext context)
            where TStructuredActivity : class, IStructuredActivityNode
        {
            ContextValues.GlobalValuesHolder.Value = context.Activity.Values;
            ContextValues.StateValuesHolder.Value = null;
            ContextValues.ParentStateValuesHolder.Value = null;
            ContextValues.SourceStateValuesHolder.Value = null;
            ContextValues.TargetStateValuesHolder.Value = null;

            ActivitiesContextHolder.NodeContext.Value = context.CurrentNode;
            ActivitiesContextHolder.FlowContext.Value = null;
            ActivitiesContextHolder.ActivityContext.Value = context.Activity;
            ActivitiesContextHolder.ExecutionContext.Value = context;
            ActivitiesContextHolder.ExceptionContext.Value = null;

            return StateflowsActivator.CreateInstanceAsync<TStructuredActivity>(ServiceProvider);
        }

        [DebuggerHidden]
        public Task<TExceptionHandler> GetExceptionHandlerAsync<TException, TExceptionHandler>(IExceptionHandlerContext<TException> context)
            where TException : Exception
            where TExceptionHandler : class, IExceptionHandlerNode<TException>
        {
            ContextValues.GlobalValuesHolder.Value = context.Activity.Values;
            ContextValues.StateValuesHolder.Value = null;
            ContextValues.ParentStateValuesHolder.Value = null;
            ContextValues.SourceStateValuesHolder.Value = null;
            ContextValues.TargetStateValuesHolder.Value = null;

            ActivitiesContextHolder.NodeContext.Value = context.CurrentNode;
            ActivitiesContextHolder.FlowContext.Value = null;
            ActivitiesContextHolder.ActivityContext.Value = context.Activity;
            ActivitiesContextHolder.ExecutionContext.Value = context;
            ActivitiesContextHolder.ExceptionContext.Value = context;

            return StateflowsActivator.CreateInstanceAsync<TExceptionHandler>(ServiceProvider);
        }

        [DebuggerHidden]
        public Task<TFlow> GetFlowAsync<TFlow>(IActivityFlowContext context)
            where TFlow : IEdge
        {
            ContextValues.GlobalValuesHolder.Value = context.Activity.Values;
            ContextValues.StateValuesHolder.Value = null;
            ContextValues.ParentStateValuesHolder.Value = null;
            ContextValues.SourceStateValuesHolder.Value = null;
            ContextValues.TargetStateValuesHolder.Value = null;

            ActivitiesContextHolder.NodeContext.Value = null;
            ActivitiesContextHolder.FlowContext.Value = context;
            ActivitiesContextHolder.ActivityContext.Value = context.Activity;
            ActivitiesContextHolder.ExecutionContext.Value = context;
            ActivitiesContextHolder.ExceptionContext.Value = null;

            return StateflowsActivator.CreateInstanceAsync<TFlow>(ServiceProvider);
        }

        [DebuggerHidden]
        public Task<TControlFlow> GetControlFlowAsync<TControlFlow>(IActivityFlowContext context)
            where TControlFlow : class, IControlFlow
            => GetFlowAsync<TControlFlow>(context);

        [DebuggerHidden]
        public Task<TFlow> GetObjectFlowAsync<TFlow, TToken>(IActivityFlowContext<TToken> context)
            where TFlow : class, IFlow<TToken>
            => GetFlowAsync<TFlow>(context);

        [DebuggerHidden]
        public Task<TTransformationFlow> GetObjectTransformationFlowAsync<TTransformationFlow, TToken, TTransformedToken>(IActivityFlowContext<TToken> context)
            where TTransformationFlow : class, IFlowTransformation<TToken, TTransformedToken>
            => GetFlowAsync<TTransformationFlow>(context);

        [DebuggerHidden]
        public Task<TElseTransformationFlow> GetElseObjectTransformationFlowAsync<TElseTransformationFlow, TToken, TTransformedToken>(IActivityFlowContext<TToken> context)
            where TElseTransformationFlow : class, IFlowTransformation<TToken, TTransformedToken>
            => GetFlowAsync<TElseTransformationFlow>(context);

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
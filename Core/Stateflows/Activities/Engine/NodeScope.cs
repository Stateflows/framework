using System;
using System.Threading;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Activities.Context.Interfaces;


namespace Stateflows.Activities.Engine
{
    internal class NodeScope : IDisposable
    {
        private readonly NodeScope BaseNodeScope = null;

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

        public void Terminate()
        {
            CancellationTokenSource.Cancel();
            IsTerminated = true;
        }

        public NodeScope(IServiceProvider serviceProvider, Guid threadId)
        {
            baseServiceProvider = serviceProvider;
            ThreadId = threadId;
        }

        public NodeScope(NodeScope nodeScope, Guid threadId)
        {
            BaseNodeScope = nodeScope;
            ThreadId = threadId;
        }

        public NodeScope ChildScope { get; private set; }

        public NodeScope CreateChildScope(Guid? threadId = null)
            => ChildScope = new NodeScope(this, threadId ?? ThreadId);

        private readonly IDictionary<Type, ActionNode> Actions = new Dictionary<Type, ActionNode>();

        public TAction GetAction<TAction>(IActionContext context)
            where TAction : ActionNode
        {
            lock (Actions)
            {
                if (!Actions.TryGetValue(typeof(TAction), out var action))
                {
                    action = ServiceProvider.GetService<TAction>();

                    Actions.Add(typeof(TAction), action);
                }

                action.Context = context;

                return action as TAction;
            }
        }

        private readonly IDictionary<Type, ActionNode> AcceptEventActions = new Dictionary<Type, ActionNode>();

        public TAcceptEventAction GetAcceptEventAction<TEvent, TAcceptEventAction>(IAcceptEventActionContext<TEvent> context)
            where TEvent : Event, new()
            where TAcceptEventAction : AcceptEventActionNode<TEvent>
        {
            lock (AcceptEventActions)
            {
                if (!AcceptEventActions.TryGetValue(typeof(TAcceptEventAction), out var action))
                {
                    action = ServiceProvider.GetService<TAcceptEventAction>();

                    AcceptEventActions.Add(typeof(TAcceptEventAction), action);
                }

                var acceptEventAction = action as TAcceptEventAction;

                acceptEventAction.Context = context;

                return acceptEventAction;
            }
        }

        private readonly IDictionary<Type, ActivityNode> SendEventActions = new Dictionary<Type, ActivityNode>();

        public TSendEventAction GetSendEventAction<TEvent, TSendEventAction>(IActionContext context)
            where TEvent : Event, new()
            where TSendEventAction : SendEventActionNode<TEvent>
        {
            lock (SendEventActions)
            {
                if (!SendEventActions.TryGetValue(typeof(TSendEventAction), out var action))
                {
                    action = ServiceProvider.GetService<TSendEventAction>();

                    SendEventActions.Add(typeof(TSendEventAction), action);
                }

                var acceptEventAction = action as TSendEventAction;

                acceptEventAction.Context = context;

                return acceptEventAction;
            }
        }

        private readonly IDictionary<Type, StructuredActivityNode> StructuredActivities = new Dictionary<Type, StructuredActivityNode>();

        public TStructuredActivity GetStructuredActivity<TStructuredActivity>(IActionContext context)
            where TStructuredActivity : StructuredActivityNode
        {
            if (!StructuredActivities.TryGetValue(typeof(TStructuredActivity), out var structuredActivity))
            {
                structuredActivity = ServiceProvider.GetService<TStructuredActivity>();

                StructuredActivities.Add(typeof(TStructuredActivity), structuredActivity);
            }

            structuredActivity.Context = context;

            return structuredActivity as TStructuredActivity;
        }

        private readonly IDictionary<Type, ActivityNode> ExceptionHandlers = new Dictionary<Type, ActivityNode>();

        public TExceptionHandler GetExceptionHandler<TException, TExceptionHandler>(IExceptionHandlerContext<TException> context)
            where TException : Exception
            where TExceptionHandler : ExceptionHandlerNode<TException>
        {
            if (!ExceptionHandlers.TryGetValue(typeof(TExceptionHandler), out var exceptionHandlerNode))
            {
                var exceptionHandler = ServiceProvider.GetService<TExceptionHandler>();

                exceptionHandler.Context = context;

                ExceptionHandlers.Add(typeof(TExceptionHandler), exceptionHandler);

                return exceptionHandler;
            }
            else
            {
                if (exceptionHandlerNode is TExceptionHandler exceptionHandler)
                {
                    exceptionHandler.Context = context;

                    return exceptionHandler;
                }
            }

            return null;
        }

        private readonly IDictionary<Type, object> Flows = new Dictionary<Type, object>();

        public TControlFlow GetControlFlow<TControlFlow>(IFlowContext context)
            where TControlFlow : ControlFlow
        {
            if (!Flows.TryGetValue(typeof(TControlFlow), out var flowObj))
            {
                var flow = ServiceProvider.GetService<TControlFlow>();

                flow.Context = context;

                Flows.Add(typeof(TControlFlow), flow);

                return flow;
            }
            else
            {
                (flowObj as TControlFlow).Context = context;

                return flowObj as TControlFlow;
            }
        }

        public TFlow GetObjectFlow<TFlow, TToken>(IFlowContext<TToken> context)
            where TFlow : TokenFlow<TToken>
            where TToken : Token, new()
        {
            if (!Flows.TryGetValue(typeof(TFlow), out var flowObj))
            {
                var flow = ServiceProvider.GetService<TFlow>();

                flow.Context = context;

                Flows.Add(typeof(TFlow), flow);

                return flow;
            }
            else
            {
                (flowObj as TFlow).Context = context;

                return flowObj as TFlow;
            }
        }

        public TFlow GetObjectTransformationFlow<TFlow, TToken, TTransformedToken>(IFlowContext<TToken> context)
            where TFlow : TokenTransformationFlow<TToken, TTransformedToken>
            where TToken : Token, new()
            where TTransformedToken : Token, new()
        {
            if (!Flows.TryGetValue(typeof(TFlow), out var flowObj))
            {
                var flow = ServiceProvider.GetService<TFlow>();

                flow.Context = context;

                Flows.Add(typeof(TFlow), flow);

                return flow;
            }
            else
            {
                (flowObj as TFlow).Context = context;

                return flowObj as TFlow;
            }
        }

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

using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities.Engine
{
    internal class NodeScope : IDisposable
    {
        public IServiceProvider ServiceProvider { get; private set; }

        private IServiceScope Scope { get; }

        public NodeScope(IServiceProvider serviceProvider)
        {
            Scope = serviceProvider.CreateScope();
            ServiceProvider = Scope.ServiceProvider;
        }

        public NodeScope CreateChildScope()
            => new NodeScope(ServiceProvider);

        private readonly IDictionary<Type, Action> Actions = new Dictionary<Type, Action>();

        public TAction GetAction<TAction>(IActionContext context)
            where TAction : Action
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

        private readonly IDictionary<Type, StructuredActivity> StructuredActivities = new Dictionary<Type, StructuredActivity>();

        public TStructuredActivity GetStructuredActivity<TStructuredActivity>(IActionContext context)
            where TStructuredActivity : StructuredActivity
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
            where TExceptionHandler : ExceptionHandler<TException>
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
            where TFlow : ObjectFlow<TToken>
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
            where TFlow : ObjectTransformationFlow<TToken, TTransformedToken>
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
            Scope.Dispose();
        }
    }
}

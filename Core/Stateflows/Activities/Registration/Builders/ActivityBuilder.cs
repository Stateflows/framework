using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Models;
using Stateflows.Activities.Models;
using Stateflows.Activities.Events;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Activities.Registration.Extensions;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.Activities.Registration.Interfaces.Base;

namespace Stateflows.Activities.Registration.Builders
{
    internal class ActivityBuilder :
        BaseActivityBuilder,
        IActivityBuilder,
        ITypedActivityBuilder
    {
        new public Graph Result
        {
            get => Node as Graph;
            set => Node = value;
        }

        public ActivityBuilder(string name, int version, Node parentNode, IServiceCollection services)
            : base(parentNode, services)
        {
            Result = new Graph(name, version);
        }

        public IActivityBuilder AddInitializer(Type initializerType, string initializerName, ActivityPredicateAsync initializerAction)
        {
            if (!Result.Initializers.TryGetValue(initializerName, out var initializer))
            {
                initializer = new Logic<ActivityPredicateAsync>()
                {
                    Name = Constants.Initialize
                };

                Result.Initializers.Add(initializerName, initializer);
            }

            initializer.Actions.Add(initializerAction);

            return this;
        }

        public IActivityBuilder AddOnInitialize(Func<IActivityInitializationContext, Task<bool>> actionAsync)
            => AddOnInitialize<InitializationRequest>(c =>
            {
                var baseContext = c as BaseContext;
                var context = new ActivityInitializationContext(baseContext, baseContext.Context.Event as InitializationRequest);
                return actionAsync(context);
            });

        public IActivityBuilder AddOnInitialize<TInitializationRequest>(Func<IActivityInitializationContext<TInitializationRequest>, Task<bool>> actionAsync)
            where TInitializationRequest : InitializationRequest, new()
        {
            actionAsync.ThrowIfNull(nameof(actionAsync));

            actionAsync = actionAsync.AddActivityInvocationContext(Result);

            var initializerName = EventInfo<TInitializationRequest>.Name;

            return AddInitializer(typeof(TInitializationRequest), initializerName, async c =>
            {
                var result = false;
                var context = new ActivityInitializationContext<TInitializationRequest>(
                    c,
                    (
                        c.Context.Event is ExecutionRequest executionRequest
                            ? executionRequest.InitializationRequest
                            : c.Context.Event
                    ) as TInitializationRequest
                );

                try
                {
                    result = await actionAsync(context);
                }
                catch (Exception e)
                {
                    await c.Context.Executor.Inspector.OnActivityInitializationExceptionAsync(context, context.InitializationRequest, e);
                    result = false;
                }

                return result;
            });
        }

        #region IActivityBuilder
        IActivityBuilder IReactiveActivity<IActivityBuilder>.AddAction(string actionNodeName, ActionDelegateAsync actionAsync, ActionBuildAction buildAction)
            => AddAction(actionNodeName, actionAsync, b => buildAction?.Invoke(b)) as IActivityBuilder;

        IActivityBuilder IReactiveActivity<IActivityBuilder>.AddStructuredActivity(string actionNodeName, ReactiveStructuredActivityBuildAction buildAction)
            => AddStructuredActivity(actionNodeName, buildAction) as IActivityBuilder;

        IActivityBuilder IActivityEvents<IActivityBuilder>.AddOnFinalize(Func<IActivityActionContext, Task> actionAsync)
            => AddOnFinalize(actionAsync) as IActivityBuilder;

        IActivityBuilder IInitial<IActivityBuilder>.AddInitial(InitialBuildAction buildAction)
            => AddInitial(buildAction) as IActivityBuilder;

        IActivityBuilder IFinal<IActivityBuilder>.AddFinal()
            => AddFinal() as IActivityBuilder;

        IActivityBuilder IInput<IActivityBuilder>.AddInput(InputBuildAction buildAction)
            => AddInput(buildAction) as IActivityBuilder;

        IActivityBuilder IOutput<IActivityBuilder>.AddOutput()
            => AddOutput() as IActivityBuilder;

        IActivityBuilder IReactiveActivity<IActivityBuilder>.AddParallelActivity<TParallelizationToken>(string actionNodeName, ParallelActivityBuildAction buildAction, int chunkSize)
            => AddParallelActivity<TParallelizationToken>(actionNodeName, buildAction, chunkSize) as IActivityBuilder;

        IActivityBuilder IReactiveActivity<IActivityBuilder>.AddIterativeActivity<TIterationToken>(string actionNodeName, IterativeActivityBuildAction buildAction, int chunkSize)
            => AddIterativeActivity<TIterationToken>(actionNodeName, buildAction, chunkSize) as IActivityBuilder;

        IActivityBuilder IAcceptEvent<IActivityBuilder>.AddAcceptEventAction<TEvent>(string actionNodeName, AcceptEventActionDelegateAsync<TEvent> eventActionAsync, AcceptEventActionBuildAction buildAction)
            => AddAcceptEventAction<TEvent>(actionNodeName, eventActionAsync, buildAction) as IActivityBuilder;

        IActivityBuilder ISendEvent<IActivityBuilder>.AddSendEventAction<TEvent>(string actionNodeName, SendEventActionDelegateAsync<TEvent> actionAsync, BehaviorIdSelectorAsync targetSelectorAsync, SendEventActionBuildAction buildAction)
            => AddSendEventAction<TEvent>(actionNodeName, actionAsync, targetSelectorAsync, buildAction) as IActivityBuilder;
        #endregion

        #region ITypedActivityBuilder
        ITypedActivityBuilder IReactiveActivity<ITypedActivityBuilder>.AddAction(string actionNodeName, ActionDelegateAsync actionAsync, ActionBuildAction buildAction)
            => AddAction(actionNodeName, actionAsync, b => buildAction?.Invoke(b)) as ITypedActivityBuilder;

        ITypedActivityBuilder IReactiveActivity<ITypedActivityBuilder>.AddStructuredActivity(string actionNodeName, ReactiveStructuredActivityBuildAction buildAction)
            => AddStructuredActivity(actionNodeName, buildAction) as ITypedActivityBuilder;

        ITypedActivityBuilder IInitial<ITypedActivityBuilder>.AddInitial(InitialBuildAction buildAction)
            => AddInitial(buildAction) as ITypedActivityBuilder;

        ITypedActivityBuilder IFinal<ITypedActivityBuilder>.AddFinal()
            => AddFinal() as ITypedActivityBuilder;

        ITypedActivityBuilder IInput<ITypedActivityBuilder>.AddInput(InputBuildAction buildAction)
            => AddInput(buildAction) as ITypedActivityBuilder;

        ITypedActivityBuilder IOutput<ITypedActivityBuilder>.AddOutput()
            => AddOutput() as ITypedActivityBuilder;

        ITypedActivityBuilder IReactiveActivity<ITypedActivityBuilder>.AddParallelActivity<TParallelizationToken>(string actionNodeName, ParallelActivityBuildAction buildAction, int chunkSize)
            => AddParallelActivity<TParallelizationToken>(actionNodeName, buildAction, chunkSize) as ITypedActivityBuilder;

        ITypedActivityBuilder IReactiveActivity<ITypedActivityBuilder>.AddIterativeActivity<TIterationToken>(string actionNodeName, IterativeActivityBuildAction buildAction, int chunkSize)
            => AddIterativeActivity<TIterationToken>(actionNodeName, buildAction, chunkSize) as ITypedActivityBuilder;

        ITypedActivityBuilder IAcceptEvent<ITypedActivityBuilder>.AddAcceptEventAction<TEvent>(string actionNodeName, AcceptEventActionDelegateAsync<TEvent> eventActionAsync, AcceptEventActionBuildAction buildAction)
            => AddAcceptEventAction<TEvent>(actionNodeName, eventActionAsync, buildAction) as ITypedActivityBuilder;

        ITypedActivityBuilder ISendEvent<ITypedActivityBuilder>.AddSendEventAction<TEvent>(string actionNodeName, SendEventActionDelegateAsync<TEvent> actionAsync, BehaviorIdSelectorAsync targetSelectorAsync, SendEventActionBuildAction buildAction)
            => AddSendEventAction<TEvent>(actionNodeName, actionAsync, targetSelectorAsync, buildAction) as ITypedActivityBuilder;
        #endregion

        #region Observability
        public IActivityBuilder AddExceptionHandler<TExceptionHandler>()
            where TExceptionHandler : class, IActivityExceptionHandler
        {
            Services.RegisterExceptionHandler<TExceptionHandler>();
            AddExceptionHandler(serviceProvider => serviceProvider.GetRequiredService<TExceptionHandler>());

            return this;
        }

        public IActivityBuilder AddExceptionHandler(ActivityExceptionHandlerFactory exceptionHandlerFactory)
        {
            Result.ExceptionHandlerFactories.Add(exceptionHandlerFactory);

            return this;
        }

        public IActivityBuilder AddInterceptor<TInterceptor>()
            where TInterceptor : class, IActivityInterceptor
        {
            Services.RegisterInterceptor<TInterceptor>();
            AddInterceptor(serviceProvider => serviceProvider.GetRequiredService<TInterceptor>());

            return this;
        }

        public IActivityBuilder AddInterceptor(ActivityInterceptorFactory interceptorFactory)
        {
            Result.InterceptorFactories.Add(interceptorFactory);

            return this;
        }

        public IActivityBuilder AddObserver<TObserver>()
            where TObserver : class, IActivityObserver
        {
            Services.RegisterObserver<TObserver>();
            AddObserver(serviceProvider => serviceProvider.GetRequiredService<TObserver>());

            return this;
        }

        public IActivityBuilder AddObserver(ActivityObserverFactory observerFactory)
        {
            Result.ObserverFactories.Add(observerFactory);

            return this;
        }

        ITypedActivityBuilder IActivityUtils<ITypedActivityBuilder>.AddExceptionHandler<TExceptionHandler>()
            => AddExceptionHandler<TExceptionHandler>() as ITypedActivityBuilder;

        ITypedActivityBuilder IActivityUtils<ITypedActivityBuilder>.AddExceptionHandler(ActivityExceptionHandlerFactory exceptionHandlerFactory)
            => AddExceptionHandler(exceptionHandlerFactory) as ITypedActivityBuilder;

        ITypedActivityBuilder IActivityUtils<ITypedActivityBuilder>.AddInterceptor<TInterceptor>()
            => AddInterceptor<TInterceptor>() as ITypedActivityBuilder;

        ITypedActivityBuilder IActivityUtils<ITypedActivityBuilder>.AddInterceptor(ActivityInterceptorFactory interceptorFactory)
            => AddInterceptor(interceptorFactory) as ITypedActivityBuilder;

        ITypedActivityBuilder IActivityUtils<ITypedActivityBuilder>.AddObserver<TObserver>()
            => AddObserver<TObserver>() as ITypedActivityBuilder;

        ITypedActivityBuilder IActivityUtils<ITypedActivityBuilder>.AddObserver(ActivityObserverFactory observerFactory)
            => AddObserver(observerFactory) as ITypedActivityBuilder;
        #endregion
    }
}

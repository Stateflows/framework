using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Models;
using Stateflows.Common.Extensions;
using Stateflows.Activities.Models;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Activities.Engine;
using Stateflows.Activities.Registration.Extensions;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.Activities.Registration.Interfaces.Base;
using Stateflows.Activities.Utils;
using Stateflows.Common.Exceptions;
using Stateflows.Common.Registration.Builders;

namespace Stateflows.Activities.Registration.Builders
{
    internal class ActivityBuilder :
        BaseActivityBuilder,
        IActivityBuilder
    {
        new public Graph Result
        {
            get => Node as Graph;
            set => Node = value;
        }

        public ActivityBuilder(string name, int version, Node parentNode, StateflowsBuilder stateflowsBuilder, IServiceCollection services)
            : base(parentNode, services)
        {
            Result = new Graph(name, version, stateflowsBuilder);
        }

        public IActivityBuilder AddInitializer(Type initializerType, string initializerName, ActivityPredicateAsync initializerAction)
        {
            if (!Result.Initializers.TryGetValue(initializerName, out var initializer))
            {
                initializer = new Logic<ActivityPredicateAsync>(Constants.Initialize);

                Result.Initializers.Add(initializerName, initializer);
                Result.InitializerTypes.Add(initializerType);
            }

            initializer.Actions.Add(initializerAction);

            return this;
        }

        public IActivityBuilder AddDefaultInitializer(Func<IActivityInitializationContext, Task<bool>> actionAsync)
        {
            Result.DefaultInitializer = new Logic<ActivityPredicateAsync>(Constants.Initialize);

            Result.DefaultInitializer.Actions.Add(c =>
            {
                var context = new ActivityInitializationContext(
                    c.Context,
                    c.NodeScope,
                    (c as ActivityInitializationContext).InputTokens
                );
                return actionAsync(context);
            });

            return this;
        }

        public IActivityBuilder AddInitializer<TInitializationEvent>(Func<IActivityInitializationContext<TInitializationEvent>, Task<bool>> actionAsync)
        {
            actionAsync.ThrowIfNull(nameof(actionAsync));

            actionAsync = actionAsync.AddActivityInvocationContext(Result);

            var initializerName = typeof(TInitializationEvent).GetEventName();

            return AddInitializer(typeof(TInitializationEvent), initializerName, async c =>
            {
                var result = false;
                var context = new ActivityInitializationContext<TInitializationEvent>(
                    c.Context,
                    c.NodeScope,
                    c.Context.EventHolder as EventHolder<TInitializationEvent>,
                    (c as ActivityInitializationContext).InputTokens
                );

                try
                {
                    result = await actionAsync(context);
                }
                catch (Exception e)
                {
                    if (e is StateflowsDefinitionException)
                    {
                        throw;
                    }
                    else
                    {
                        if (!await c.Context.Executor.Inspector.OnActivityInitializationExceptionAsync(context, context.InitializationEventHolder, e))
                        {
                            throw;
                        }
                        else
                        {
                            throw new BehaviorExecutionException(e);
                        }
                    }
                }

                return result;
            });
        }

        #region IActivityBuilder
        IActivityBuilder IReactiveActivity<IActivityBuilder>.AddAction(string actionNodeName, ActionDelegateAsync actionAsync, ActionBuildAction buildAction)
            => AddAction(actionNodeName, actionAsync, b => buildAction?.Invoke(b)) as IActivityBuilder;

        IActivityBuilder IReactiveActivity<IActivityBuilder>.AddStructuredActivity(string actionNodeName, ReactiveStructuredActivityBuildAction buildAction)
            => AddStructuredActivity(actionNodeName, buildAction) as IActivityBuilder;

        IActivityBuilder IActivityEvents<IActivityBuilder>.AddFinalizer(Func<IActivityActionContext, Task> actionAsync)
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

        IActivityBuilder IAcceptEvent<IActivityBuilder>.AddTimeEventAction<TTimeEvent>(string actionNodeName, TimeEventActionDelegateAsync eventActionAsync, AcceptEventActionBuildAction buildAction)
            => AddTimeEventAction<TTimeEvent>(actionNodeName, eventActionAsync, buildAction) as IActivityBuilder;

        IActivityBuilder ISendEvent<IActivityBuilder>.AddSendEventAction<TEvent>(string actionNodeName, SendEventActionDelegateAsync<TEvent> actionAsync, BehaviorIdSelectorAsync targetSelectorAsync, SendEventActionBuildAction buildAction)
            => AddSendEventAction<TEvent>(actionNodeName, actionAsync, targetSelectorAsync, buildAction) as IActivityBuilder;
        #endregion

        #region Observability
        public IActivityBuilder AddExceptionHandler<TExceptionHandler>()
            where TExceptionHandler : class, IActivityExceptionHandler
        {
            Services.AddServiceType<TExceptionHandler>();
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
            Services.AddServiceType<TInterceptor>();
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
            Services.AddServiceType<TObserver>();
            AddObserver(serviceProvider => serviceProvider.GetRequiredService<TObserver>());

            return this;
        }

        public IActivityBuilder AddObserver(ActivityObserverFactory observerFactory)
        {
            Result.ObserverFactories.Add(observerFactory);

            return this;
        }
        #endregion
    }
}

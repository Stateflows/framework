using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Models;
using Stateflows.Common.Classes;
using Stateflows.Common.Exceptions;
using Stateflows.Common.Registration.Builders;
using Stateflows.Activities.Models;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Activities.Registration.Extensions;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.Activities.Registration.Interfaces.Base;
using Stateflows.Activities.Registration.Interfaces.Internal;
using Stateflows.Common.Registration;

namespace Stateflows.Activities.Registration.Builders
{
    internal class ActivityBuilder :
        BaseActivityBuilder,
        IActivityBuilder,
        IGraphBuilder,
        IBehaviorBuilder
    {
        public new Graph Graph
        {
            get => Node as Graph;
            set => Node = value;
        }

        public ActivityBuilder(string name, int version, Node parentNode, StateflowsBuilder stateflowsBuilder, IServiceCollection services)
            : base(parentNode, services)
        {
            Graph = new Graph(name, version, stateflowsBuilder);
        }

        private IActivityBuilder AddInitializer(Type initializerType, string initializerName, ActivityPredicateAsync initializerAction)
        {
            if (!Graph.Initializers.TryGetValue(initializerName, out var initializer))
            {
                initializer = new Logic<ActivityPredicateAsync>(Constants.Initialize);

                Graph.Initializers.Add(initializerName, initializer);
                Graph.InitializerTypes.Add(initializerType);
            }

            initializer.Actions.Add(initializerAction);

            return this;
        }

        public IActivityBuilder AddDefaultInitializer(Func<IActivityInitializationContext, Task<bool>> actionAsync)
        {
            Graph.DefaultInitializer = new Logic<ActivityPredicateAsync>(Constants.Initialize);

            Graph.DefaultInitializer.Actions.Add(c =>
            {
                var context = new ActivityInitializationContext(
                    c.Context,
                    c.NodeScope,
                    (c as ActivityInitializationContext)?.InputTokens
                );
                return actionAsync(context);
            });
            
            Graph.VisitingTasks.Add(v => v.DefaultInitializerAddedAsync(Graph.Name, Graph.Version));

            return this;
        }

        public IActivityBuilder AddInitializer<TInitializationEvent>(Func<IActivityInitializationContext<TInitializationEvent>, Task<bool>> actionAsync)
        {
            actionAsync.ThrowIfNull(nameof(actionAsync));

            actionAsync = actionAsync.AddActivityInvocationContext(Graph);

            var initializerName = typeof(TInitializationEvent).GetEventName();
            
            if (!Graph.Initializers.TryGetValue(initializerName, out var initializer))
            {
                initializer = new Logic<ActivityPredicateAsync>(Constants.Initialize);

                Graph.Initializers.Add(initializerName, initializer);
                Graph.InitializerTypes.Add(typeof(TInitializationEvent));
            }

            initializer.Actions.Add(async c =>
            {
                var result = false;
                var context = new ActivityInitializationContext<TInitializationEvent>(
                    c.Context,
                    c.NodeScope,
                    c.Context.EventHolder as EventHolder<TInitializationEvent>,
                    (c as ActivityInitializationContext)?.InputTokens
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
                        Trace.WriteLine($"⦗→s⦘ Activity '{c.Context.Id.Name}:{c.Context.Id.Instance}': exception '{e.GetType().FullName}' thrown with message '{e.Message}'");
                        if (!c.Context.Executor.Inspector.OnActivityInitializationException(context, context.InitializationEventHolder, e))
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
            
            Graph.VisitingTasks.Add(v => v.InitializerAddedAsync<TInitializationEvent>(Graph.Name, Graph.Version));

            return this;
        }

        #region IActivityBuilder
        IActivityBuilder IReactiveActivity<IActivityBuilder>.AddAction(string actionNodeName, Func<IActionContext, Task> actionAsync, ActionBuildAction buildAction)
            => AddAction(actionNodeName, actionAsync, b => buildAction?.Invoke(b)) as IActivityBuilder;

        IActivityBuilder IReactiveActivity<IActivityBuilder>.AddStructuredActivity(string actionNodeName, ReactiveStructuredActivityBuildAction buildAction)
            => AddStructuredActivity(actionNodeName, buildAction) as IActivityBuilder;

        IActivityBuilder IActivityEvents<IActivityBuilder>.AddFinalizer(Func<IActivityActionContext, Task> actionAsync)
        {
            var result = AddOnFinalize(actionAsync) as IActivityBuilder;
            
            Graph.VisitingTasks.Add(v => v.FinalizerAddedAsync(Graph.Name, Graph.Version));

            return result;
        }

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
            AddExceptionHandler(async serviceProvider => await StateflowsActivator.CreateInstanceAsync<TExceptionHandler>(serviceProvider, "exception handler"));

            return this;
        }

        public IActivityBuilder AddExceptionHandler(ActivityExceptionHandlerFactory exceptionHandlerFactory)
        {
            Graph.ExceptionHandlerFactories.Add(serviceProvider => Task.FromResult(exceptionHandlerFactory(serviceProvider)));

            return this;
        }

        public IActivityBuilder AddExceptionHandler(ActivityExceptionHandlerFactoryAsync exceptionHandlerFactoryAsync)
        {
            Graph.ExceptionHandlerFactories.Add(exceptionHandlerFactoryAsync);

            return this;
        }

        public IActivityBuilder AddInterceptor<TInterceptor>()
            where TInterceptor : class, IActivityInterceptor
        {
            AddInterceptor(async serviceProvider => await StateflowsActivator.CreateInstanceAsync<TInterceptor>(serviceProvider, "interceptor"));

            return this;
        }

        public IActivityBuilder AddInterceptor(ActivityInterceptorFactory interceptorFactory)
        {
            Graph.InterceptorFactories.Add(serviceProvider => Task.FromResult(interceptorFactory(serviceProvider)));

            return this;
        }

        public IActivityBuilder AddInterceptor(ActivityInterceptorFactoryAsync interceptorFactoryAsync)
        {
            Graph.InterceptorFactories.Add(interceptorFactoryAsync);

            return this;
        }

        public IActivityBuilder AddObserver<TObserver>()
            where TObserver : class, IActivityObserver
        {
            AddObserver(async serviceProvider => await StateflowsActivator.CreateInstanceAsync<TObserver>(serviceProvider, "observer"));

            return this;
        }

        public IActivityBuilder AddObserver(ActivityObserverFactory observerFactory)
        {
            Graph.ObserverFactories.Add(serviceProvider => Task.FromResult(observerFactory(serviceProvider)));

            return this;
        }

        public IActivityBuilder AddObserver(ActivityObserverFactoryAsync observerFactoryAsync)
        {
            Graph.ObserverFactories.Add(observerFactoryAsync);

            return this;
        }
        #endregion

        public BehaviorClass BehaviorClass => Graph.Class;
        public int BehaviorVersion => Graph.Version;
    }
}

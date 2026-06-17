using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Common.Models;
using Stateflows.Common.Classes;
using Stateflows.Common.Utilities;
using Stateflows.Common.Exceptions;
using Stateflows.Common.Registration;
using Stateflows.Common.Registration.Builders;
using Stateflows.Activities.Models;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.Activities.Registration.Interfaces.Base;
using Stateflows.Activities.Registration.Interfaces.Internal;

namespace Stateflows.Activities.Registration.Builders
{
    internal class ActivityBuilder :
        BaseActivityBuilder,
        IActivityBuilder,
        IOverridenActivityBuilder,
        IGraphBuilder,
        IBehaviorBuilder
    {
        public new Graph Graph
        {
            get => Node as Graph;
            set => Node = value;
        }

        public ActivityBuilder(string name, int version, Node parentNode, StateflowsBuilder stateflowsBuilder, BehaviorClass? ownerClass, BehaviorClass? parentClass)
            : base(parentNode)
        {
            Graph = new Graph(name, version, stateflowsBuilder, ownerClass, parentClass);
        }

        // private IActivityBuilder AddInitializer(Type initializerType, string initializerName, ActivityPredicateAsync initializerAction)
        // {
        //     if (!Graph.Initializers.TryGetValue(initializerName, out var initializer))
        //     {
        //         initializer = new Logic<ActivityPredicateAsync>(Constants.Initialize);
        //
        //         Graph.Initializers.Add(initializerName, initializer);
        //         Graph.InitializerTypes.Add(initializerType);
        //     }
        //
        //     initializer.Actions.Add(initializerAction);
        //
        //     return this;
        // }

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

        IOverridenActivityBuilder IActivityEvents<IOverridenActivityBuilder>.AddInitializer<TInitializationEvent>(Func<IActivityInitializationContext<TInitializationEvent>, Task<bool>> actionAsync)
            => AddInitializer<TInitializationEvent>(actionAsync) as IOverridenActivityBuilder;

        IOverridenActivityBuilder IActivityEvents<IOverridenActivityBuilder>.AddFinalizer(Func<IActivityActionContext, Task> actionAsync)
            => AddFinalizer(actionAsync);

        IOverridenActivityBuilder IActivityEvents<IOverridenActivityBuilder>.AddDefaultInitializer(Func<IActivityInitializationContext, Task<bool>> actionAsync)
            => AddDefaultInitializer(actionAsync) as IOverridenActivityBuilder;

        public IActivityBuilder AddInitializer<TInitializationEvent>(Func<IActivityInitializationContext<TInitializationEvent>, Task<bool>> actionAsync)
        {
            actionAsync.ThrowIfNull(nameof(actionAsync));

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
        IActivityBuilder IActivityActionBase<IActivityBuilder>.AddAction(string actionNodeName, Func<IActionContext, Task> actionAsync, ActionBuildAction buildAction)
            => AddAction(actionNodeName, actionAsync, b => buildAction?.Invoke(b)) as IActivityBuilder;

        IActivityBuilder IReactiveActivityBase<IActivityBuilder>.AddStructuredActivity(string actionNodeName, ReactiveStructuredActivityBuildAction buildAction)
            => AddStructuredActivity(actionNodeName, buildAction) as IActivityBuilder;

        IOverridenActivityBuilder IReactiveActivityBase<IOverridenActivityBuilder>.AddParallelActivity<TParallelizationToken>(string actionNodeName,
            ParallelActivityBuildAction buildAction, int chunkSize)
            => AddParallelActivity<TParallelizationToken>(actionNodeName, buildAction, chunkSize) as IOverridenActivityBuilder;

        IOverridenActivityBuilder IReactiveActivityBase<IOverridenActivityBuilder>.AddIterativeActivity<TToken>(string actionNodeName, IterativeActivityBuildAction buildAction,
            int chunkSize)
            => AddIterativeActivity<TToken>(actionNodeName, buildAction, chunkSize) as IOverridenActivityBuilder;

        IActivityBuilder IActivityEvents<IActivityBuilder>.AddFinalizer(Func<IActivityActionContext, Task> actionAsync)
            => AddFinalizer(actionAsync);
        
        private ActivityBuilder AddFinalizer(Func<IActivityActionContext, Task> actionAsync)
        {
            var result = AddOnFinalize(actionAsync) as ActivityBuilder;
            
            Graph.VisitingTasks.Add(v => v.FinalizerAddedAsync(Graph.Name, Graph.Version));

            return result;
        }

        IActivityBuilder IInitialBase<IActivityBuilder>.AddInitial(InitialBuildAction buildAction)
            => AddInitial(buildAction) as IActivityBuilder;

        IActivityBuilder IFinalBase<IActivityBuilder>.AddFinal()
            => AddFinal() as IActivityBuilder;

        IActivityBuilder IInputBase<IActivityBuilder>.AddInput(InputBuildAction buildAction)
            => AddInput(buildAction) as IActivityBuilder;

        IActivityBuilder IOutputBase<IActivityBuilder>.AddOutput()
            => AddOutput() as IActivityBuilder;

        IOverridenActivityBuilder IReactiveActivityBase<IOverridenActivityBuilder>.AddStructuredActivity(string actionNodeName,
            ReactiveStructuredActivityBuildAction buildAction)
            => AddStructuredActivity(actionNodeName, buildAction) as IOverridenActivityBuilder;

        IActivityBuilder IReactiveActivityBase<IActivityBuilder>.AddParallelActivity<TParallelizationToken>(string actionNodeName, ParallelActivityBuildAction buildAction, int chunkSize)
            => AddParallelActivity<TParallelizationToken>(actionNodeName, buildAction, chunkSize) as IActivityBuilder;

        IActivityBuilder IReactiveActivityBase<IActivityBuilder>.AddIterativeActivity<TIterationToken>(string actionNodeName, IterativeActivityBuildAction buildAction, int chunkSize)
            => AddIterativeActivity<TIterationToken>(actionNodeName, buildAction, chunkSize) as IActivityBuilder;

        IActivityBuilder IAcceptEventBase<IActivityBuilder>.AddAcceptEventAction<TEvent>(string actionNodeName, AcceptEventActionDelegateAsync<TEvent> eventActionAsync, AcceptEventActionBuildAction<TEvent> buildAction)
            => AddAcceptEventAction<TEvent>(actionNodeName, eventActionAsync, buildAction) as IActivityBuilder;

        IOverridenActivityBuilder IAcceptEventBase<IOverridenActivityBuilder>.AddTimeEventAction<TTimeEvent>(string actionNodeName,
            TimeEventActionDelegateAsync eventActionAsync, TimeEventNodeBuildAction buildAction)
            => AddTimeEventAction<TTimeEvent>(actionNodeName, eventActionAsync, buildAction) as IOverridenActivityBuilder;

        IOverridenActivityBuilder IAcceptEventBase<IOverridenActivityBuilder>.AddAcceptEventAction<TEvent>(string actionNodeName,
            AcceptEventActionDelegateAsync<TEvent> eventActionAsync, AcceptEventActionBuildAction<TEvent> buildAction)
            => AddAcceptEventAction<TEvent>(actionNodeName, eventActionAsync, buildAction) as IOverridenActivityBuilder;

        IActivityBuilder IAcceptEventBase<IActivityBuilder>.AddTimeEventAction<TTimeEvent>(string actionNodeName, TimeEventActionDelegateAsync eventActionAsync, TimeEventNodeBuildAction buildAction)
            => AddTimeEventAction<TTimeEvent>(actionNodeName, eventActionAsync, buildAction) as IActivityBuilder;

        IActivityBuilder ISendEventBase<IActivityBuilder>.AddSendEventAction<TEvent>(string actionNodeName, SendEventActionDelegateAsync<TEvent> actionAsync, BehaviorIdSelectorAsync targetSelectorAsync, SendEventActionBuildAction buildAction)
            => AddSendEventAction<TEvent>(actionNodeName, actionAsync, targetSelectorAsync, buildAction) as IActivityBuilder;
        #endregion

        #region Observability
        public IActivityBuilder AddExceptionHandler<TExceptionHandler>()
            where TExceptionHandler : class, IActivityExceptionHandler
        {
            AddExceptionHandler(async serviceProvider => await StateflowsActivator.CreateModelElementInstanceAsync<TExceptionHandler>(serviceProvider, "exception handler"));

            return this;
        }

        IOverridenActivityBuilder IActivityUtils<IOverridenActivityBuilder>.SetResourceName(string resourceName)
            => SetResourceName(resourceName) as IOverridenActivityBuilder;

        IOverridenActivityBuilder IActivityUtils<IOverridenActivityBuilder>.AddExceptionHandler<TExceptionHandler>()
            => AddExceptionHandler<TExceptionHandler>() as  IOverridenActivityBuilder;

        public IActivityBuilder SetResourceName(string resourceName)
        {
            Graph.ResourceName = resourceName;

            return this;
        }

        IOverridenActivityBuilder IActivityUtils<IOverridenActivityBuilder>.AddExceptionHandler(ActivityExceptionHandlerFactoryAsync exceptionHandlerFactory)
            => AddExceptionHandler(exceptionHandlerFactory) as IOverridenActivityBuilder;

        public IActivityBuilder AddExceptionHandler(ActivityExceptionHandlerFactory exceptionHandlerFactory)
        {
            Graph.ExceptionHandlerFactories.Add(serviceProvider => Task.FromResult(exceptionHandlerFactory(serviceProvider)));

            return this;
        }

        IOverridenActivityBuilder IActivityUtils<IOverridenActivityBuilder>.AddObserver<TObserver>()
            => AddObserver<TObserver>() as IOverridenActivityBuilder;

        public IActivityBuilder AddExceptionHandler(ActivityExceptionHandlerFactoryAsync exceptionHandlerFactoryAsync)
        {
            Graph.ExceptionHandlerFactories.Add(exceptionHandlerFactoryAsync);

            return this;
        }

        public IActivityBuilder AddInterceptor<TInterceptor>()
            where TInterceptor : class, IActivityInterceptor
        {
            AddInterceptor(async serviceProvider => await StateflowsActivator.CreateModelElementInstanceAsync<TInterceptor>(serviceProvider, "interceptor"));

            return this;
        }

        IOverridenActivityBuilder IActivityUtils<IOverridenActivityBuilder>.AddObserver(ActivityObserverFactoryAsync observerFactory)
            => AddObserver(observerFactory) as IOverridenActivityBuilder;

        IOverridenActivityBuilder IActivityUtils<IOverridenActivityBuilder>.AddInterceptor(ActivityInterceptorFactoryAsync interceptorFactory)
            => AddInterceptor(interceptorFactory) as IOverridenActivityBuilder;

        IOverridenActivityBuilder IActivityUtils<IOverridenActivityBuilder>.AddInterceptor<TInterceptor>()
            => AddInterceptor<TInterceptor>() as IOverridenActivityBuilder;

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
            AddObserver(async serviceProvider => await StateflowsActivator.CreateModelElementInstanceAsync<TObserver>(serviceProvider, "observer"));

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
        public IOverridenActivityBuilder UseActivity<TActivity>(OverridenActivityBuildAction buildAction)
            where TActivity : class, IActivity
        {
            Graph.BaseActivityName = Activity<TActivity>.Name;
            TActivity.Build(this);
            
            foreach (var node in Graph.AllNodes.Values)
            {
                node.OriginActivityName ??= Graph.BaseActivityName;
            }
            
            foreach (var edge in Graph.AllEdgesList)
            {
                edge.OriginActivityName ??= Graph.BaseActivityName;
            }
            
            buildAction?.Invoke(this);

            return this;
        }

        IOverridenActivityBuilder IActivityActionBase<IOverridenActivityBuilder>.AddAction(string actionNodeName, Func<IActionContext, Task> actionAsync, ActionBuildAction buildAction)
            => AddAction(actionNodeName, actionAsync, b => buildAction?.Invoke(b)) as IOverridenActivityBuilder;

        IOverridenActivityBuilder IInitialBase<IOverridenActivityBuilder>.AddInitial(InitialBuildAction buildAction)
            => AddInitial(buildAction) as IOverridenActivityBuilder;

        IOverridenActivityBuilder IFinalBase<IOverridenActivityBuilder>.AddFinal()
            => AddFinal() as IOverridenActivityBuilder;

        IOverridenActivityBuilder IInputBase<IOverridenActivityBuilder>.AddInput(InputBuildAction buildAction)
            => AddInput(buildAction) as IOverridenActivityBuilder;

        IOverridenActivityBuilder IOutputBase<IOverridenActivityBuilder>.AddOutput()
            => AddOutput() as IOverridenActivityBuilder;

        IOverridenActivityBuilder ISendEventBase<IOverridenActivityBuilder>.AddSendEventAction<TEvent>(string actionNodeName, SendEventActionDelegateAsync<TEvent> actionAsync,
            BehaviorIdSelectorAsync targetSelectorAsync, SendEventActionBuildAction buildAction)
            => AddSendEventAction<TEvent>(actionNodeName, actionAsync, targetSelectorAsync, buildAction) as IOverridenActivityBuilder;

        IOverridenActivityBuilder IActivitySpecialsOverrides<IOverridenActivityBuilder>.UseInitial(OverridenInitialBuildAction buildAction)
            => UseInitial(buildAction) as IOverridenActivityBuilder;

        IOverridenActivityBuilder IActivitySpecialsOverrides<IOverridenActivityBuilder>.UseInput(OverridenInputBuildAction buildAction)
            => UseInput(buildAction) as IOverridenActivityBuilder;

        IOverridenActivityBuilder IActivitySpecialsOverrides<IOverridenActivityBuilder>.UseJoin(string joinNodeName, OverridenJoinBuildAction buildAction)
            => UseJoin(joinNodeName, buildAction) as IOverridenActivityBuilder;

        IOverridenActivityBuilder IActivitySpecialsOverrides<IOverridenActivityBuilder>.UseFork(string forkNodeName, OverridenForkBuildAction buildAction)
            => UseFork(forkNodeName, buildAction) as IOverridenActivityBuilder;

        IOverridenActivityBuilder IActivitySpecialsOverrides<IOverridenActivityBuilder>.UseMerge(string mergeNodeName, OverridenMergeBuildAction buildAction)
            => UseMerge(mergeNodeName, buildAction) as IOverridenActivityBuilder;

        IOverridenActivityBuilder IActivitySpecialsOverrides<IOverridenActivityBuilder>.UseControlDecision(string decisionNodeName, OverridenDecisionBuildAction buildAction)
            => UseControlDecision(decisionNodeName, buildAction) as IOverridenActivityBuilder;

        IOverridenActivityBuilder IActivitySpecialsOverrides<IOverridenActivityBuilder>.UseDecision<TToken>(string decisionNodeName, OverridenDecisionBuildAction<TToken> decisionBuildAction)
            => UseDecision(decisionNodeName, decisionBuildAction) as IOverridenActivityBuilder;

        IOverridenActivityBuilder IActivitySpecialsOverrides<IOverridenActivityBuilder>.UseDataStore(string dataStoreNodeName, OverridenDataStoreBuildAction buildAction)
            => UseDataStore(dataStoreNodeName, buildAction) as IOverridenActivityBuilder;

        // public IOverridenActivityBuilder UseAcceptEventAction<TEvent>(string actionNodeName,
        //     OverridenAcceptEventActionBuildAction<TEvent> buildAction)
        //     => UseAcceptEventAction<TEvent>(actionNodeName, buildAction);

        // public IOverridenActivityBuilder UseTimeEventAction<TTimeEvent>(string actionNodeName,
        //     OverridenTimeEventNodeBuildAction buildAction) where TTimeEvent : TimeEvent, new()
        //     => UseTimeEventAction<TTimeEvent>(actionNodeName, buildAction);

        IOverridenActivityBuilder IPublishEventBase<IOverridenActivityBuilder>.AddPublishEventAction<TEvent>(string actionNodeName,
            PublishEventActionDelegateAsync<TEvent> actionAsync, PublishEventActionBuildAction buildAction)
            => AddPublishEventAction<TEvent>(actionNodeName, actionAsync, buildAction) as IOverridenActivityBuilder;

        IOverridenActivityBuilder IReactiveActivityOverrides<IOverridenActivityBuilder>.UseParallelActivity<TParallelizationToken>(string actionNodeName,
            OverridenParallelActivityBuildAction buildAction)
            => UseParallelActivity<TParallelizationToken>(actionNodeName, buildAction) as IOverridenActivityBuilder;

        IOverridenActivityBuilder IReactiveActivityOverrides<IOverridenActivityBuilder>.UseIterativeActivity<TToken>(string actionNodeName,
            OverridenIterativeActivityBuildAction buildAction)
            => UseIterativeActivity<TToken>(actionNodeName, buildAction) as IOverridenActivityBuilder;

        IOverridenActivityBuilder IReactiveActivityOverrides<IOverridenActivityBuilder>.UseStructuredActivity(string structuredActivityNodeName,
            OverridenReactiveStructuredActivityBuildAction buildAction)
            => UseStructuredActivity(structuredActivityNodeName, b => buildAction?.Invoke(b as IOverridenReactiveStructuredActivityBuilder)) as IOverridenActivityBuilder;

        IOverridenActivityBuilder IActivityActionOverrides<IOverridenActivityBuilder>.UseAction(string actionNodeName, OverridenActionBuildAction buildAction)
            => UseAction(actionNodeName, buildAction) as IOverridenActivityBuilder;

        IActivityBuilder IPublishEventBase<IActivityBuilder>.AddPublishEventAction<TEvent>(string actionNodeName,
            PublishEventActionDelegateAsync<TEvent> actionAsync,
            PublishEventActionBuildAction buildAction)
            => AddPublishEventAction<TEvent>(actionNodeName, actionAsync, buildAction) as IActivityBuilder;

        IOverridenActivityBuilder ISendEventOverrides<IOverridenActivityBuilder>.UseSendEventAction<TEvent>(string actionNodeName, OverridenSendEventActionBuildAction buildAction)
            => UseSendEventAction<TEvent>(actionNodeName, buildAction) as IOverridenActivityBuilder;

        IOverridenActivityBuilder IPublishEventOverrides<IOverridenActivityBuilder>.UsePublishEventAction<TEvent>(string actionNodeName,
            OverridenPublishEventActionBuildAction buildAction)
            => UsePublishEventAction<TEvent>(actionNodeName, buildAction) as IOverridenActivityBuilder;

        IOverridenActivityBuilder IAcceptEventOverrides<IOverridenActivityBuilder>.UseAcceptEventAction<TEvent>(string actionNodeName,
            OverridenAcceptEventActionBuildAction<TEvent> buildAction)
            => UseAcceptEventAction<TEvent>(actionNodeName, buildAction) as IOverridenActivityBuilder;

        IOverridenActivityBuilder IAcceptEventOverrides<IOverridenActivityBuilder>.UseTimeEventAction<TTimeEvent>(string actionNodeName,
            OverridenTimeEventNodeBuildAction buildAction)
            => UseTimeEventAction<TTimeEvent>(actionNodeName, buildAction) as IOverridenActivityBuilder;
    }
}

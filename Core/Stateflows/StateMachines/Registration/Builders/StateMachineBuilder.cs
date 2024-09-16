using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Models;
using Stateflows.Common.Extensions;
using Stateflows.Common.Registration;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Interfaces;
using Stateflows.StateMachines.Exceptions;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Base;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Registration.Builders
{
    internal class StateMachineBuilder :
        IInitializedStateMachineBuilder,
        IFinalizedStateMachineBuilder,
        IStateMachineBuilder,
        IInternal,
        IBehaviorBuilder
    {
        public Graph Result { get; }

        public IServiceCollection Services { get; }

        BehaviorClass IBehaviorBuilder.BehaviorClass => new BehaviorClass(Constants.StateMachine, Result.Name);

        int IBehaviorBuilder.BehaviorVersion => Result.Version;

        public StateMachineBuilder(string name, int version, IServiceCollection services)
        {
            Services = services;
            Result = new Graph(name, version);
        }

        public IInitializedStateMachineBuilder AddInitializer(Type initializerType, string initializerName, StateMachinePredicateAsync initializerAction)
        {
            if (!Result.Initializers.TryGetValue(initializerName, out var initializer))
            {
                initializer = new Logic<StateMachinePredicateAsync>()
                {
                    Name = Constants.Initialize
                };

                Result.Initializers.Add(initializerName, initializer);
                Result.InitializerTypes.Add(initializerType);
            }

            initializer.Actions.Add(initializerAction);

            return this;
        }

        public IInitializedStateMachineBuilder AddDefaultInitializer(Func<IStateMachineInitializationContext, Task<bool>> actionAsync)
        {
            Result.DefaultInitializer = new Logic<StateMachinePredicateAsync>()
            {
                Name = Constants.Initialize
            };

            Result.DefaultInitializer.Actions.Add(c =>
            {
                var context = new StateMachineInitializationContext(c);
                return actionAsync(context);
            });

            return this;
        }

        public IInitializedStateMachineBuilder AddInitializer<TInitializationEvent>(Func<IStateMachineInitializationContext<TInitializationEvent>, Task<bool>> actionAsync)
        {
            actionAsync.ThrowIfNull(nameof(actionAsync));
            
            var initializerName = Event<TInitializationEvent>.Name;

            return AddInitializer(typeof(TInitializationEvent), initializerName, async c =>
            {
                var result = false;
                var context = new StateMachineInitializationContext<TInitializationEvent>(c, c.EventHolder as EventHolder<TInitializationEvent>);

                result = await actionAsync(context);

                return result;
            });
        }

        public IInitializedStateMachineBuilder AddFinalizer(Func<IStateMachineActionContext, Task> actionAsync)
        {
            actionAsync.ThrowIfNull(nameof(actionAsync));

            Result.Finalize.Actions.Add(async c =>
            {
                var context = new StateMachineActionContext(c);

                await actionAsync(context);
            });

            return this;
        }

        private IInitializedStateMachineBuilder AddVertex(string stateName, VertexType type, Action<Vertex> vertexBuildAction = null)
        {
            stateName.ThrowIfNullOrEmpty(nameof(stateName));

            if (Result.Vertices.ContainsKey(stateName))
                throw new StateDefinitionException(stateName, $"State '{stateName}' is already registered", Result.Class);

            var vertex = new Vertex()
            {
                Name = stateName,
                Type = type,
                Graph = Result,
            };

            vertexBuildAction?.Invoke(vertex);

            Result.Vertices.Add(vertex.Name, vertex);
            Result.AllVertices.Add(vertex.Identifier, vertex);

            return this;
        }

        #region AddState
        public IInitializedStateMachineBuilder AddState(string stateName, StateBuildAction stateBuildAction = null)
            => AddVertex(stateName, VertexType.State, vertex => stateBuildAction?.Invoke(new StateBuilder(vertex, Services)));
        #endregion

        #region AddFinalState
        public IFinalizedStateMachineBuilder AddFinalState(string stateName = FinalState.Name)
            => AddVertex(stateName, VertexType.FinalState) as IFinalizedStateMachineBuilder;
        #endregion

        #region AddCompositeState
        public IInitializedStateMachineBuilder AddCompositeState(string stateName, CompositeStateBuildAction compositeStateBuildAction)
            => AddVertex(stateName, VertexType.CompositeState, vertex => compositeStateBuildAction?.Invoke(new CompositeStateBuilder(vertex, Services)));

        public IInitializedStateMachineBuilder AddInitialState(string stateName, StateBuildAction stateBuildAction = null)
        {
            Result.InitialVertexName = stateName;
            return AddVertex(stateName, VertexType.InitialState, vertex => stateBuildAction?.Invoke(new StateBuilder(vertex, Services)));
        }

        public IInitializedStateMachineBuilder AddInitialCompositeState(string stateName, CompositeStateBuildAction compositeStateBuildAction)
        {
            Result.InitialVertexName = stateName;
            return AddVertex(stateName, VertexType.InitialCompositeState, vertex => compositeStateBuildAction?.Invoke(new CompositeStateBuilder(vertex, Services)));
        }
        #endregion

        #region Observability
        public IInitializedStateMachineBuilder AddExceptionHandler<TExceptionHandler>()
            where TExceptionHandler : class, IStateMachineExceptionHandler
        {
            Services.AddServiceType<TExceptionHandler>();
            AddExceptionHandler(serviceProvider => serviceProvider.GetRequiredService<TExceptionHandler>());

            return this;
        }

        public IInitializedStateMachineBuilder AddExceptionHandler(StateMachineExceptionHandlerFactory exceptionHandlerFactory)
        {
            Result.ExceptionHandlerFactories.Add(exceptionHandlerFactory);

            return this;
        }

        public IInitializedStateMachineBuilder AddInterceptor<TInterceptor>()
            where TInterceptor : class, IStateMachineInterceptor
        {
            Services.AddServiceType<TInterceptor>();
            AddInterceptor(serviceProvider => serviceProvider.GetRequiredService<TInterceptor>());

            return this;
        }

        public IInitializedStateMachineBuilder AddInterceptor(StateMachineInterceptorFactory interceptorFactory)
        {
            Result.InterceptorFactories.Add(interceptorFactory);

            return this;
        }

        public IInitializedStateMachineBuilder AddObserver<TObserver>()
            where TObserver : class, IStateMachineObserver
        {
            Services.AddServiceType<TObserver>();
            AddObserver(serviceProvider => serviceProvider.GetRequiredService<TObserver>());

            return this;
        }

        public IInitializedStateMachineBuilder AddObserver(StateMachineObserverFactory observerFactory)
        {
            Result.ObserverFactories.Add(observerFactory);

            return this;
        }

        IStateMachineBuilder IStateMachineUtils<IStateMachineBuilder>.AddInterceptor(StateMachineInterceptorFactory interceptorFactory)
            => AddInterceptor(interceptorFactory) as IStateMachineBuilder;

        IStateMachineBuilder IStateMachineUtils<IStateMachineBuilder>.AddInterceptor<TInterceptor>()
            => AddInterceptor<TInterceptor>() as IStateMachineBuilder;

        IStateMachineBuilder IStateMachineUtils<IStateMachineBuilder>.AddObserver(StateMachineObserverFactory observerFactory)
            => AddObserver(observerFactory) as IStateMachineBuilder;

        IStateMachineBuilder IStateMachineUtils<IStateMachineBuilder>.AddObserver<TObserver>()
            => AddObserver<TObserver>() as IStateMachineBuilder;

        IStateMachineBuilder IStateMachineUtils<IStateMachineBuilder>.AddExceptionHandler(StateMachineExceptionHandlerFactory exceptionHandlerFactory)
            => AddExceptionHandler(exceptionHandlerFactory) as IStateMachineBuilder;

        IStateMachineBuilder IStateMachineUtils<IStateMachineBuilder>.AddExceptionHandler<TExceptionHandler>()
            => AddExceptionHandler<TExceptionHandler>() as IStateMachineBuilder;

        IStateMachineBuilder IStateMachineEvents<IStateMachineBuilder>.AddDefaultInitializer(Func<IStateMachineInitializationContext, Task<bool>> actionAsync)
            => AddDefaultInitializer(actionAsync) as IStateMachineBuilder;

        IStateMachineBuilder IStateMachineEvents<IStateMachineBuilder>.AddInitializer<TInitializationEvent>(Func<IStateMachineInitializationContext<TInitializationEvent>, Task<bool>> actionAsync)
            => AddInitializer<TInitializationEvent>(actionAsync) as IStateMachineBuilder;

        IStateMachineBuilder IStateMachineEvents<IStateMachineBuilder>.AddFinalizer(Func<IStateMachineActionContext, Task> actionAsync)
            => AddFinalizer(actionAsync) as IStateMachineBuilder;

        IFinalizedStateMachineBuilder IStateMachineUtils<IFinalizedStateMachineBuilder>.AddInterceptor(StateMachineInterceptorFactory interceptorFactory)
            => AddInterceptor(interceptorFactory) as IFinalizedStateMachineBuilder;

        IFinalizedStateMachineBuilder IStateMachineUtils<IFinalizedStateMachineBuilder>.AddInterceptor<TInterceptor>()
            => AddInterceptor<TInterceptor>() as IFinalizedStateMachineBuilder;

        IFinalizedStateMachineBuilder IStateMachineUtils<IFinalizedStateMachineBuilder>.AddObserver(StateMachineObserverFactory observerFactory)
            => AddObserver(observerFactory) as IFinalizedStateMachineBuilder;

        IFinalizedStateMachineBuilder IStateMachineUtils<IFinalizedStateMachineBuilder>.AddObserver<TObserver>()
            => AddObserver<TObserver>() as IFinalizedStateMachineBuilder;

        IFinalizedStateMachineBuilder IStateMachineUtils<IFinalizedStateMachineBuilder>.AddExceptionHandler(StateMachineExceptionHandlerFactory exceptionHandlerFactory)
            => AddExceptionHandler(exceptionHandlerFactory) as IFinalizedStateMachineBuilder;

        IFinalizedStateMachineBuilder IStateMachineUtils<IFinalizedStateMachineBuilder>.AddExceptionHandler<TExceptionHandler>()
            => AddExceptionHandler<TExceptionHandler>() as IFinalizedStateMachineBuilder;

        IFinalizedStateMachineBuilder IStateMachineEvents<IFinalizedStateMachineBuilder>.AddDefaultInitializer(Func<IStateMachineInitializationContext, Task<bool>> actionAsync)
            => AddDefaultInitializer(actionAsync) as IFinalizedStateMachineBuilder;

        IFinalizedStateMachineBuilder IStateMachineEvents<IFinalizedStateMachineBuilder>.AddInitializer<TInitializationEvent>(Func<IStateMachineInitializationContext<TInitializationEvent>, Task<bool>> actionAsync)
            => AddInitializer(actionAsync) as IFinalizedStateMachineBuilder;

        IFinalizedStateMachineBuilder IStateMachineEvents<IFinalizedStateMachineBuilder>.AddFinalizer(Func<IStateMachineActionContext, Task> actionAsync)
            => AddFinalizer(actionAsync) as IFinalizedStateMachineBuilder;
        #endregion
    }
}


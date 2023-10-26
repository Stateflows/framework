using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Models;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Interfaces;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Base;
using Stateflows.StateMachines.Registration.Interfaces.Internal;
using Stateflows.StateMachines.Exceptions;

namespace Stateflows.StateMachines.Registration.Builders
{
    internal class StateMachineBuilder :
        IStateMachineBuilder,
        IFinalizedStateMachineBuilder,
        IStateMachineInitialBuilder,
        ITypedStateMachineBuilder,
        ITypedFinalizedStateMachineBuilder,
        ITypedStateMachineInitialBuilder,
        IInternal
    {
        public Graph Result { get; }

        public IServiceCollection Services { get; }

        public StateMachineBuilder(string name, IServiceCollection services)
        {
            Services = services;
            Result = new Graph(name);
        }

        public IStateMachineBuilder AddInitializer(string initializerName, StateMachineActionAsync initializerAction)
        {
            if (!Result.Initializers.TryGetValue(initializerName, out var initializer))
            {
                initializer = new Logic<StateMachineActionAsync>()
                {
                    Name = Constants.Initialize
                };

                Result.Initializers.Add(initializerName, initializer);
            }

            initializer.Actions.Add(initializerAction);

            return this;
        }

        public IStateMachineBuilder AddOnInitialize(Func<IStateMachineInitializationContext, Task> actionAsync)
        {
            actionAsync.ThrowIfNull(nameof(actionAsync));

            var initializerName = EventInfo<InitializationRequest>.Name;

            return AddInitializer(initializerName, async c =>
            {
                var context = new StateMachineInitializationContext(c.Event as InitializationRequest, c);
                try
                {
                    await actionAsync(context);
                }
                catch (Exception e)
                {
                    await c.Executor.Inspector.OnStateMachineInitializeExceptionAsync(context, e);
                }
            });
        }

        public IStateMachineBuilder AddOnInitialize<TInitializationRequest>(Func<IStateMachineInitializationContext<TInitializationRequest>, Task> actionAsync)
            where TInitializationRequest : InitializationRequest, new()
        {
            actionAsync.ThrowIfNull(nameof(actionAsync));
            
            var initializerName = EventInfo<TInitializationRequest>.Name;

            return AddInitializer(initializerName, async c =>
            {
                var context = new StateMachineInitializationContext<TInitializationRequest>(c.Event as TInitializationRequest, c);
                try
                {
                    await actionAsync(context);
                }
                catch (Exception e)
                {
                    await c.Executor.Inspector.OnStateMachineInitializeExceptionAsync(context, e);
                }
            });
        }

        public IStateMachineBuilder AddOnFinalize(Func<IStateMachineActionContext, Task> actionAsync)
        {
            actionAsync.ThrowIfNull(nameof(actionAsync));

            Result.Finalize.Actions.Add(async c =>
            {
                var context = new StateMachineActionContext(c);
                try
                {
                    await actionAsync(context);
                }
                catch (Exception e)
                {
                    await c.Executor.Inspector.OnStateMachineFinalizeExceptionAsync(context, e);
                }
            });

            return this;
        }

        private IStateMachineBuilder AddVertex(string stateName, VertexType type, Action<Vertex> vertexBuildAction = null)
        {
            stateName.ThrowIfNullOrEmpty(nameof(stateName));

            if (Result.Vertices.ContainsKey(stateName))
                throw new StateDefinitionException(stateName, $"State '{stateName}' is already registered");

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
        public IStateMachineBuilder AddState(string stateName, StateBuilderAction stateBuildAction = null)
            => AddVertex(stateName, VertexType.State, vertex => stateBuildAction?.Invoke(new StateBuilder(vertex, Services)));
        #endregion

        #region AddFinalState
        public IFinalizedStateMachineBuilder AddFinalState(string stateName = FinalState.Name)
            => AddVertex(stateName, VertexType.FinalState) as IFinalizedStateMachineBuilder;
        #endregion

        #region AddCompositeState
        public IStateMachineBuilder AddCompositeState(string stateName, CompositeStateBuilderAction compositeStateBuildAction)
            => AddVertex(stateName, VertexType.CompositeState, vertex => compositeStateBuildAction?.Invoke(new CompositeStateBuilder(vertex, Services)));

        public IStateMachineBuilder AddInitialState(string stateName, StateBuilderAction stateBuildAction = null)
        {
            Result.InitialVertexName = stateName;
            return AddVertex(stateName, VertexType.InitialState, vertex => stateBuildAction?.Invoke(new StateBuilder(vertex, Services)));
        }

        public IStateMachineBuilder AddInitialCompositeState(string stateName, CompositeStateBuilderAction compositeStateBuildAction)
        {
            Result.InitialVertexName = stateName;
            return AddVertex(stateName, VertexType.InitialCompositeState, vertex => compositeStateBuildAction?.Invoke(new CompositeStateBuilder(vertex, Services)));
        }
        #endregion

        #region Observability
        public IStateMachineBuilder AddExceptionHandler<TExceptionHandler>()
            where TExceptionHandler : class, IStateMachineExceptionHandler
        {
            Services.RegisterExceptionHandler<TExceptionHandler>();
            AddExceptionHandler(serviceProvider => serviceProvider.GetRequiredService<TExceptionHandler>());

            return this;
        }

        public IStateMachineBuilder AddExceptionHandler(StateMachineExceptionHandlerFactory exceptionHandlerFactory)
        {
            Result.ExceptionHandlerFactories.Add(exceptionHandlerFactory);

            return this;
        }

        public IStateMachineBuilder AddInterceptor<TInterceptor>()
            where TInterceptor : class, IStateMachineInterceptor
        {
            Services.RegisterInterceptor<TInterceptor>();
            AddInterceptor(serviceProvider => serviceProvider.GetRequiredService<TInterceptor>());

            return this;
        }

        public IStateMachineBuilder AddInterceptor(StateMachineInterceptorFactory interceptorFactory)
        {
            Result.InterceptorFactories.Add(interceptorFactory);

            return this;
        }

        public IStateMachineBuilder AddObserver<TObserver>()
            where TObserver : class, IStateMachineObserver
        {
            Services.RegisterObserver<TObserver>();
            AddObserver(serviceProvider => serviceProvider.GetRequiredService<TObserver>());

            return this;
        }

        public IStateMachineBuilder AddObserver(StateMachineObserverFactory observerFactory)
        {
            Result.ObserverFactories.Add(observerFactory);

            return this;
        }
        #endregion

        IStateMachineInitialBuilder IStateMachineUtils<IStateMachineInitialBuilder>.AddInterceptor<TInterceptor>()
            => AddInterceptor<TInterceptor>() as IStateMachineInitialBuilder;

        IStateMachineInitialBuilder IStateMachineUtils<IStateMachineInitialBuilder>.AddObserver<TObserver>()
            => AddObserver<TObserver>() as IStateMachineInitialBuilder;

        IStateMachineInitialBuilder IStateMachineUtils<IStateMachineInitialBuilder>.AddExceptionHandler<TExceptionHandler>()
            => AddExceptionHandler<TExceptionHandler>() as IStateMachineInitialBuilder;

        IStateMachineInitialBuilder IStateMachineEvents<IStateMachineInitialBuilder>.AddOnInitialize(Func<IStateMachineInitializationContext, Task> actionAsync)
            => AddOnInitialize(actionAsync) as IStateMachineInitialBuilder;

        IStateMachineInitialBuilder IStateMachineEvents<IStateMachineInitialBuilder>.AddOnInitialize<TInitializationRequest>(Func<IStateMachineInitializationContext<TInitializationRequest>, Task> actionAsync)
            => AddOnInitialize<TInitializationRequest>(actionAsync) as IStateMachineInitialBuilder;

        IStateMachineInitialBuilder IStateMachineEvents<IStateMachineInitialBuilder>.AddOnFinalize(Func<IStateMachineActionContext, Task> actionAsync)
            => AddOnFinalize(actionAsync) as IStateMachineInitialBuilder;

        IFinalizedStateMachineBuilder IStateMachineUtils<IFinalizedStateMachineBuilder>.AddInterceptor<TInterceptor>()
            => AddInterceptor<TInterceptor>() as IFinalizedStateMachineBuilder;

        IFinalizedStateMachineBuilder IStateMachineUtils<IFinalizedStateMachineBuilder>.AddObserver<TObserver>()
            => AddObserver<TObserver>() as IFinalizedStateMachineBuilder;

        IFinalizedStateMachineBuilder IStateMachineUtils<IFinalizedStateMachineBuilder>.AddExceptionHandler<TExceptionHandler>()
            => AddExceptionHandler<TExceptionHandler>() as IFinalizedStateMachineBuilder;

        IFinalizedStateMachineBuilder IStateMachineEvents<IFinalizedStateMachineBuilder>.AddOnInitialize(Func<IStateMachineInitializationContext, Task> actionAsync)
            => AddOnInitialize(actionAsync) as IFinalizedStateMachineBuilder;

        IFinalizedStateMachineBuilder IStateMachineEvents<IFinalizedStateMachineBuilder>.AddOnInitialize<TInitializationRequest>(Func<IStateMachineInitializationContext<TInitializationRequest>, Task> actionAsync)
            => AddOnInitialize(actionAsync) as IFinalizedStateMachineBuilder;

        IFinalizedStateMachineBuilder IStateMachineEvents<IFinalizedStateMachineBuilder>.AddOnFinalize(Func<IStateMachineActionContext, Task> actionAsync)
            => AddOnFinalize(actionAsync) as IFinalizedStateMachineBuilder;


        ITypedStateMachineBuilder IStateMachine<ITypedStateMachineBuilder>.AddState(string stateName, StateBuilderAction stateBuildAction)
            => AddState(stateName, stateBuildAction) as ITypedStateMachineBuilder;

        ITypedFinalizedStateMachineBuilder IStateMachineFinal<ITypedFinalizedStateMachineBuilder>.AddFinalState(string stateName)
            => AddFinalState(stateName) as ITypedFinalizedStateMachineBuilder;

        ITypedStateMachineBuilder IStateMachine<ITypedStateMachineBuilder>.AddCompositeState(string stateName, CompositeStateBuilderAction compositeStateBuildAction)
            => AddCompositeState(stateName, compositeStateBuildAction) as ITypedStateMachineBuilder;

        ITypedStateMachineBuilder IStateMachineUtils<ITypedStateMachineBuilder>.AddInterceptor<TInterceptor>()
            => AddInterceptor<TInterceptor>() as ITypedStateMachineBuilder;

        ITypedStateMachineBuilder IStateMachineUtils<ITypedStateMachineBuilder>.AddObserver<TObserver>()
            => AddObserver<TObserver>() as ITypedStateMachineBuilder;

        ITypedStateMachineBuilder IStateMachineUtils<ITypedStateMachineBuilder>.AddExceptionHandler<TExceptionHandler>()
            => AddExceptionHandler<TExceptionHandler>() as ITypedStateMachineBuilder;

        ITypedStateMachineBuilder IStateMachineInitial<ITypedStateMachineBuilder>.AddInitialState(string stateName, StateBuilderAction stateBuildAction)
            => AddInitialState(stateName, stateBuildAction) as ITypedStateMachineBuilder;

        ITypedStateMachineBuilder IStateMachineInitial<ITypedStateMachineBuilder>.AddInitialCompositeState(string stateName, CompositeStateBuilderAction compositeStateBuildAction)
            => AddInitialCompositeState(stateName, compositeStateBuildAction) as ITypedStateMachineBuilder;

        ITypedStateMachineInitialBuilder IStateMachineUtils<ITypedStateMachineInitialBuilder>.AddInterceptor<TInterceptor>()
            => AddInterceptor<TInterceptor>() as ITypedStateMachineInitialBuilder;

        ITypedStateMachineInitialBuilder IStateMachineUtils<ITypedStateMachineInitialBuilder>.AddObserver<TObserver>()
            => AddObserver<TObserver>() as ITypedStateMachineInitialBuilder;

        ITypedStateMachineInitialBuilder IStateMachineUtils<ITypedStateMachineInitialBuilder>.AddExceptionHandler<TExceptionHandler>()
            => AddExceptionHandler<TExceptionHandler>() as ITypedStateMachineInitialBuilder;

        ITypedFinalizedStateMachineBuilder IStateMachine<ITypedFinalizedStateMachineBuilder>.AddState(string stateName, StateBuilderAction stateBuildAction)
            => AddState(stateName, stateBuildAction) as ITypedFinalizedStateMachineBuilder;

        ITypedFinalizedStateMachineBuilder IStateMachine<ITypedFinalizedStateMachineBuilder>.AddCompositeState(string stateName, CompositeStateBuilderAction compositeStateBuildAction)
            => AddCompositeState(stateName, compositeStateBuildAction) as ITypedFinalizedStateMachineBuilder;

        ITypedFinalizedStateMachineBuilder IStateMachineUtils<ITypedFinalizedStateMachineBuilder>.AddInterceptor<TInterceptor>()
            => AddInterceptor<TInterceptor>() as ITypedFinalizedStateMachineBuilder;

        ITypedFinalizedStateMachineBuilder IStateMachineUtils<ITypedFinalizedStateMachineBuilder>.AddObserver<TObserver>()
            => AddObserver<TObserver>() as ITypedFinalizedStateMachineBuilder;

        ITypedFinalizedStateMachineBuilder IStateMachineUtils<ITypedFinalizedStateMachineBuilder>.AddExceptionHandler<TExceptionHandler>()
            => AddExceptionHandler<TExceptionHandler>() as ITypedFinalizedStateMachineBuilder;
    }
}


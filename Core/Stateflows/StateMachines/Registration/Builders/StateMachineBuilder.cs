using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Models;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Interfaces;
using Stateflows.StateMachines.Exceptions;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Base;
using Stateflows.StateMachines.Registration.Interfaces.Internal;
using Stateflows.Common.Registration;

namespace Stateflows.StateMachines.Registration.Builders
{
    internal class StateMachineBuilder :
        IInitializedStateMachineBuilder,
        IFinalizedStateMachineBuilder,
        IStateMachineBuilder,
        ITypedInitializedStateMachineBuilder,
        ITypedFinalizedStateMachineBuilder,
        ITypedStateMachineBuilder,
        IInternal,
        IBehaviorBuilder
    {
        public Graph Result { get; }

        public IServiceCollection Services { get; }

        BehaviorClass IBehaviorBuilder.BehaviorClass => new BehaviorClass(nameof(StateMachine), Result.Name);

        int IBehaviorBuilder.BehaviorVersion => Result.Version;

        public StateMachineBuilder(string name, int version, IServiceCollection services)
        {
            Services = services;
            Result = new Graph(name, version);
        }

        public IInitializedStateMachineBuilder AddInitializer(string initializerName, StateMachinePredicateAsync initializerAction)
        {
            if (!Result.Initializers.TryGetValue(initializerName, out var initializer))
            {
                initializer = new Logic<StateMachinePredicateAsync>()
                {
                    Name = Constants.Initialize
                };

                Result.Initializers.Add(initializerName, initializer);
            }

            initializer.Actions.Add(initializerAction);

            return this;
        }

        public IInitializedStateMachineBuilder AddOnInitialize(Func<IStateMachineInitializationContext, Task<bool>> actionAsync)
            => AddOnInitialize<InitializationRequest>(c =>
            {
                var ctx = (c as BaseContext).Context;
                var context = new StateMachineInitializationContext(ctx, ctx.Event as InitializationRequest);
                return actionAsync(context);
            });

        public IInitializedStateMachineBuilder AddOnInitialize<TInitializationRequest>(Func<IStateMachineInitializationContext<TInitializationRequest>, Task<bool>> actionAsync)
            where TInitializationRequest : InitializationRequest, new()
        {
            actionAsync.ThrowIfNull(nameof(actionAsync));
            
            var initializerName = EventInfo<TInitializationRequest>.Name;

            return AddInitializer(initializerName, async c =>
            {
                var result = false;
                var context = new StateMachineInitializationContext<TInitializationRequest>(c, c.Event as TInitializationRequest);
                try
                {
                    result = await actionAsync(context);
                }
                catch (Exception e)
                {
                    await c.Executor.Inspector.OnStateMachineInitializationExceptionAsync(context, e);
                    result = false;
                }

                return result;
            });
        }

        public IInitializedStateMachineBuilder AddOnFinalize(Func<IStateMachineActionContext, Task> actionAsync)
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
                    await c.Executor.Inspector.OnStateMachineFinalizationExceptionAsync(context, e);
                }
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
            Services.RegisterExceptionHandler<TExceptionHandler>();
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
            Services.RegisterInterceptor<TInterceptor>();
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
            Services.RegisterObserver<TObserver>();
            AddObserver(serviceProvider => serviceProvider.GetRequiredService<TObserver>());

            return this;
        }

        public IInitializedStateMachineBuilder AddObserver(StateMachineObserverFactory observerFactory)
        {
            Result.ObserverFactories.Add(observerFactory);

            return this;
        }

        IStateMachineBuilder IStateMachineUtils<IStateMachineBuilder>.AddInterceptor<TInterceptor>()
            => AddInterceptor<TInterceptor>() as IStateMachineBuilder;

        IStateMachineBuilder IStateMachineUtils<IStateMachineBuilder>.AddObserver<TObserver>()
            => AddObserver<TObserver>() as IStateMachineBuilder;

        IStateMachineBuilder IStateMachineUtils<IStateMachineBuilder>.AddExceptionHandler<TExceptionHandler>()
            => AddExceptionHandler<TExceptionHandler>() as IStateMachineBuilder;

        IStateMachineBuilder IStateMachineEvents<IStateMachineBuilder>.AddOnInitialize(Func<IStateMachineInitializationContext, Task<bool>> actionAsync)
            => AddOnInitialize(actionAsync) as IStateMachineBuilder;

        IStateMachineBuilder IStateMachineEvents<IStateMachineBuilder>.AddOnInitialize<TInitializationRequest>(Func<IStateMachineInitializationContext<TInitializationRequest>, Task<bool>> actionAsync)
            => AddOnInitialize<TInitializationRequest>(actionAsync) as IStateMachineBuilder;

        IStateMachineBuilder IStateMachineEvents<IStateMachineBuilder>.AddOnFinalize(Func<IStateMachineActionContext, Task> actionAsync)
            => AddOnFinalize(actionAsync) as IStateMachineBuilder;

        IFinalizedStateMachineBuilder IStateMachineUtils<IFinalizedStateMachineBuilder>.AddInterceptor<TInterceptor>()
            => AddInterceptor<TInterceptor>() as IFinalizedStateMachineBuilder;

        IFinalizedStateMachineBuilder IStateMachineUtils<IFinalizedStateMachineBuilder>.AddObserver<TObserver>()
            => AddObserver<TObserver>() as IFinalizedStateMachineBuilder;

        IFinalizedStateMachineBuilder IStateMachineUtils<IFinalizedStateMachineBuilder>.AddExceptionHandler<TExceptionHandler>()
            => AddExceptionHandler<TExceptionHandler>() as IFinalizedStateMachineBuilder;

        IFinalizedStateMachineBuilder IStateMachineEvents<IFinalizedStateMachineBuilder>.AddOnInitialize(Func<IStateMachineInitializationContext, Task<bool>> actionAsync)
            => AddOnInitialize(actionAsync) as IFinalizedStateMachineBuilder;

        IFinalizedStateMachineBuilder IStateMachineEvents<IFinalizedStateMachineBuilder>.AddOnInitialize<TInitializationRequest>(Func<IStateMachineInitializationContext<TInitializationRequest>, Task<bool>> actionAsync)
            => AddOnInitialize(actionAsync) as IFinalizedStateMachineBuilder;

        IFinalizedStateMachineBuilder IStateMachineEvents<IFinalizedStateMachineBuilder>.AddOnFinalize(Func<IStateMachineActionContext, Task> actionAsync)
            => AddOnFinalize(actionAsync) as IFinalizedStateMachineBuilder;


        ITypedInitializedStateMachineBuilder IStateMachine<ITypedInitializedStateMachineBuilder>.AddState(string stateName, StateBuildAction stateBuildAction)
            => AddState(stateName, stateBuildAction) as ITypedInitializedStateMachineBuilder;

        ITypedFinalizedStateMachineBuilder IStateMachineFinal<ITypedFinalizedStateMachineBuilder>.AddFinalState(string stateName)
            => AddFinalState(stateName) as ITypedFinalizedStateMachineBuilder;

        ITypedInitializedStateMachineBuilder IStateMachine<ITypedInitializedStateMachineBuilder>.AddCompositeState(string stateName, CompositeStateBuildAction compositeStateBuildAction)
            => AddCompositeState(stateName, compositeStateBuildAction) as ITypedInitializedStateMachineBuilder;

        ITypedInitializedStateMachineBuilder IStateMachineUtils<ITypedInitializedStateMachineBuilder>.AddInterceptor<TInterceptor>()
            => AddInterceptor<TInterceptor>() as ITypedInitializedStateMachineBuilder;

        ITypedInitializedStateMachineBuilder IStateMachineUtils<ITypedInitializedStateMachineBuilder>.AddObserver<TObserver>()
            => AddObserver<TObserver>() as ITypedInitializedStateMachineBuilder;

        ITypedInitializedStateMachineBuilder IStateMachineUtils<ITypedInitializedStateMachineBuilder>.AddExceptionHandler<TExceptionHandler>()
            => AddExceptionHandler<TExceptionHandler>() as ITypedInitializedStateMachineBuilder;

        ITypedInitializedStateMachineBuilder IStateMachineInitial<ITypedInitializedStateMachineBuilder>.AddInitialState(string stateName, StateBuildAction stateBuildAction)
            => AddInitialState(stateName, stateBuildAction) as ITypedInitializedStateMachineBuilder;

        ITypedInitializedStateMachineBuilder IStateMachineInitial<ITypedInitializedStateMachineBuilder>.AddInitialCompositeState(string stateName, CompositeStateBuildAction compositeStateBuildAction)
            => AddInitialCompositeState(stateName, compositeStateBuildAction) as ITypedInitializedStateMachineBuilder;

        ITypedStateMachineBuilder IStateMachineUtils<ITypedStateMachineBuilder>.AddInterceptor<TInterceptor>()
            => AddInterceptor<TInterceptor>() as ITypedStateMachineBuilder;

        ITypedStateMachineBuilder IStateMachineUtils<ITypedStateMachineBuilder>.AddObserver<TObserver>()
            => AddObserver<TObserver>() as ITypedStateMachineBuilder;

        ITypedStateMachineBuilder IStateMachineUtils<ITypedStateMachineBuilder>.AddExceptionHandler<TExceptionHandler>()
            => AddExceptionHandler<TExceptionHandler>() as ITypedStateMachineBuilder;

        ITypedFinalizedStateMachineBuilder IStateMachineUtils<ITypedFinalizedStateMachineBuilder>.AddInterceptor<TInterceptor>()
            => AddInterceptor<TInterceptor>() as ITypedFinalizedStateMachineBuilder;

        ITypedFinalizedStateMachineBuilder IStateMachineUtils<ITypedFinalizedStateMachineBuilder>.AddObserver<TObserver>()
            => AddObserver<TObserver>() as ITypedFinalizedStateMachineBuilder;

        ITypedFinalizedStateMachineBuilder IStateMachineUtils<ITypedFinalizedStateMachineBuilder>.AddExceptionHandler<TExceptionHandler>()
            => AddExceptionHandler<TExceptionHandler>() as ITypedFinalizedStateMachineBuilder;
        #endregion
    }
}


using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Base;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Registration.Builders
{
    internal class StateMachineBuilderBase : IStateMachineBuilder, IStateMachineInitialBuilder, IStateMachineBuilderInternal
    {
        public Graph Result { get; }

        public IServiceCollection Services { get; }

        public StateMachineBuilderBase(string name, IServiceCollection services)
        {
            Services = services;
            Result = new Graph(name);
        }

        public IStateMachineBuilder AddOnInitialize(Func<IStateMachineActionContext, Task> actionAsync)
        {
            if (actionAsync == null)
                throw new ArgumentNullException("Action not provided");

            Result.Initialize.Actions.Add(async c =>
            {
                var context = new StateMachineActionContext(c);
                try
                {
                    await actionAsync(context);
                }
                catch (Exception e)
                {
                    await c.Executor.Observer.OnStateMachineInitializeExceptionAsync(context, e);
                }
            });

            return this;
        }

        #region AddState
        public IStateMachineBuilder AddState(string stateName, StateBuilderAction stateBuildAction = null)
        {
            if (Result.AllVertices.ContainsKey(stateName))
            {
                throw new Exception($"State '{stateName}' is already registered");
            }

            var vertex = new Vertex()
            {
                Name = stateName,
                Graph = Result,
            };

            stateBuildAction?.Invoke(new StateBuilder(vertex, Services));

            Result.Vertices.Add(vertex.Name, vertex);
            Result.AllVertices.Add(vertex.Name, vertex);

            return this;
        }
        #endregion

        #region AddCompositeState
        public IStateMachineBuilder AddCompositeState(string stateName, CompositeStateBuilderAction compositeStateBuildAction)
        {
            var vertex = new Vertex()
            {
                Name = stateName,
                Graph = Result,
            };

            compositeStateBuildAction?.Invoke(new CompositeStateBuilder(vertex, Services));

            Result.Vertices.Add(vertex.Name, vertex);
            Result.AllVertices.Add(vertex.Name, vertex);

            return this;
        }

        public IStateMachineBuilder AddExceptionHandler<TExceptionHandler>()
            where TExceptionHandler : class, IStateMachineExceptionHandler
        {
            Services.RegisterExceptionHandler<TExceptionHandler>();
            AddExceptionHandler(serviceProvider => serviceProvider.GetRequiredService<TExceptionHandler>());

            return this;
        }

        public IStateMachineBuilder AddExceptionHandler(ExceptionHandlerFactory exceptionHandlerFactory)
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

        public IStateMachineBuilder AddInterceptor(InterceptorFactory interceptorFactory)
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

        public IStateMachineBuilder AddObserver(ObserverFactory observerFactory)
        {
            Result.ObserverFactories.Add(observerFactory);

            return this;
        }

        public IStateMachineBuilder AddInitialState(string stateName, StateBuilderAction stateBuildAction = null)
        {
            Result.InitialVertexName = stateName;
            return AddState(stateName, stateBuildAction);
        }

        public IStateMachineBuilder AddInitialCompositeState(string stateName, CompositeStateBuilderAction compositeStateBuildAction)
        {
            Result.InitialVertexName = stateName;
            return AddCompositeState(stateName, compositeStateBuildAction);
        }
        #endregion

        IStateMachineInitialBuilder IStateMachineUtilsBuilderBase<IStateMachineInitialBuilder>.AddInterceptor<TInterceptor>()
            => AddInterceptor<TInterceptor>() as IStateMachineInitialBuilder;

        IStateMachineInitialBuilder IStateMachineUtilsBuilderBase<IStateMachineInitialBuilder>.AddObserver<TObserver>()
            => AddObserver<TObserver>() as IStateMachineInitialBuilder;

        IStateMachineInitialBuilder IStateMachineUtilsBuilderBase<IStateMachineInitialBuilder>.AddExceptionHandler<TExceptionHandler>()
            => AddExceptionHandler<TExceptionHandler>() as IStateMachineInitialBuilder;

        IStateMachineInitialBuilder IStateMachineEventsBuilderBase<IStateMachineInitialBuilder>.AddOnInitialize(Func<IStateMachineActionContext, Task> actionAsync)
            => AddOnInitialize(actionAsync) as IStateMachineInitialBuilder;
    }
}


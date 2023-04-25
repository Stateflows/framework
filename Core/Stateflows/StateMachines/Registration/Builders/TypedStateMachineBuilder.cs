using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Base;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Registration.Builders
{
    internal class TypedStateMachineBuilder : StateMachineBuilderBase, ITypedStateMachineBuilder, ITypedStateMachineInitialBuilder, IStateMachineBuilderInternal
    {
        public TypedStateMachineBuilder(string name, Type stateMachineType, IServiceCollection services)
            : base(name, services)
        {
            Result.StateMachineType = stateMachineType;

            this.AddStateMachineEvents(stateMachineType);
        }

        ITypedStateMachineBuilder IStateMachineBuilderBase<ITypedStateMachineBuilder>.AddState(string stateName, StateBuilderAction stateBuildAction)
        {
            AddState(stateName, stateBuildAction);
            return this;
        }

        ITypedStateMachineBuilder IStateMachineBuilderBase<ITypedStateMachineBuilder>.AddCompositeState(string stateName, CompositeStateBuilderAction compositeStateBuildAction)
        {
            AddCompositeState(stateName, compositeStateBuildAction);
            return this;
        }

        ITypedStateMachineBuilder IStateMachineUtilsBuilderBase<ITypedStateMachineBuilder>.AddInterceptor<TInterceptor>()
        {
            AddInterceptor<TInterceptor>();
            return this;
        }

        ITypedStateMachineBuilder IStateMachineUtilsBuilderBase<ITypedStateMachineBuilder>.AddObserver<TObserver>()
        {
            AddObserver<TObserver>();
            return this;
        }

        ITypedStateMachineBuilder IStateMachineUtilsBuilderBase<ITypedStateMachineBuilder>.AddExceptionHandler<TExceptionHandler>()
        {
            AddExceptionHandler<TExceptionHandler>();
            return this;
        }

        ITypedStateMachineBuilder IStateMachineEventsBuilderBase<ITypedStateMachineBuilder>.AddOnInitialize(Func<IStateMachineActionContext, Task> actionAsync)
        {
            AddOnInitialize(actionAsync);
            return this;
        }

        ITypedStateMachineBuilder IStateMachineInitialBuilderBase<ITypedStateMachineBuilder>.AddInitialState(string stateName, StateBuilderAction stateBuildAction)
        {
            AddInitialState(stateName, stateBuildAction);
            return this;
        }

        ITypedStateMachineBuilder IStateMachineInitialBuilderBase<ITypedStateMachineBuilder>.AddInitialCompositeState(string stateName, CompositeStateBuilderAction compositeStateBuildAction)
        {
            AddInitialCompositeState(stateName, compositeStateBuildAction);
            return this;
        }

        ITypedStateMachineInitialBuilder IStateMachineUtilsBuilderBase<ITypedStateMachineInitialBuilder>.AddInterceptor<TInterceptor>()
        {
            AddInterceptor<TInterceptor>();
            return this;
        }

        ITypedStateMachineInitialBuilder IStateMachineUtilsBuilderBase<ITypedStateMachineInitialBuilder>.AddObserver<TObserver>()
        {
            AddObserver<TObserver>(); 
            return this;
        }

        ITypedStateMachineInitialBuilder IStateMachineUtilsBuilderBase<ITypedStateMachineInitialBuilder>.AddExceptionHandler<TExceptionHandler>()
        {
            AddExceptionHandler<TExceptionHandler>(); 
            return this;
        }
    }
}


using System;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.StateMachines.Extensions;
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
            => AddState(stateName, stateBuildAction) as ITypedStateMachineBuilder;

        ITypedStateMachineBuilder IStateMachineBuilderBase<ITypedStateMachineBuilder>.AddCompositeState(string stateName, CompositeStateBuilderAction compositeStateBuildAction)
            => AddCompositeState(stateName, compositeStateBuildAction) as ITypedStateMachineBuilder;

        ITypedStateMachineBuilder IStateMachineUtilsBuilderBase<ITypedStateMachineBuilder>.AddInterceptor<TInterceptor>()
            => AddInterceptor<TInterceptor>() as ITypedStateMachineBuilder;

        ITypedStateMachineBuilder IStateMachineUtilsBuilderBase<ITypedStateMachineBuilder>.AddObserver<TObserver>()
            => AddObserver<TObserver>() as ITypedStateMachineBuilder;

        ITypedStateMachineBuilder IStateMachineUtilsBuilderBase<ITypedStateMachineBuilder>.AddExceptionHandler<TExceptionHandler>()
            => AddExceptionHandler<TExceptionHandler>() as ITypedStateMachineBuilder;

        ITypedStateMachineBuilder IStateMachineInitialBuilderBase<ITypedStateMachineBuilder>.AddInitialState(string stateName, StateBuilderAction stateBuildAction)
            => AddInitialState(stateName, stateBuildAction) as ITypedStateMachineBuilder;

        ITypedStateMachineBuilder IStateMachineInitialBuilderBase<ITypedStateMachineBuilder>.AddInitialCompositeState(string stateName, CompositeStateBuilderAction compositeStateBuildAction)
            => AddInitialCompositeState(stateName, compositeStateBuildAction) as ITypedStateMachineBuilder;

        ITypedStateMachineInitialBuilder IStateMachineUtilsBuilderBase<ITypedStateMachineInitialBuilder>.AddInterceptor<TInterceptor>()
            => AddInterceptor<TInterceptor>() as ITypedStateMachineInitialBuilder;

        ITypedStateMachineInitialBuilder IStateMachineUtilsBuilderBase<ITypedStateMachineInitialBuilder>.AddObserver<TObserver>()
            => AddObserver<TObserver>() as ITypedStateMachineInitialBuilder;

        ITypedStateMachineInitialBuilder IStateMachineUtilsBuilderBase<ITypedStateMachineInitialBuilder>.AddExceptionHandler<TExceptionHandler>()
            => AddExceptionHandler<TExceptionHandler>() as ITypedStateMachineInitialBuilder;
    }
}


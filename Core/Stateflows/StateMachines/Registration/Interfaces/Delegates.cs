﻿using System;
using System.Threading.Tasks;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    public delegate void StateMachinesBuildAction(IStateMachinesBuilder builder);

    public delegate void StateMachineBuildAction(IStateMachineBuilder builder);
    
    public delegate void OverridenStateMachineBuildAction(IOverridenStateMachineBuilder builder);

    public delegate void StateBuildAction(IStateBuilder builder);
    
    public delegate void OverridenStateBuildAction(IOverridenStateBuilder builder);

    public delegate void JunctionBuildAction(IJunctionBuilder builder);
    
    public delegate void OverridenJunctionBuildAction(IOverridenJunctionBuilder builder);

    public delegate void ChoiceBuildAction(IChoiceBuilder builder);
    
    public delegate void OverridenChoiceBuildAction(IOverridenChoiceBuilder builder);

    public delegate void ForkBuildAction(IForkBuilder builder);
    
    public delegate void OverridenForkBuildAction(IOverridenForkBuilder builder);
    
    public delegate void JoinBuildAction(IJoinBuilder builder);
    
    public delegate void OverridenJoinBuildAction(IOverridenJoinBuilder builder);

    public delegate void CompositeStateBuildAction(ICompositeStateBuilder builder);

    public delegate void OverridenCompositeStateBuildAction(IOverridenCompositeStateBuilder builder);

    public delegate void OrthogonalStateBuildAction(IOrthogonalStateBuilder builder);

    public delegate void OverridenOrthogonalStateBuildAction(IOverridenOrthogonalStateBuilder builder);

    public delegate void RegionBuildAction(IRegionBuilder builder);

    public delegate void OverridenRegionBuildAction(IOverridenRegionBuilder builder);

    public delegate void TransitionBuildAction<TEvent>(ITransitionBuilder<TEvent> builder);
    
    public delegate void OverridenTransitionBuildAction<TEvent>(IOverridenTransitionBuilder<TEvent> builder);

    public delegate void InternalTransitionBuildAction<TEvent>(IInternalTransitionBuilder<TEvent> builder);

    public delegate void OverridenInternalTransitionBuildAction<TEvent>(IOverridenInternalTransitionBuilder<TEvent> builder);

    public delegate void DefaultTransitionBuildAction(IDefaultTransitionBuilder builder);
    
    public delegate void OverridenDefaultTransitionBuildAction(IOverridenDefaultTransitionBuilder builder);

    public delegate void DefaultTransitionEffectBuildAction(IDefaultTransitionEffectBuilder builder);
    
    public delegate void OverridenDefaultTransitionEffectBuildAction(IOverridenDefaultTransitionEffectBuilder builder);

    public delegate void ElseTransitionBuildAction<TEvent>(IElseTransitionBuilder<TEvent> builder);

    public delegate void OverridenElseTransitionBuildAction<TEvent>(IElseTransitionBuilder<TEvent> builder);

    public delegate void ElseInternalTransitionBuildAction<TEvent>(IElseInternalTransitionBuilder<TEvent> builder);

    public delegate void OverridenElseInternalTransitionBuildAction<TEvent>(IElseInternalTransitionBuilder<TEvent> builder);

    public delegate void ElseDefaultTransitionBuildAction(IElseDefaultTransitionBuilder builder);

    public delegate void OverridenElseDefaultTransitionBuildAction(IElseDefaultTransitionBuilder builder);

    public delegate void EmbeddedBehaviorBuildAction(IEmbeddedBehaviorBuilder builder);

    public delegate void ForwardedEventBuildAction<TEvent>(IForwardedEventBuilder<TEvent> builder);

    public delegate IStateMachineObserver StateMachineObserverFactory(IServiceProvider serviceProvider);
    public delegate Task<IStateMachineObserver> StateMachineObserverFactoryAsync(IServiceProvider serviceProvider);

    public delegate IStateMachineInterceptor StateMachineInterceptorFactory(IServiceProvider serviceProvider);
    public delegate Task<IStateMachineInterceptor> StateMachineInterceptorFactoryAsync(IServiceProvider serviceProvider);

    public delegate IStateMachineExceptionHandler StateMachineExceptionHandlerFactory(IServiceProvider serviceProvider);
    public delegate Task<IStateMachineExceptionHandler> StateMachineExceptionHandlerFactoryAsync(IServiceProvider serviceProvider);
    
    public delegate object StateActionInitializationBuilder(IStateActionContext context);
}
﻿using Stateflows.StateMachines.Typed;
using Stateflows.StateMachines.Attributes;
using Stateflows.Common;

namespace StateMachine.IntegrationTests.Classes.StateMachines
{
    internal class TypedOtherInitializer : IInitializer<OtherEvent>
    {
        public Task<bool> OnInitializeAsync(OtherEvent initializationEvent)
        {
            return Task.FromResult(true);
        }
    }

    internal class TypedDefaultInitializer : IDefaultInitializer
    {
        public Task<bool> OnInitializeAsync()
        {
            return Task.FromResult(true);
        }
    }
    
    internal class TypedFinalizer : IFinalizer
    {
        public Task OnFinalizeAsync()
        {
            return Task.CompletedTask;
        }
    }

    internal class StateA : IStateEntry, IStateExit
    {
        private readonly IStateContext stateContext;
        private readonly IStateMachineContext stateMachineContext;
        private readonly IExecutionContext executionContext;
        public StateA(IStateContext stateContext, IStateMachineContext stateMachineContext, IExecutionContext executionContext)
        {
            this.stateContext = stateContext;
            this.stateMachineContext = stateMachineContext;
            this.executionContext = executionContext;
        }

        public Task OnEntryAsync()
        {
            return Task.CompletedTask;
        }

        public Task OnExitAsync()
        {
            return Task.CompletedTask;
        }
    }

    internal class StateB : IStateEntry
    {
        public Task OnEntryAsync()
        {
            return Task.CompletedTask;
        }
    }

    internal class SomeTransition : ITransitionGuard<SomeEvent>, ITransitionEffect<SomeEvent>
    {
        private readonly IStateMachineContext stateMachineContext;
        private readonly ITransitionContext transitionContext;
        private readonly IExecutionContext executionContext;

        public SomeTransition(
            IStateMachineContext stateMachineContext,
            ITransitionContext transitionContext,
            IExecutionContext executionContext
        )
        {
            this.stateMachineContext = stateMachineContext;
            this.transitionContext = transitionContext;
            this.executionContext = executionContext;
        }
        public Task<bool> GuardAsync(SomeEvent @event)
        {
            return Task.FromResult(true);
        }

        public Task EffectAsync(SomeEvent @event)
        {
            return Task.CompletedTask;
        }
    }

    [StateMachineBehavior]
    internal class TypedStateMachine : IStateMachine
    {
        public void Build(IStateMachineBuilder builder)
        {
            builder
                .AddInitializer<OtherEvent, TypedOtherInitializer>()
                //.AddDefaultInitializer<TypedDefaultInitializer>()
                .AddFinalizer<TypedFinalizer>()
                
                .AddInitialState<StateA>(b => b
                    .AddTransition<SomeEvent, SomeTransition, StateB>()
                )
                .AddState<StateB>(b => b
                    .AddTransition<SomeEvent, SomeTransition, FinalState>()
                )
                .AddState<FinalState>();
        }
    }
}

﻿using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.Activities.StateMachines.Interfaces;
using System.Threading.Tasks;

namespace Stateflows.Activities.Extensions
{
    public delegate EventHolder StateActionActivityInitializationBuilder(IStateActionContext context);

    public delegate EventHolder TransitionActivityInitializationBuilder<in TEvent>(ITransitionContext<TEvent> context);

    public delegate Task<TInitializationEvent> StateActionActivityInitializationBuilderAsync<TInitializationEvent>(IStateActionContext context);

    public delegate Task<EventHolder> TransitionActivityInitializationBuilderAsync<TEvent, TInitializationEvent>(ITransitionContext<TEvent> context);

    public delegate void StateActionActivityBuildAction(IStateActionActivityBuilder builder);

    public delegate void TransitionActivityBuildAction<TEvent>(ITransitionActivityBuilder<TEvent> builder);
}
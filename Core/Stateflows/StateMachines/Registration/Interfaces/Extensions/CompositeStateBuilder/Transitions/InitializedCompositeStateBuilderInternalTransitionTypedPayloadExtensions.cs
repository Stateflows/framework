﻿using Stateflows.Common;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Typed.Data
{
    public static class InitializedCompositeStateBuilderInternalTransitionTypedPayloadExtensions
    {
        public static IInitializedCompositeStateBuilder AddInternalDataTransition<TEventPayload, TTransition>(this IInitializedCompositeStateBuilder builder)
            where TTransition : Transition<Event<TEventPayload>>
            => builder.AddTransition<Event<TEventPayload>, TTransition>(Constants.DefaultTransitionTarget);
    }
}

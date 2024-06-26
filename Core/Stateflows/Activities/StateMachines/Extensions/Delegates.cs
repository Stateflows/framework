﻿using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.Activities.StateMachines.Interfaces;

namespace Stateflows.Activities.Extensions
{
    public delegate InitializationRequest StateActionActivityInitializationBuilder(IStateActionContext context);

    public delegate InitializationRequest GuardActivityInitializationBuilder<in TEvent>(IGuardContext<TEvent> context)
        where TEvent : Event, new();

    public delegate InitializationRequest EffectActivityInitializationBuilder<in TEvent>(IEventContext<TEvent> context)
        where TEvent : Event, new();

    public delegate void IntegratedActivityBuildAction(IIntegratedActivityBuilder builder);
}
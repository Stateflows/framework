using System;
using Stateflows.Common.Interfaces;

namespace Stateflows.Common.Classes;

internal class BehaviorFactory(StateflowsService stateflowsService, IServiceProvider serviceProvider) : IBehaviorFactory
{
    public IBehavior CreateBehavior(BehaviorId behaviorId)
        => new Behavior(stateflowsService, serviceProvider, behaviorId);
}
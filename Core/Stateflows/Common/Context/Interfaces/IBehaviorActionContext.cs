﻿namespace Stateflows.Common.Context.Interfaces
{
    public interface IBehaviorActionContext : IBehaviorLocator
    {
        IBehaviorContext Behavior { get; }
    }
}

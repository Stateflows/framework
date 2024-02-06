﻿namespace Stateflows.Activities.Registration.Interfaces.Base
{
    public interface IInitial<out TReturn>
    {
        TReturn AddInitial(InitialBuildAction buildAction);
    }
}

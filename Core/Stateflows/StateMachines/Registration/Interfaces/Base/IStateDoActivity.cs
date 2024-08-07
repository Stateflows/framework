﻿namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IStateDoActivity<out TReturn>
    {
        TReturn AddDoActivity(string doActivityName, EmbeddedBehaviorBuildAction buildAction = null, StateActionInitializationBuilderAsync initializationBuilder = null);
    }
}

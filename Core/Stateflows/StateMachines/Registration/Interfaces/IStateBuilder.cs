﻿using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    public interface IStateBuilder :
        IStateEvents<IStateBuilder>,
        IStateUtils<IStateBuilder>,
        IStateTransitions<IStateBuilder>,
        IStateSubmachine<IBehaviorStateBuilder>,
        IStateDoActivity<IBehaviorStateBuilder>
    { }

    public interface IBehaviorStateBuilder :
        IStateEvents<IBehaviorStateBuilder>,
        IStateUtils<IBehaviorStateBuilder>,
        IStateTransitions<IBehaviorStateBuilder>
    { }

    public interface ITypedStateBuilder :
        IStateUtils<ITypedStateBuilder>,
        IStateTransitions<ITypedStateBuilder>,
        IStateSubmachine<IBehaviorTypedStateBuilder>,
        IStateDoActivity<IBehaviorTypedStateBuilder>
    { }

    public interface IBehaviorTypedStateBuilder :
        IStateUtils<IBehaviorTypedStateBuilder>,
        IStateTransitions<IBehaviorTypedStateBuilder>
    { }
}

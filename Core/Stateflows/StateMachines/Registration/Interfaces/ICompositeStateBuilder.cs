﻿using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    public interface IInitializedCompositeStateBuilder :
        IStateEvents<IInitializedCompositeStateBuilder>,
        IStateUtils<IInitializedCompositeStateBuilder>,
        ICompositeStateEvents<IInitializedCompositeStateBuilder>,
        IStateTransitions<IInitializedCompositeStateBuilder>,
        IStateMachine<IInitializedCompositeStateBuilder>,
        //IStateDoActivity<IInitializedCompositeStateBuilder>,
        IStateMachineFinal<IFinalizedCompositeStateBuilder>
    { }

    public interface IFinalizedCompositeStateBuilder :
        IStateEvents<IFinalizedCompositeStateBuilder>,
        IStateUtils<IFinalizedCompositeStateBuilder>,
        ICompositeStateEvents<IFinalizedCompositeStateBuilder>,
        IStateTransitions<IFinalizedCompositeStateBuilder>
        //IStateDoActivity<IFinalizedCompositeStateBuilder>
    { }

    public interface ICompositeStateBuilder :
        IStateEvents<ICompositeStateBuilder>,
        IStateUtils<ICompositeStateBuilder>,
        ICompositeStateEvents<ICompositeStateBuilder>,
        IStateTransitions<ICompositeStateBuilder>,
        //IStateDoActivity<ICompositeStateBuilder>,
        IStateMachineInitial<IInitializedCompositeStateBuilder>
    { }

    public interface ITypedInitializedCompositeStateBuilder :
        IStateUtils<ITypedInitializedCompositeStateBuilder>,
        IStateTransitions<ITypedInitializedCompositeStateBuilder>,
        IStateMachine<ITypedInitializedCompositeStateBuilder>,
        //IStateDoActivity<ITypedInitializedCompositeStateBuilder>,
        IStateMachineFinal<ITypedFinalizedCompositeStateBuilder>
    { }

    public interface ITypedFinalizedCompositeStateBuilder :
        IStateUtils<ITypedFinalizedCompositeStateBuilder>,
        IStateTransitions<ITypedFinalizedCompositeStateBuilder>
        //IStateDoActivity<ITypedFinalizedCompositeStateBuilder>
    { }

    public interface ITypedCompositeStateBuilder :
        IStateUtils<ITypedCompositeStateBuilder>,
        IStateTransitions<ITypedCompositeStateBuilder>,
        //IStateDoActivity<ITypedCompositeStateBuilder>,
        IStateMachineInitial<ITypedInitializedCompositeStateBuilder>
    { }
}

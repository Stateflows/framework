using System;

namespace Stateflows.StateMachines
{
    [Obsolete("State class is deprecated, use IState instead.")]
    public abstract class State : BaseState, IBaseState
    { }
}

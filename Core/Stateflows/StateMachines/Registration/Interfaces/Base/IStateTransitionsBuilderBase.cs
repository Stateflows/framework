using Stateflows.Common;
using Stateflows.StateMachines.Events;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IStateTransitionsBuilderBase<TReturn>
    {
        #region Transitions
        TReturn AddTransition<TEvent>(string targetStateName, TransitionBuilderAction<TEvent> transitionBuildAction = null)
            where TEvent : Event, new();

        TReturn AddDefaultTransition(string targetStateName, TransitionBuilderAction<Completion> transitionBuildAction = null);

        TReturn AddInternalTransition<TEvent>(TransitionBuilderAction<TEvent> transitionBuildAction = null)
            where TEvent : Event, new();
        #endregion
    }
}

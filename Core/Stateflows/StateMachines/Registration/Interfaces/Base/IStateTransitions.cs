using Stateflows.Common;
using Stateflows.StateMachines.Events;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IStateTransitions<TReturn>
    {
        #region Transitions
        TReturn AddTransition<TEvent>(string targetVertexName, TransitionBuilderAction<TEvent> transitionBuildAction = null)
            where TEvent : Event;

        TReturn AddDefaultTransition(string targetVertexName, TransitionBuilderAction<Completion> transitionBuildAction = null);

        TReturn AddInternalTransition<TEvent>(TransitionBuilderAction<TEvent> transitionBuildAction = null)
            where TEvent : Event;
        #endregion
    }
}

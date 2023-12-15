using Stateflows.Common;
using Stateflows.StateMachines.Events;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IStateTransitions<TReturn>
    {
        #region Transitions
        TReturn AddTransition<TEvent>(string targetVertexName, TransitionBuilderAction<TEvent> transitionBuildAction = null)
            where TEvent : Event, new();

        TReturn AddDefaultTransition(string targetVertexName, TransitionBuilderAction<Completion> transitionBuildAction = null);

        TReturn AddInternalTransition<TEvent>(TransitionBuilderAction<TEvent> transitionBuildAction = null)
            where TEvent : Event, new();
        #endregion

        #region ElseTransitions
        TReturn AddElseTransition<TEvent>(string targetVertexName, ElseTransitionBuilderAction<TEvent> transitionBuildAction = null)
            where TEvent : Event, new();

        TReturn AddElseDefaultTransition(string targetVertexName, ElseTransitionBuilderAction<Completion> transitionBuildAction = null);

        TReturn AddElseInternalTransition<TEvent>(ElseTransitionBuilderAction<TEvent> transitionBuildAction = null)
            where TEvent : Event, new();
        #endregion
    }
}

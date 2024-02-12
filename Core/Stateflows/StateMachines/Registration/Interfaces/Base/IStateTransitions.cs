using Stateflows.Common;
using Stateflows.StateMachines.Events;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IStateTransitions<TReturn>
    {
        #region Transitions
        TReturn AddTransition<TEvent>(string targetVertexName, TransitionBuildAction<TEvent> transitionBuildAction = null)
            where TEvent : Event, new();

        TReturn AddDefaultTransition(string targetVertexName, TransitionBuildAction<CompletionEvent> transitionBuildAction = null);

        TReturn AddInternalTransition<TEvent>(TransitionBuildAction<TEvent> transitionBuildAction = null)
            where TEvent : Event, new();
        #endregion

        #region ElseTransitions
        TReturn AddElseTransition<TEvent>(string targetVertexName, ElseTransitionBuildAction<TEvent> transitionBuildAction = null)
            where TEvent : Event, new();

        TReturn AddElseDefaultTransition(string targetVertexName, ElseTransitionBuildAction<CompletionEvent> transitionBuildAction = null);

        TReturn AddElseInternalTransition<TEvent>(ElseTransitionBuildAction<TEvent> transitionBuildAction = null)
            where TEvent : Event, new();
        #endregion
    }
}

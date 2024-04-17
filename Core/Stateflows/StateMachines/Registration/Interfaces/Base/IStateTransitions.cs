using Stateflows.Common;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IStateTransitions<TReturn>
    {
        #region Transitions
        TReturn AddTransition<TEvent>(string targetVertexName, TransitionBuildAction<TEvent> transitionBuildAction = null)
            where TEvent : Event, new();

        TReturn AddDefaultTransition(string targetVertexName, DefaultTransitionBuildAction transitionBuildAction = null);

        TReturn AddInternalTransition<TEvent>(InternalTransitionBuildAction<TEvent> transitionBuildAction)
            where TEvent : Event, new();
        #endregion

        #region ElseTransitions
        TReturn AddElseTransition<TEvent>(string targetVertexName, ElseTransitionBuildAction<TEvent> transitionBuildAction = null)
            where TEvent : Event, new();

        TReturn AddElseDefaultTransition(string targetVertexName, ElseDefaultTransitionBuildAction transitionBuildAction = null);

        TReturn AddElseInternalTransition<TEvent>(ElseInternalTransitionBuildAction<TEvent> transitionBuildAction)
            where TEvent : Event, new();
        #endregion
    }
}

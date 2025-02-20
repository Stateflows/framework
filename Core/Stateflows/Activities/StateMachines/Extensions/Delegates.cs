using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.Activities.StateMachines.Interfaces;
using System.Threading.Tasks;

namespace Stateflows.Activities.Extensions
{
    public delegate Task<TInitializationEvent> StateActionBehaviorInitializationBuilderAsync<TInitializationEvent>(IStateActionContext context);
 
    public delegate Task<EventHolder> TransitionBehaviorInitializationBuilderAsync<TEvent, TInitializationEvent>(ITransitionContext<TEvent> context);
 
    public delegate void StateActionActivityBuildAction(IStateActionActivityBuilder builder);
    public delegate void StateActionActionBuildAction(IStateActionActionBuilder builder);

    public delegate void TransitionActivityBuildAction<TEvent>(ITransitionActivityBuilder<TEvent> builder);
    public delegate void TransitionActionBuildAction<TEvent>(ITransitionActionBuilder<TEvent> builder);
}
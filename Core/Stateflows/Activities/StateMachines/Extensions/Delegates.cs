using Stateflows.Common;
using Stateflows.Activities.StateMachines.Interfaces;
using System.Threading.Tasks;
using Stateflows.StateMachines;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.Activities.Extensions
{
    public delegate Task<TInitializationEvent> StateActionBehaviorInitializationBuilderAsync<TInitializationEvent>(IStateActionContext context);
 
    public delegate Task<TInitializationEvent> TransitionBehaviorInitializationBuilderAsync<TEvent, TInitializationEvent>(ITransitionContext<TEvent> context);
 
    public delegate void StateActionActivityBuildAction(IActionActivityBuilder builder);
    public delegate void StateActionActionBuildAction(IActionActionBuilder builder);

    public delegate void TransitionActivityBuildAction<TEvent>(ITransitionActivityBuilder<TEvent> builder);
    public delegate void TransitionActionBuildAction<TEvent>(ITransitionActionBuilder<TEvent> builder);
}
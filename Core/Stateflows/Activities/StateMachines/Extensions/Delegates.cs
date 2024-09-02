using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.Activities.StateMachines.Interfaces;
using System.Threading.Tasks;

namespace Stateflows.Activities.Extensions
{
    public delegate Event StateActionActivityInitializationBuilder(IStateActionContext context);

    public delegate Event TransitionActivityInitializationBuilder<in TEvent>(ITransitionContext<TEvent> context);

    public delegate Task<TInitializationEvent> StateActionActivityInitializationBuilderAsync<TInitializationEvent>(IStateActionContext context);

    public delegate Task<Event> TransitionActivityInitializationBuilderAsync<TEvent, TInitializationEvent>(ITransitionContext<TEvent> context);

    public delegate void StateActionActivityBuildAction(IStateActionActivityBuilder builder);

    public delegate void TransitionActivityBuildAction<TEvent>(ITransitionActivityBuilder<TEvent> builder);
}
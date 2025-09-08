using Stateflows.Activities.Extensions;

namespace Stateflows.Activities.StateMachines.Interfaces
{
    public interface ITransitionInstantation<TEvent, out TReturn>
    {
        TReturn InstantiateAs(TransitionBehaviorInstanceBuilderAsync<TEvent> builderAsync);
    }
    
    public interface ITransitionInstantation<TEvent>
    {
        void InstantiateAs(TransitionBehaviorInstanceBuilderAsync<TEvent> builderAsync);
    }
}

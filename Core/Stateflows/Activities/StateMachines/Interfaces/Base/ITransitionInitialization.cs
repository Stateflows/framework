using Stateflows.Activities.Extensions;

namespace Stateflows.Activities.StateMachines.Interfaces
{
    public interface ITransitionInitialization<TEvent, out TReturn>
    {
        TReturn InitializeWith<TInitializationEvent>(TransitionBehaviorInitializationBuilderAsync<TEvent, TInitializationEvent> builderAsync);
    }
}

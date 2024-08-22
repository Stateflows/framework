using Stateflows.Common;
using Stateflows.Activities.Extensions;

namespace Stateflows.Activities.StateMachines.Interfaces
{
    public interface ITransitionInitialization<TEvent, out TReturn>
        where TEvent : Event, new()
    {
        TReturn InitializeWith<TInitializationEvent>(TransitionActivityInitializationBuilderAsync<TEvent, TInitializationEvent> builderAsync)
            where TInitializationEvent : Event, new();
    }
}

using Stateflows.Common;
using Stateflows.Activities.Extensions;

namespace Stateflows.Activities.StateMachines.Interfaces
{
    public interface ITransitionInitialization<TEvent, out TReturn>

    {
        TReturn InitializeWith<TInitializationEvent>(TransitionActivityInitializationBuilderAsync<TEvent, TInitializationEvent> builderAsync);
    }
}

using Stateflows.Activities.Extensions;

namespace Stateflows.Activities.StateMachines.Interfaces
{
    public interface IStateActionInitialization<out TReturn>
    {
        TReturn InitializeWith<TInitializationEvent>(StateActionBehaviorInitializationBuilderAsync<TInitializationEvent> builderAsync);
    }
    
    public interface IStateActionInitialization
    {
        void InitializeWith<TInitializationEvent>(StateActionBehaviorInitializationBuilderAsync<TInitializationEvent> builderAsync);
    }
}

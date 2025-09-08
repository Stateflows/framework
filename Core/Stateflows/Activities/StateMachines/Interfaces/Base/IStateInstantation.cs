using Stateflows.Activities.Extensions;

namespace Stateflows.Activities.StateMachines.Interfaces
{
    public interface IStateActionInstantation<out TReturn>
    {
        TReturn InstantiateAs(StateActionBehaviorInstanceBuilderAsync builderAsync);
    }
    
    public interface IStateActionInstantation
    {
        void InstantiateAs(StateActionBehaviorInstanceBuilderAsync builderAsync);
    }
}

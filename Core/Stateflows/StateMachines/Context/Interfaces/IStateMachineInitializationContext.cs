using Stateflows.Common;

namespace Stateflows.StateMachines.Context.Interfaces
{
    public interface IStateMachineInitializationContext<out TInitializationRequest> : IStateMachineActionContext
        //where TInitializationRequest : InitializationRequest, new()
    {
        TInitializationRequest InitializationRequest { get; }
    }

    public interface IStateMachineInitializationContext : IStateMachineInitializationContext<object>
    { }
}

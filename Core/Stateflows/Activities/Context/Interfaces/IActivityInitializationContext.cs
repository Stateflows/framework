using Stateflows.Common;

namespace Stateflows.Activities.Context.Interfaces
{
    public interface IActivityInitializationContext<out TInitializationRequest> : IActivityActionContext
        where TInitializationRequest : InitializationRequest
    {
        TInitializationRequest InitializationRequest { get; }
    }

    public interface IActivityInitializationContext : IActivityInitializationContext<InitializationRequest>
    { }
}

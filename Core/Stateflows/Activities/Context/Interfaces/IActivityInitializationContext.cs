using Stateflows.Common;

namespace Stateflows.Activities.Context.Interfaces
{
    public interface IActivityInitializationContext<out TInitializationRequest> : IActivityActionContext, IOutput
    {
        TInitializationRequest InitializationEvent { get; }
    }

    public interface IActivityInitializationContext : IActivityInitializationContext<object>
    { }
}

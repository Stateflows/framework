using Stateflows.Common;

namespace Stateflows.Activities.Context.Interfaces
{
    public interface IActivityInitializationContext<out TInitializationRequest> : IActivityActionContext, IOutput
        where TInitializationRequest : Event
    {
        TInitializationRequest InitializationEvent { get; }
    }

    public interface IActivityInitializationContext : IActivityInitializationContext<Event>
    { }
}

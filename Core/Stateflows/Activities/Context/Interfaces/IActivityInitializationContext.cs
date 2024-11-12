namespace Stateflows.Activities.Context.Interfaces
{
    public interface IActivityInitializationContext<out TInitializationRequest> : IActivityInitializationContext
    {
        TInitializationRequest InitializationEvent { get; }
    }

    public interface IActivityInitializationContext : IActivityActionContext, IOutput
    { }
}

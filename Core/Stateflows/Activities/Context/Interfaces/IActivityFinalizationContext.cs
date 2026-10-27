namespace Stateflows.Activities.Context.Interfaces
{
    /// <summary>Context supplied while an activity instance is being finalized.</summary>
    /// <remarks>Finalization can run after the activity has completed or failed; use the context only for finalization work.</remarks>
    public interface IActivityFinalizationContext : IActivityActionContext
    { }
}

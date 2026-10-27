namespace Stateflows.Activities.Context.Interfaces
{
    /// <summary>Initialization context that exposes the request event that started the activity.</summary>
    /// <typeparam name="TInitializationRequest">The initialization request type.</typeparam>
    public interface IActivityInitializationContext<out TInitializationRequest> : IActivityInitializationContext
    {
        /// <summary>The request event supplied when the activity was initialized.</summary>
        TInitializationRequest InitializationEvent { get; }
    }

    /// <summary>Context supplied during activity initialization, including output-token operations.</summary>
    public interface IActivityInitializationContext : IActivityActionContext, IOutput
    { }
}

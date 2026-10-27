namespace Stateflows.Extensions.MinimalAPIs.Interfaces
{
    /// <summary>Describes an active State Machine state exposed to an HTTP handler.</summary>
    public interface IStateContext
    {
        /// <summary>Gets the registered state name.</summary>
        string Name { get; }
    }
}

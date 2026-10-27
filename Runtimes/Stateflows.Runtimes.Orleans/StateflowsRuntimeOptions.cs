namespace Stateflows;

/// <summary>
/// Configures persistence for the Orleans Stateflows runtime.
/// </summary>
public sealed class StateflowsRuntimeOptions
{
    /// <summary>
    /// The Orleans grain storage provider used for all Stateflows persistent state.
    /// Null selects Orleans' default provider. An explicit name must be nonempty.
    /// </summary>
    public string? StorageProviderName { get; set; }
}

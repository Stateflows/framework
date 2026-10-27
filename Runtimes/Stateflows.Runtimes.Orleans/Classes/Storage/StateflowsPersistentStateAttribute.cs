namespace Stateflows.Storage;

[AttributeUsage(AttributeTargets.Parameter)]
internal sealed class StateflowsPersistentStateAttribute(string stateName) : Attribute, IFacetMetadata
{
    public string StateName { get; } = stateName;
}

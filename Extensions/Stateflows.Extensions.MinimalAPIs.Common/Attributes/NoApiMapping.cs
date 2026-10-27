namespace Stateflows.Extensions.MinimalAPIs.Attributes;

/// <summary>Excludes the annotated behavior, event, field, or operation from automatic HTTP API mapping.</summary>
/// <remarks>This affects generated endpoints, not the underlying behavior's messaging contract.
/// See <see href="https://github.com/Stateflows/framework/wiki/extension-minimal-apis"/>.</remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface |
    AttributeTargets.Property | AttributeTargets.Method)]
public sealed class NoApiMappingAttribute : Attribute;
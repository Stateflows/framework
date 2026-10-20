namespace Stateflows.Extensions.MinimalAPIs.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface |
    AttributeTargets.Property | AttributeTargets.Method)]
public sealed class NoApiMappingAttribute : Attribute;
using System;
using Stateflows.Entities.Enums;

namespace Stateflows.Entities.Attributes;

/// <summary>Marks an entity property as a projection and configures its publication targets.</summary>
/// <param name="publishScope">Behaviors to which changes to this projection are published.</param>
[AttributeUsage(AttributeTargets.Property)]
public sealed class ProjectionAttribute(PublishScope publishScope = PublishScope.None) : Attribute
{
    /// <summary>Gets the publication scope for this projection.</summary>
    public PublishScope PublishScope { get; init; } = publishScope;
}
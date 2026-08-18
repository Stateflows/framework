using System;
using Stateflows.Entities.Enums;

namespace Stateflows.Entities.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public sealed class ProjectionAttribute(PublishScope publishScope = PublishScope.None) : Attribute
{
    public PublishScope PublishScope { get; init; } = publishScope;
}
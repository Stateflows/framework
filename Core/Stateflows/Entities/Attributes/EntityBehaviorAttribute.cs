using System;
using Stateflows.Common.Attributes;

namespace Stateflows.Entities.Attributes
{
    /// <summary>Configures the registered name, version, and resource group of an entity behavior.</summary>
    /// <param name="name">Optional registered name; when omitted, a name is inferred from the entity type.</param>
    /// <param name="version">Entity definition version.</param>
    /// <param name="resourceName">Optional name of the resource group hosting this entity.</param>
    [AttributeUsage(AttributeTargets.Class)]
    public class EntityBehaviorAttribute(string? name = null, int version = 1, string? resourceName = null)
        : BehaviorAttribute(name, version, resourceName);
}

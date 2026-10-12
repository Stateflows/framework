using System;
using Stateflows.Entities;

#nullable enable
namespace Stateflows.Common.Attributes
{
    public enum EntityScope
    {
        Self,
        Parent,
        Owner
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    public class FromProjectionAttribute(string entityName, EntityScope scope = EntityScope.Self, bool required = true) : Attribute
    {
        public string EntityName { get; init; } = entityName;
        public EntityScope Scope { get; init; } = scope;
        public bool Required { get; init; } = required;
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    public class FromProjectionAttribute<TTemplate>(EntityScope scope = EntityScope.Self, bool required = true) :
        FromProjectionAttribute(Entity<TTemplate>.Name, scope, required)
        where TTemplate : class;

    [AttributeUsage(AttributeTargets.Parameter)]
    public class FromProjectionAttribute<TTemplate, TEntity>(EntityScope scope = EntityScope.Self, bool required = true) :
        FromProjectionAttribute(Entity<TTemplate, TEntity>.Name, scope, required)
        where TTemplate : class
        where TEntity : class, IEntity<TTemplate>;
}

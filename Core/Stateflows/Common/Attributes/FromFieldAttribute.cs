using System;
using Stateflows.Entities;

#nullable enable
namespace Stateflows.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class FromFieldAttribute(string entityName, string? fieldName = null, EntityScope scope = EntityScope.Self, bool required = true) : Attribute
    {
        public string EntityName { get; init; } = entityName;
        public string? FieldName { get; init; } = fieldName;
        public EntityScope Scope { get; init; } = scope;
        public bool Required { get; init; } = required;
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    public class FromFieldAttribute<TTemplate>(string? fieldName = null, EntityScope scope = EntityScope.Self, bool required = true) :
        FromFieldAttribute(Entity<TTemplate>.Name, fieldName, scope, required)
        where TTemplate : class;

    [AttributeUsage(AttributeTargets.Parameter)]
    public class FromFieldAttribute<TTemplate, TEntity>(string? fieldName = null, EntityScope scope = EntityScope.Self, bool required = true) :
        FromFieldAttribute(Entity<TTemplate, TEntity>.Name, fieldName, scope, required)
        where TTemplate : class
        where TEntity : class, IEntity<TTemplate>;
}

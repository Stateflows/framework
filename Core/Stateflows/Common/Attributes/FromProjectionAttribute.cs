using System;
using Stateflows.Entities;

#nullable enable
namespace Stateflows.Common.Attributes
{
    /// <summary>Identifies where an entity projection is resolved for parameter injection.</summary>
    public enum EntityScope
    {
        /// <summary>The entity instance associated with the current operation.</summary>
        Self,
        /// <summary>The entity associated with the containing parent behavior.</summary>
        Parent,
        /// <summary>The entity associated with the owning behavior.</summary>
        Owner
    }

    /// <summary>Injects a projection from an entity behavior into an operation parameter.</summary>
    /// <param name="entityName">The registered entity name.</param>
    /// <param name="scope">The related entity scope from which to resolve the projection.</param>
    /// <param name="required">Whether resolving the projection is required.</param>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class FromProjectionAttribute(string entityName, EntityScope scope = EntityScope.Self, bool required = true) : Attribute
    {
        /// <summary>Gets the registered entity name.</summary>
        public string EntityName { get; init; } = entityName;
        /// <summary>Gets the related entity scope used to resolve the projection.</summary>
        public EntityScope Scope { get; init; } = scope;
        /// <summary>Gets whether the projection must be available.</summary>
        public bool Required { get; init; } = required;
    }

    /// <summary>Injects a projection from an entity selected by its template type.</summary>
    /// <typeparam name="TTemplate">The entity template type.</typeparam>
    /// <param name="scope">The related entity scope from which to resolve the projection.</param>
    /// <param name="required">Whether resolving the projection is required.</param>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class FromProjectionAttribute<TTemplate>(EntityScope scope = EntityScope.Self, bool required = true) :
        FromProjectionAttribute(Entity<TTemplate>.Name, scope, required)
        where TTemplate : class;

    /// <summary>Injects a projection from an entity selected by template and implementation types.</summary>
    /// <typeparam name="TTemplate">The entity template type.</typeparam>
    /// <typeparam name="TEntity">The entity implementation type.</typeparam>
    /// <param name="scope">The related entity scope from which to resolve the projection.</param>
    /// <param name="required">Whether resolving the projection is required.</param>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class FromProjectionAttribute<TTemplate, TEntity>(EntityScope scope = EntityScope.Self, bool required = true) :
        FromProjectionAttribute(Entity<TTemplate, TEntity>.Name, scope, required)
        where TTemplate : class
        where TEntity : class, IEntity<TTemplate>;
}

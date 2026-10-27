using System;
using Stateflows.Entities;

#nullable enable
namespace Stateflows.Common.Attributes
{
    /// <summary>Injects an entity field value into a behavior operation parameter.</summary>
    /// <param name="entityName">The registered name of the entity behavior.</param>
    /// <param name="fieldName">The field name; when omitted, the parameter name is used.</param>
    /// <param name="scope">The related entity scope to read from.</param>
    /// <param name="required">Whether resolution must find a field value.</param>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class FromFieldAttribute(string entityName, string? fieldName = null, EntityScope scope = EntityScope.Self, bool required = true) : Attribute
    {
        /// <summary>Gets the registered entity name.</summary>
        public string EntityName { get; init; } = entityName;
        /// <summary>Gets the field key, or <see langword="null"/> to use the parameter name.</summary>
        public string? FieldName { get; init; } = fieldName;
        /// <summary>Gets the related entity scope used to resolve the field.</summary>
        public EntityScope Scope { get; init; } = scope;
        /// <summary>Gets whether the field value is required.</summary>
        public bool Required { get; init; } = required;
    }

    /// <summary>Injects a field from an entity selected by its template type.</summary>
    /// <typeparam name="TTemplate">The entity template type.</typeparam>
    /// <param name="fieldName">The field name; when omitted, the parameter name is used.</param>
    /// <param name="scope">The related entity scope to read from.</param>
    /// <param name="required">Whether resolution must find a field value.</param>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class FromFieldAttribute<TTemplate>(string? fieldName = null, EntityScope scope = EntityScope.Self, bool required = true) :
        FromFieldAttribute(Entity<TTemplate>.Name, fieldName, scope, required)
        where TTemplate : class;

    /// <summary>Injects a field from an entity selected by template and implementation types.</summary>
    /// <typeparam name="TTemplate">The entity template type.</typeparam>
    /// <typeparam name="TEntity">The entity implementation type.</typeparam>
    /// <param name="fieldName">The field name; when omitted, the parameter name is used.</param>
    /// <param name="scope">The related entity scope to read from.</param>
    /// <param name="required">Whether resolution must find a field value.</param>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class FromFieldAttribute<TTemplate, TEntity>(string? fieldName = null, EntityScope scope = EntityScope.Self, bool required = true) :
        FromFieldAttribute(Entity<TTemplate, TEntity>.Name, fieldName, scope, required)
        where TTemplate : class
        where TEntity : class, IEntity<TTemplate>;
}

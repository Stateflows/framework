using System.Reflection;
using Stateflows.Common.Extensions;
using Stateflows.Entities.Attributes;

namespace Stateflows.Entities
{
    /// <summary>Defines a concrete entity behavior for a reusable entity template.</summary>
    /// <typeparam name="TTemplate">The template whose fields and projections are exposed by the entity.</typeparam>
    public interface IEntity<TTemplate>
        where TTemplate : class
    {
        /// <summary>Declares the entity's fields, mutations, projections, and initialization behavior.</summary>
        /// <param name="builder">The builder to configure for this entity definition.</param>
        static abstract void Build(IEntityBuilder<TTemplate> builder);
    }
    
    internal abstract class TemplatedEntity<TTemplate> : IEntity<TTemplate>
        where TTemplate : class
    {
        public static void Build(IEntityBuilder<TTemplate> builder)
        { }
    }

    /// <summary>Provides name and identifier helpers for entities using a template type.</summary>
    /// <typeparam name="TTemplate">The entity template type.</typeparam>
    public static class Entity<TTemplate>
        where TTemplate : class
    {
        /// <summary>Gets the inferred registered entity name for the template.</summary>
        public static string Name
        {
            get
            {
                var entityType = typeof(TTemplate);
                var attribute = entityType.GetCustomAttribute<EntityBehaviorAttribute>();
                return attribute != null && attribute.Name != null
                    ? attribute.Name
                    : entityType.GetReadableName(TypedElements.Entities);
            }
        }

        /// <summary>Creates the behavior class identifier for this entity template.</summary>
        /// <returns>The entity behavior class.</returns>
        public static BehaviorClass ToClass()
            => new BehaviorClass(BehaviorType.Entity, Name);

        /// <summary>Creates an identifier for an instance of this entity template.</summary>
        /// <param name="instance">The instance key; an empty value denotes the default instance.</param>
        /// <returns>The behavior identifier.</returns>
        public static BehaviorId ToId(string instance)
            => new BehaviorId(ToClass(), instance);
    }

    /// <summary>Provides name and identifier helpers for an entity template and concrete implementation.</summary>
    /// <typeparam name="TTemplate">The entity template type.</typeparam>
    /// <typeparam name="TEntity">The entity implementation type.</typeparam>
    public static class Entity<TTemplate, TEntity>
        where TTemplate : class
        where TEntity : class, IEntity<TTemplate>
    {
        /// <summary>Gets the inferred registered name for this template and entity implementation.</summary>
        public static string Name
        {
            get
            {
                var entityType = typeof(TEntity);
                var attribute = entityType.GetCustomAttribute<EntityBehaviorAttribute>();
                return attribute != null && attribute.Name != null
                    ? attribute.Name
                    : entityType.GetReadableName(TypedElements.Entities);
            }
        }

        /// <summary>Creates the behavior class identifier for this entity implementation.</summary>
        /// <returns>The entity behavior class.</returns>
        public static BehaviorClass ToClass()
            => new BehaviorClass(BehaviorType.Entity, Name);

        /// <summary>Creates an identifier for an instance of this entity implementation.</summary>
        /// <param name="instance">The instance key; an empty value denotes the default instance.</param>
        /// <returns>The behavior identifier.</returns>
        public static BehaviorId ToId(string instance)
            => new BehaviorId(ToClass(), instance);
    }
}

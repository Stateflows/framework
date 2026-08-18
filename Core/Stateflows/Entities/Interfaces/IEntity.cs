using System.Reflection;
using Stateflows.Common.Extensions;
using Stateflows.Entities.Attributes;

namespace Stateflows.Entities
{
    public interface IEntity<TEntityTemplate>
        where TEntityTemplate : class
    {
        static abstract void Build(IEntityBuilder<TEntityTemplate> builder);
    }

    public static class Entity<TEntityTemplate, TEntity>
        where TEntityTemplate : class
        where TEntity : class, IEntity<TEntityTemplate>
    {
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

        public static BehaviorClass ToClass()
            => new BehaviorClass(BehaviorType.Entity, Name);

        public static BehaviorId ToId(string instance)
            => new BehaviorId(ToClass(), instance);
    }
}


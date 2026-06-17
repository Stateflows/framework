using System.Reflection;
using Stateflows.Common.Extensions;
using Stateflows.Entities.Attributes;

namespace Stateflows.Entities
{
    public interface IEntity<TTemplate>
        where TTemplate : class
    {
        static abstract void Build(IEntityBuilder<TTemplate> builder);
    }

    public static class Entity<TTemplate, TEntity>
        where TTemplate : class
        where TEntity : class, IEntity<TTemplate>
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


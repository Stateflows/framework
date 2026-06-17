using System.Threading.Tasks;

namespace Stateflows.Entities
{
    public interface IEntityVisitor
    {
        Task EntityAddingAsync<TTemplate>(string entityName, int entityVersion, BehaviorClass? ownerClass = null, BehaviorClass? parentClass = null, bool hasDefaultInstance = false)
            where TTemplate : class;

        Task EntityAddedAsync<TTemplate>(string entityName, int entityVersion)
            where TTemplate : class;

        Task EntityTypeAddedAsync<TTemplate, TEntity>(string entityName, int entityVersion)
            where TTemplate : class
            where TEntity : class, IEntity<TTemplate>;
    }
}


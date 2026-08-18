using Stateflows.Entities;
using Stateflows.Entities.Registration.Interfaces;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IStateMachineEntity<out TReturn>
    {
        TReturn AddEntity<TTemplate>(EntityBuildAction<TTemplate>? buildAction = null)
            where TTemplate : class;
        
        TReturn AddEntity<TTemplate, TEntity>(EntityBuildAction<TTemplate>? buildAction = null)
            where TTemplate : class
            where TEntity : class, IEntity<TTemplate>;
    }
}

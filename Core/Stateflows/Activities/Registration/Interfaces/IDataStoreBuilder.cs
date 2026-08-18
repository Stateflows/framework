using Stateflows.Activities.Registration.Interfaces.Base;

namespace Stateflows.Activities.Registration.Interfaces
{
    public interface IDataStoreBuilder : IObjectFlowBase<IDataStoreBuilder>;
    
    public interface IOverridenDataStoreBuilder :
        IObjectFlowBase<IOverridenDataStoreBuilder>,
        IOverridenObjectFlowBase<IOverridenDataStoreBuilder>;
}

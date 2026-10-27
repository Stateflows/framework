using Stateflows.Activities.Registration.Interfaces.Base;

namespace Stateflows.Activities.Registration.Interfaces
{
    /// <summary>Configures a data-store node and its typed object flows.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-data-store-node" />
    public interface IDataStoreBuilder : IObjectFlowBase<IDataStoreBuilder>;
    
    /// <summary>Provides fluent APIs for extending elements inherited from a reusable Activity definition.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-data-store-node" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    public interface IOverridenDataStoreBuilder :
        IObjectFlowBase<IOverridenDataStoreBuilder>,
        IOverridenObjectFlowBase<IOverridenDataStoreBuilder>;
}

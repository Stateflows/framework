using System;
using System.Threading.Tasks;

namespace Stateflows.Common.Initializer
{

    /// <summary>Asynchronously creates the initialization event used to initialize a behavior's default instance.</summary>
    /// <param name="serviceProvider">The service provider available when creating the initialization event.</param>
    /// <param name="behaviorClass">The behavior class whose default instance is being initialized.</param>
    /// <returns>The initialization event payload to send to the default instance.</returns>
    public delegate Task<object> DefaultInstanceInitializationRequestFactoryAsync(IServiceProvider serviceProvider, BehaviorClass behaviorClass);
}
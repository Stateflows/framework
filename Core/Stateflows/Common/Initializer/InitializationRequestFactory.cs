using System;
using System.Threading.Tasks;

namespace Stateflows.Common.Initializer
{

    public delegate Task<object> DefaultInstanceInitializationRequestFactoryAsync(IServiceProvider serviceProvider, BehaviorClass behaviorClass);
}
using System;
using System.Threading.Tasks;

namespace Stateflows.Common.Initializer
{
    public delegate Task<InitializationRequest> InitializationRequestFactoryAsync(IServiceProvider serviceProvider, BehaviorClass behaviorClass);
}
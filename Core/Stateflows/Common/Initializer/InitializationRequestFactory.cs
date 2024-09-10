using System;
using System.Threading.Tasks;

namespace Stateflows.Common.Initializer
{

    public delegate Task<EventHolder> DefaultInstanceInitializationRequestFactoryAsync(IServiceProvider serviceProvider, BehaviorClass behaviorClass);
}
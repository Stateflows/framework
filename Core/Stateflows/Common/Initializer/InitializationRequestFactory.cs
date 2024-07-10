using System;
using System.Threading.Tasks;

namespace Stateflows.Common.Initializer
{

    public delegate Task<Event> DefaultInstanceInitializationRequestFactoryAsync(IServiceProvider serviceProvider, BehaviorClass behaviorClass);
//#nullable enable
//    public delegate Task<InitializationRequest?> AutoInitializationRequestFactoryAsync(IServiceProvider serviceProvider, BehaviorId behaviorId);
#nullable disable
}
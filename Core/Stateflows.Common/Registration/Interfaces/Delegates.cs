using System;

namespace Stateflows.Common.Registration.Interfaces
{
    public delegate IBehaviorClientInterceptor BehaviorClientInterceptorFactory(IServiceProvider serviceProvider);
}
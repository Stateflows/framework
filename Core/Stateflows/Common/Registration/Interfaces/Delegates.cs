using System;

namespace Stateflows.Common.Registration.Interfaces
{
    public delegate IBehaviorInterceptor BehaviorInterceptorFactory(IServiceProvider serviceProvider);
}
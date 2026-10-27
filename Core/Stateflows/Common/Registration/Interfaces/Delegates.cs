using System;

namespace Stateflows.Common.Registration.Interfaces
{
    /// <summary>Creates a behavior interceptor using the host service provider.</summary>
    /// <param name="serviceProvider">The service provider available to the interceptor factory.</param>
    /// <returns>The created interceptor.</returns>
    public delegate IBehaviorInterceptor BehaviorInterceptorFactory(IServiceProvider serviceProvider);
}
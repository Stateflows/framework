using System;

namespace Stateflows.Common.Registration.Interfaces
{
    /// <summary>Creates a client interceptor using the host service provider.</summary>
    /// <param name="serviceProvider">The service provider available to the interceptor factory.</param>
    /// <returns>The created client interceptor.</returns>
    public delegate IStateflowsClientInterceptor ClientInterceptorFactory(IServiceProvider serviceProvider);
    
    /// <summary>Creates a validator using the host service provider.</summary>
    /// <param name="serviceProvider">The service provider available to the validator factory.</param>
    /// <returns>The created validator.</returns>
    public delegate IStateflowsValidator ValidatorFactory(IServiceProvider serviceProvider);
}
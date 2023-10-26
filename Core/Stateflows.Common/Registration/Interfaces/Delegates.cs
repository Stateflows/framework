using System;

namespace Stateflows.Common.Registration.Interfaces
{
    public delegate IClientInterceptor ClientInterceptorFactory(IServiceProvider serviceProvider);
}
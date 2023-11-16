using System;

namespace Stateflows.Common.Registration.Interfaces
{
    public delegate IStateflowsClientInterceptor ClientInterceptorFactory(IServiceProvider serviceProvider);
}
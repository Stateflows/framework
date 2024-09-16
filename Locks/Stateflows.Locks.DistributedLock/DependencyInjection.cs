using System;
using System.Threading.Tasks;
using Medallion.Threading;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Exceptions;
using Stateflows.Common.Extensions;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Registration.Interfaces;
using Stateflows.Locks.DistributedLock.Classes;

namespace Stateflows
{
    public static class DependencyInjection
    {
        public static IStateflowsBuilder AddDistributedLock(this IStateflowsBuilder builder, Func<IServiceProvider, string, Task<IDistributedLock>> distributedLockFactory)
        {
            if (builder.ServiceCollection.IsServiceRegistered<IStateflowsLock>())
            {
                throw new StateflowsDefinitionException("Another Stateflows lock already registered");
            }

            builder
                .ServiceCollection
                .AddScoped<IStateflowsLock>(serviceProvider => new DistributedLockService(async id => await distributedLockFactory(serviceProvider, id)));

            return builder;
        }
    }
}

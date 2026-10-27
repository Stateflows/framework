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
    /// <summary>Registers a distributed lock implementation for Stateflows behavior execution.</summary>
    public static class DependencyInjection
    {
        /// <summary>Uses the supplied factory to acquire a distributed lock for each behavior identifier.</summary>
        /// <param name="builder">The Stateflows builder whose services receive the lock registration.</param>
        /// <param name="distributedLockFactory">Creates a lock from the current service provider and behavior lock identifier.</param>
        /// <returns>The same builder, for further Stateflows configuration.</returns>
        /// <remarks>Only one <see cref="IStateflowsLock"/> implementation may be registered in the container.</remarks>
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

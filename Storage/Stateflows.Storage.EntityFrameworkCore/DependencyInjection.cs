using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Extensions;
using Stateflows.Common.Exceptions;
using Stateflows.Common.Registration.Interfaces;
using Stateflows.Storage.EntityFrameworkCore.Stateflows;
using Stateflows.Storage.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Stateflows
{
    public static class DependencyInjection
    {
        public static IStateflowsBuilder AddEntityFrameworkCoreStorage<TDbContext>(this IStateflowsBuilder builder)
            where TDbContext : DbContext, IStateflowsDbContext_v1
        {
            if (builder.ServiceCollection.IsServiceRegistered<IStateflowsStorage>())
            {
                throw new StateflowsException("Another Stateflows storage already registered");
            }

            builder.ServiceCollection
                .AddTransient<IStateflowsStorage, EntityFrameworkCoreStorage>()
                .AddDbContext<IStateflowsDbContext_v1, TDbContext>()
            ;

            return builder;
        }
    }
}

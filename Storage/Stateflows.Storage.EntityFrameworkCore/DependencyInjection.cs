using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Extensions;
using Stateflows.Common.Registration.Interfaces;
using Stateflows.Storage.EntityFrameworkCore.Stateflows;
using Stateflows.Storage.EntityFrameworkCore.EntityFrameworkCore;

namespace Stateflows
{
    public static class DependencyInjection
    {
        public static IStateflowsBuilder AddEntityFrameworkCoreStorage(this IStateflowsBuilder builder, Action<DbContextOptionsBuilder>? optionsAction = null)
        {
            if (builder.Services.IsServiceRegistered<IStateflowsStorage>())
            {
                throw new Exception("Another Stateflows storage already registered");
            }

            builder.Services
                .AddTransient<IStateflowsStorage, EntityFrameworkCoreStorage>()
                .AddDbContextFactory<StateflowsDbContext>(optionsAction);

            return builder;
        }
    }
}

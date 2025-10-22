using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Classes;
using Stateflows.Common.Enums;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Extensions;
using Stateflows.Common.Exceptions;
using Stateflows.Common.Registration.Interfaces;
using Stateflows.Storage.EntityFrameworkCore.Stateflows;
using Stateflows.Storage.EntityFrameworkCore.EntityFrameworkCore;

namespace Stateflows
{
    public static class DependencyInjection
    {
        public static IStateflowsBuilder AddEntityFrameworkCoreStorage<TDbContext>(this IStateflowsBuilder builder, StorageKind storageKind = StorageKind.All)
            where TDbContext : DbContext, IStateflowsDbContext_v1
        {
            if (storageKind.HasFlag(StorageKind.Context))
            {
                if (builder.ServiceCollection.IsServiceRegistered<IStateflowsStorage>())
                {
                    throw new StateflowsDefinitionException("Another Stateflows storage already registered");
                }

                builder.ServiceCollection
                    .AddTransient<IStateflowsStorage, EntityFrameworkCoreStorage>();
            }

            if (storageKind.HasFlag(StorageKind.Notifications))
            {
                if (builder.ServiceCollection.IsServiceRegistered<IStateflowsNotificationsStorage>())
                {
                    throw new StateflowsDefinitionException("Another Stateflows notifications storage already registered");
                }

                builder.ServiceCollection
                    .AddTransient<IStateflowsNotificationsStorage, EntityFrameworkCoreNotificationsStorage>();
            }

            builder.ServiceCollection
                .AddScoped<IStateflowsDbContext_v1>(provider => provider.GetRequiredService<TDbContext>())
                .AddHostedService<NotificationsCleaner<TDbContext>>();

            return builder;
        }
    }
}

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
    /// <summary>Registers Stateflows persistence services backed by Entity Framework Core.</summary>
    public static class DependencyInjection
    {
        /// <summary>Registers the selected Stateflows storage services using the application's DbContext.</summary>
        /// <typeparam name="TDbContext">The application's context implementing <see cref="IStateflowsDbContext_v1"/>.</typeparam>
        /// <param name="builder">The Stateflows builder whose service collection receives the storage registrations.</param>
        /// <param name="storageKind">The categories of Stateflows data to persist; <see cref="StorageKind.All"/> selects all supported categories.</param>
        /// <returns>The same builder, for further Stateflows configuration.</returns>
        /// <remarks>Register the <typeparamref name="TDbContext"/> with dependency injection and implement its Stateflows entity sets before selecting the corresponding storage kinds.</remarks>
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
                    .AddTransient<IStateflowsStorage, EntityFrameworkCoreStorage<TDbContext>>();
            }

            if (storageKind.HasFlag(StorageKind.Notifications))
            {
                if (builder.ServiceCollection.IsServiceRegistered<IStateflowsNotificationsStorage>())
                {
                    throw new StateflowsDefinitionException("Another Stateflows notifications storage already registered");
                }

                builder.ServiceCollection
                    .AddTransient<IStateflowsNotificationsStorage, EntityFrameworkCoreNotificationsStorage<TDbContext>>();
            }

            if (storageKind.HasFlag(StorageKind.Values))
            {
                if (builder.ServiceCollection.IsServiceRegistered<IStateflowsValueStorage>())
                {
                    throw new StateflowsDefinitionException("Another Stateflows value storage already registered");
                }

                builder.ServiceCollection
                    .AddTransient<IStateflowsValueStorage, EntityFrameworkCoreValueStorage<TDbContext>>();
            }

            builder.ServiceCollection
                .AddScoped<IStateflowsDbContext_v1>(provider => provider.GetRequiredService<TDbContext>())
                .AddHostedService<NotificationsCleaner<TDbContext>>();

            return builder;
        }
    }
}

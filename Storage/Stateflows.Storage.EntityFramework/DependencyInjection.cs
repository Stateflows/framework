using System;
using System.Data.Entity;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Extensions;
using Stateflows.Common.Registration.Interfaces;
using Stateflows.Storage.EntityFramework;

namespace Stateflows
{
    public static class DependencyInjection
    {
        public static IStateflowsBuilder AddEntityFrameworkStorage(this IStateflowsBuilder builder)
        {
            if (builder.Services.IsServiceRegistered<IStateflowsStorage>())
            {
                throw new Exception("Another Stateflows storage already registered");
            }

            Database.SetInitializer(new CreateDatabaseIfNotExists<StateflowsDbContext>());

            builder.Services.AddTransient<IStateflowsStorage, EntityFrameworkStorage>();

            return builder;
        }
    }
}

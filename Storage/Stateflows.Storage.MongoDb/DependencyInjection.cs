using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Extensions;
using Stateflows.Common.Registration.Interfaces;
using Stateflows.Storage.MongoDB.Utils;
using Stateflows.Storage.MongoDB.Stateflows;
using MongoDB.Driver;
using System;

namespace Stateflows
{
    public static class DependencyInjection
    {
        public static IStateflowsBuilder AddMongoDBStorage(this IStateflowsBuilder builder, Func<IServiceProvider, MongoDatabaseConfiguration> settingProvider)
        {
            if (builder.Services.IsServiceRegistered<IStateflowsStorage>())
            {
                throw new Exception("Another Stateflows storage already registered");
            }

            builder.Services
                .AddSingleton<IStateflowsStorage>(provider =>
                {
                    var settings = settingProvider(provider);
                    return new MongoDBStorage(
                        new MongoClient(settings.MongoClientSettings), settings.DatabaseName);
                });

            return builder;
        }
    }
}
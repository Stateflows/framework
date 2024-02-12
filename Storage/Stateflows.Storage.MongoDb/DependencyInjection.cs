using System;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Stateflows.Common.Exceptions;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Extensions;
using Stateflows.Common.Registration.Interfaces;
using Stateflows.Storage.MongoDB.Utils;
using Stateflows.Storage.MongoDB.Stateflows;

namespace Stateflows
{
    public static class DependencyInjection
    {
        public static IStateflowsBuilder AddMongoDBStorage(this IStateflowsBuilder builder, Func<IServiceProvider, MongoDatabaseConfiguration> settingProvider)
        {
            if (builder.ServiceCollection.IsServiceRegistered<IStateflowsStorage>())
            {
                throw new StateflowsException("Another Stateflows storage already registered");
            }

            builder.ServiceCollection
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
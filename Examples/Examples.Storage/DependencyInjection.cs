using Stateflows;
using Stateflows.Common.Registration.Interfaces;
using Stateflows.Storage.MongoDB.Utils;

namespace Examples.Storage
{
    public static class DependencyInjection
    {
        public static IStateflowsBuilder AddStorage(this IStateflowsBuilder builder)
        {
            return builder
                //.AddEntityFrameworkCoreStorage<StateflowsDbContext>()
                .AddMongoDBStorage(provider => new MongoDatabaseConfiguration("localhost", 27017, "local"))
                ;
        }
    }
}

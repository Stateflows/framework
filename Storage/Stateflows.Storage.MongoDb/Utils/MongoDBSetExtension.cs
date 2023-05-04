using MongoDB.Driver;
using Stateflows.Common.Context;
using Stateflows.Storage.MongoDB.MongoDB.Entities;
using System.Threading.Tasks;

namespace Stateflows.Storage.MongoDB.Utils
{
    internal static class MongoDBSetExtension
    {
        public static async Task<Context_v1> FindOrCreate(this IMongoDatabase mongoDb, string collectionName, StateflowsContext context)
            => await FindOrCreate(mongoDb, collectionName, context.Id);

        public static async Task<Context_v1> FindOrCreate(this IMongoDatabase mongoDb, string collectionName, BehaviorId id)
        {
            var collection = mongoDb.GetCollection<Context_v1>(collectionName);

            return await collection
                    .Find(c => c.BehaviorId == id.ToString())
                    .FirstOrDefaultAsync() ?? new Context_v1(id.ToString(), string.Empty);
        }
    }
}

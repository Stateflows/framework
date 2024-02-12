using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using MongoDB.Driver;
using Stateflows.Storage.MongoDB.MongoDB.Entities;

namespace Stateflows.Storage.MongoDB.Utils
{
    internal static class MongoDBSetExtension
    {
        public static async Task<IEnumerable<StateflowsContext_v1>> FindContextByTimeTriggerAsync(this IMongoDatabase mongoDb, IEnumerable<BehaviorClass> behaviorClasses)
        {
            var collection = mongoDb.GetCollection<StateflowsContext_v1>(CollectionNames.StateflowsContexts_v1);
            var filter = 
                Builders<StateflowsContext_v1>.Filter.In(c => c.BehaviorClass, behaviorClasses.Select(c => c.ToString())) &
                Builders<StateflowsContext_v1>.Filter.Where(c => c.TriggerTime <= DateTime.Now);
            return await collection.Find(filter).ToListAsync();
        }

        public static async Task<IEnumerable<StateflowsContext_v1>> FindContextByBehaviorClassAsync(this IMongoDatabase mongoDb, IEnumerable<BehaviorClass> behaviorClasses)
        {
            var collection = mongoDb.GetCollection<StateflowsContext_v1>(CollectionNames.StateflowsContexts_v1);
            var filter = Builders<StateflowsContext_v1>.Filter.In(c => c.BehaviorClass, behaviorClasses.Select(c => c.ToString()));
            return await collection.Find(filter).ToListAsync();
        }

        public static async Task<StateflowsContext_v1> FindOrCreateContextAsync(this IMongoDatabase mongoDb, BehaviorId id)
            => await mongoDb.GetCollection<StateflowsContext_v1>(CollectionNames.StateflowsContexts_v1)
                .Find(c => c.BehaviorId == id)
                .FirstOrDefaultAsync() ?? new StateflowsContext_v1(id);

        public static async Task UpdateOrInsertContextAsync(this IMongoDatabase mongoDb, StateflowsContext_v1 contextEntity)
        {
            var collection = mongoDb.GetCollection<StateflowsContext_v1>(CollectionNames.StateflowsContexts_v1);

            if (contextEntity.Id == default)
            {
                await collection.InsertOneAsync(contextEntity);
            }
            else
            {
                var filter = Builders<StateflowsContext_v1>.Filter.Where(x => x.Id == contextEntity.Id);
                var updateDefBuilder = Builders<StateflowsContext_v1>.Update;
                var updateDef = updateDefBuilder.Combine(
                    updateDefBuilder
                    .Set(x => x.Data, contextEntity.Data))
                    .Set(x => x.TriggerTime, contextEntity.TriggerTime);

                await collection.UpdateOneAsync(filter, updateDef);
            }
        }

        public static async Task<IEnumerable<StateflowsTrace_v1>> FindTracesAsync(this IMongoDatabase mongoDb, BehaviorId id)
            => await mongoDb.GetCollection<StateflowsTrace_v1>(CollectionNames.StateflowsTraces_v1)
                .Find(c => c.BehaviorId == id)
                .ToListAsync();

        public static Task InsertTraceAsync(this IMongoDatabase mongoDb, StateflowsTrace_v1 traceEntity)
            => mongoDb.GetCollection<StateflowsTrace_v1>(CollectionNames.StateflowsTraces_v1).InsertOneAsync(traceEntity);
    }
}

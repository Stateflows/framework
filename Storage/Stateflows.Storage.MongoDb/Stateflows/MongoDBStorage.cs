using MongoDB.Bson;
using MongoDB.Driver;
using Stateflows.Common.Context;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Utilities;
using Stateflows.Storage.MongoDB.MongoDB.Entities;
using Stateflows.Storage.MongoDB.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TimeToken = Stateflows.Common.Classes.TimeToken;
using TimeTokenEntity = Stateflows.Storage.MongoDB.MongoDB.Entities.TimeToken_v1;

namespace Stateflows.Storage.MongoDB.Stateflows
{
    public class MongoDBStorage : IStateflowsStorage
    {
        private readonly IMongoDatabase _mongoDatabase;
        public MongoDBStorage(IMongoClient mongoDBClient, string databaseName)
        {
            _mongoDatabase = mongoDBClient.GetDatabase(databaseName);
        }

        public async Task<StateflowsContext> Hydrate(BehaviorId id)
        {
            var context = await MongoDBSetExtension.FindOrCreate(_mongoDatabase, CollectionNames.Context_v1, id);
            StateflowsContext result = StateflowsJsonConverter.DeserializeObject<StateflowsContext>(context?.Data ?? string.Empty)
                ?? new StateflowsContext() { Id = id };

            return result;
        }

        public async Task Dehydrate(StateflowsContext context)
        {
            var contextEntity = await _mongoDatabase.FindOrCreate(CollectionNames.Context_v1, context);
            contextEntity.Data = StateflowsJsonConverter.SerializeObject(context);
            await UpdateOrInsertContextData(contextEntity);
        }

        public async Task AddTimeTokens(TimeToken[] timeTokens)
        {
            if (!timeTokens.Any()) return;

            var tokens = new Dictionary<TimeToken, TimeTokenEntity>();
            var collection = _mongoDatabase.GetCollection<TimeTokenEntity>(CollectionNames.TimeToken_v1);

            foreach (var timeToken in timeTokens)
            {
                var timeTokenEntity = new TimeTokenEntity(
                    StateflowsJsonConverter.SerializeObject(timeToken.TargetId.BehaviorClass),
                    StateflowsJsonConverter.SerializeObject(timeToken)
                );
                tokens.Add(timeToken, timeTokenEntity);
            }

            await collection.InsertManyAsync(tokens.Values);
            UpdateIdsForInsertedTimeTokens(tokens, timeTokens);
        }

        public async Task ClearTimeTokens(BehaviorId behaviorId, IEnumerable<string> ids)
        {
            var collection = _mongoDatabase.GetCollection<TimeTokenEntity>(CollectionNames.TimeToken_v1);
            var objectIds = ids.Select(id => ObjectId.Parse(id)).ToList();

            var filter = Builders<TimeTokenEntity>.Filter.In("_id", objectIds);

            await collection.DeleteManyAsync(filter);
        }

        public async Task<IEnumerable<TimeToken>> GetTimeTokens(IEnumerable<BehaviorClass> behaviorClasses)
        {
            var collection = _mongoDatabase.GetCollection<TimeTokenEntity>(CollectionNames.TimeToken_v1);
            var behaviorClassStrings = behaviorClasses.Select(bc => StateflowsJsonConverter.SerializeObject(bc));

            return (await collection.Find(p => behaviorClassStrings.Contains(p.BehaviorClass)).ToListAsync())
                .Select(e =>
                {
                    var result = StateflowsJsonConverter.DeserializeObject<TimeToken>(e.Data);
                    if (result != null)
                    {
                        result.Id = e.Id.ToString();
                    }
                    else
                    {
                        result = new TimeToken();
                    }

                    return result;
                })
                .Where(e => e.Id != null)
                .ToArray();
        }

        private async Task UpdateOrInsertContextData(Context_v1 contextEntity)
        {
            var collection = _mongoDatabase.GetCollection<Context_v1>(CollectionNames.Context_v1);

            if (contextEntity.Id == default)
            {
                await collection.InsertOneAsync(contextEntity);
            }
            else
            {
                var filter = Builders<Context_v1>.Filter.Where(x => x.Id == contextEntity.Id);
                var updateDefBuilder = Builders<Context_v1>.Update;
                var updateDef = updateDefBuilder.Combine(
                    updateDefBuilder
                    .Set(x => x.Data, contextEntity.Data));

                await collection.UpdateOneAsync(filter, updateDef);
            }
        }

        private static void UpdateIdsForInsertedTimeTokens(Dictionary<TimeToken, TimeTokenEntity> tokens, TimeToken[] timeTokens)
        {
            var ids = tokens.Values.Select(_ => _.Id);

            for (int i = 0; i < timeTokens.Length; i++)
            {
                timeTokens[i].Id = ids.ElementAt(i).ToString();
            }
        }
    }
}

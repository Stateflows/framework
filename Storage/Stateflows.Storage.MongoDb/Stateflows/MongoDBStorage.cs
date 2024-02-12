using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using MongoDB.Driver;
using Stateflows.Common.Context;
using Stateflows.Common.Utilities;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Trace.Models;
using Stateflows.Storage.MongoDB.Utils;
using Stateflows.Storage.MongoDB.MongoDB.Entities;

namespace Stateflows.Storage.MongoDB.Stateflows
{
    public class MongoDBStorage : IStateflowsStorage
    {
        private readonly IMongoDatabase _mongoDatabase;
        public MongoDBStorage(IMongoClient mongoDBClient, string databaseName)
        {
            _mongoDatabase = mongoDBClient.GetDatabase(databaseName);
        }

        public async Task<StateflowsContext> HydrateAsync(BehaviorId behaviorId)
        {
            var context = await MongoDBSetExtension.FindOrCreateContextAsync(_mongoDatabase, behaviorId);
            StateflowsContext result = StateflowsJsonConverter.DeserializeObject<StateflowsContext>(context?.Data ?? string.Empty)
                ?? new StateflowsContext() { Id = behaviorId };

            return result;
        }

        public async Task DehydrateAsync(StateflowsContext context)
        {
            var contextEntity = await _mongoDatabase.FindOrCreateContextAsync(context.Id);
            contextEntity.Data = StateflowsJsonConverter.SerializePolymorphicObject(context);
            contextEntity.TriggerTime = context.TriggerTime;
            await _mongoDatabase.UpdateOrInsertContextAsync(contextEntity);
        }

        public async Task<IEnumerable<StateflowsContext>> GetContextsAsync(IEnumerable<BehaviorClass> behaviorClasses)
            => (await _mongoDatabase.FindContextByBehaviorClassAsync(behaviorClasses))
                .Select(e => StateflowsJsonConverter.DeserializeObject<StateflowsContext>(e.Data));

        public async Task<IEnumerable<StateflowsContext>> GetContextsToTimeTriggerAsync(IEnumerable<BehaviorClass> behaviorClasses)
            => (await _mongoDatabase.FindContextByTimeTriggerAsync(behaviorClasses))
                .Select(e => StateflowsJsonConverter.DeserializeObject<StateflowsContext>(e.Data));

        public Task SaveTraceAsync(BehaviorTrace behaviorTrace)
            => _mongoDatabase.InsertTraceAsync(
                new StateflowsTrace_v1(
                    behaviorTrace.BehaviorId,
                    behaviorTrace.ExecutedAt,
                    StateflowsJsonConverter.SerializePolymorphicObject(behaviorTrace)
                )
            );

        public async Task<IEnumerable<BehaviorTrace>> GetTracesAsync(BehaviorId behaviorId)
            => (await _mongoDatabase.FindTracesAsync(behaviorId))
                .Select(e => StateflowsJsonConverter.DeserializeObject<BehaviorTrace>(e.Data));
    }
}

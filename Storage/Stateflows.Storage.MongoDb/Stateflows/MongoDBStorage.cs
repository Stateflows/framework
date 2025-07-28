using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using MongoDB.Driver;
using Stateflows.Common.Context;
using Stateflows.Common.Utilities;
using Stateflows.Common.Interfaces;
using Stateflows.Storage.MongoDB.Utils;

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
                ?? new StateflowsContext(behaviorId);

            return result;
        }

        public async Task DehydrateAsync(StateflowsContext context)
        {
            var contextEntity = await _mongoDatabase.FindOrCreateContextAsync(context.Id);
            contextEntity.Data = StateflowsJsonConverter.SerializePolymorphicObject(context);
            contextEntity.TriggerTime = context.TriggerTime;
            contextEntity.TriggerOnStartup = context.TriggerOnStartup;
            await _mongoDatabase.UpdateOrInsertContextAsync(contextEntity);
        }

        public async Task<IEnumerable<StateflowsContext>> GetAllContextsAsync(IEnumerable<BehaviorClass> behaviorClasses)
            => (await _mongoDatabase.FindContextByBehaviorClassAsync(behaviorClasses))
                .Select(e => StateflowsJsonConverter.DeserializeObject<StateflowsContext>(e.Data));

        public async Task<IEnumerable<BehaviorId>> GetAllContextIdsAsync(IEnumerable<BehaviorClass> behaviorClasses)
            => (await _mongoDatabase.FindContextByBehaviorClassAsync(behaviorClasses))
                .Select(e => StateflowsJsonConverter.DeserializeObject<BehaviorId>(e.BehaviorId));

        public async Task<IEnumerable<StateflowsContext>> GetTimeTriggeredContextsAsync(IEnumerable<BehaviorClass> behaviorClasses)
            => (await _mongoDatabase.FindContextByTimeTriggerAsync(behaviorClasses))
                .Select(e => StateflowsJsonConverter.DeserializeObject<StateflowsContext>(e.Data));

        public async Task<IEnumerable<StateflowsContext>> GetStartupTriggeredContextsAsync(IEnumerable<BehaviorClass> behaviorClasses)
            => (await _mongoDatabase.FindContextByStartupTriggerAsync(behaviorClasses))
                .Select(e => StateflowsJsonConverter.DeserializeObject<StateflowsContext>(e.Data));
    }
}

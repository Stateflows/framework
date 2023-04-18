using System.Linq;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Collections.Generic;
using Stateflows.Common.Context;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Utilities;
using Stateflows.Storage.EntityFramework.Utils;
using TimeToken = Stateflows.Common.Classes.TimeToken;
using TimeTokenEntity = Stateflows.Storage.EntityFramework.EntityFramework.Entities.TimeToken_v1;

namespace Stateflows.Storage.EntityFramework
{
    internal class EntityFrameworkStorage : IStateflowsStorage
    {
        public async Task Dehydrate(StateflowsContext context)
        {
            using (var dbContext = new StateflowsDbContext())
            {
                var contextEntity = await dbContext.Contexts.FindOrCreate(context);
                contextEntity.Data = StateflowsJsonConverter.SerializeObject(context);
                if (contextEntity.Id == 0)
                {
                    dbContext.Contexts.Add(contextEntity);
                }
                else
                {
                    dbContext.Entry(contextEntity).State = EntityState.Modified;
                }

                await dbContext.SaveChangesAsync();
            }
        }

        public async Task<StateflowsContext> Hydrate(BehaviorId id)
        {
            using (var dbContext = new StateflowsDbContext())
            {
                var c = await dbContext.Contexts.FindOrCreate(id);

                return StateflowsJsonConverter.DeserializeObject<StateflowsContext>(c.Data ?? string.Empty)
                    ?? new StateflowsContext() { Id = id };
            }
        }

        public async Task AddTimeTokens(IEnumerable<TimeToken> timeTokens)
        {
            var tokens = new Dictionary<TimeToken, TimeTokenEntity>();
            using (var dbContext = new StateflowsDbContext())
            {
                foreach (var timeToken in timeTokens)
                {
                    var t = new TimeTokenEntity(
                        timeToken.TargetId.BehaviorClass.ToString(),
                        StateflowsJsonConverter.SerializeObject(timeToken)
                    );

                    tokens.Add(timeToken, t);

                    dbContext.TimeTokens.Add(t);

                    timeToken.Id = t.Id.ToString();
                }

                await dbContext.SaveChangesAsync();

                foreach (var timeToken in tokens.Keys)
                {
                    timeToken.Id = tokens[timeToken].Id.ToString();
                }
            }
        }

        public async Task<IEnumerable<TimeToken>> GetTimeTokens(IEnumerable<BehaviorClass> behaviorClasses)
        {
            using (var dbContext = new StateflowsDbContext())
            {
                return (await dbContext.TimeTokens.ToArrayAsync())
                    .Select(e =>
                    {
                        var result = StateflowsJsonConverter.DeserializeObject<TimeToken>(e.Data);
                        result.Id = e.Id.ToString();

                        return result;
                    })
                    .ToArray();
            }
        }

        public async Task ClearTimeTokens(BehaviorId behaviorId, IEnumerable<string> ids)
        {
            using (var dbContext = new StateflowsDbContext())
            {
                var tokens = await dbContext.TimeTokens
                    .Where(e => ids.ToArray().Contains(e.Id.ToString()))
                    .ToArrayAsync();
                dbContext.TimeTokens.RemoveRange(tokens);
                await dbContext.SaveChangesAsync();
            }
        }
    }
}

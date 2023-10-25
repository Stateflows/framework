using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Stateflows.Common.Context;
using Stateflows.Common.Utilities;
using Stateflows.Common.Interfaces;
using Stateflows.Storage.EntityFrameworkCore.Utils;
using Stateflows.Storage.EntityFrameworkCore.EntityFrameworkCore;
using TimeToken = Stateflows.Common.Classes.TimeToken;
using TimeTokenEntity = Stateflows.Storage.EntityFrameworkCore.EntityFrameworkCore.Entities.TimeToken_v1;

namespace Stateflows.Storage.EntityFrameworkCore.Stateflows
{
    internal class EntityFrameworkCoreStorage : IStateflowsStorage
    {
        private IStateflowsDbContext_v1 DbContext { get; set; }

        public EntityFrameworkCoreStorage(IStateflowsDbContext_v1 dbContext)
        {
            DbContext = dbContext;
        }

        public async Task Dehydrate(StateflowsContext context)
        {
            {
                var contextEntity = await DbContext.Contexts_v1.FindOrCreate(context);
                contextEntity.Data = StateflowsJsonConverter.SerializeObject(context);
                if (contextEntity.Id == 0)
                {
                    DbContext.Contexts_v1.Add(contextEntity);
                }
                else
                {
                    DbContext.Contexts_v1.Update(contextEntity);
                }

                await DbContext.SaveChangesAsync();
            }
        }

        public async Task<StateflowsContext> Hydrate(BehaviorId id)
        {
            StateflowsContext? result = null;
            try
            {
                {
                    var c = await DbContext.Contexts_v1.FindOrCreate(id);

                    result = StateflowsJsonConverter.DeserializeObject<StateflowsContext>(c.Data ?? string.Empty);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Exception catched in {nameof(EntityFrameworkCoreStorage)}.{nameof(Hydrate)}(): '{e.GetType().Name}' with message \"{e.Message}\"");
            }

            if (result == null)
            {
                result = new StateflowsContext() { Id = id };
            }

            return result;
        }

        public async Task AddTimeTokens(TimeToken[] timeTokens)
        {
            var tokens = new Dictionary<TimeToken, TimeTokenEntity>();
            {
                foreach (var timeToken in timeTokens)
                {
                    var t = new TimeTokenEntity(
                        timeToken.TargetId.BehaviorClass.ToString(),
                        StateflowsJsonConverter.SerializeObject(timeToken)
                    );

                    tokens.Add(timeToken, t);

                    DbContext.TimeTokens_v1.Add(t);

                    timeToken.Id = t.Id.ToString();
                }

                await DbContext.SaveChangesAsync();

                foreach (var timeToken in tokens.Keys)
                {
                    timeToken.Id = tokens[timeToken].Id.ToString();
                }
            }
        }

        public async Task<IEnumerable<TimeToken>> GetTimeTokens(IEnumerable<BehaviorClass> behaviorClasses)
        {
            {
                var behaviorClassStrings = behaviorClasses.Select(bc => StateflowsJsonConverter.SerializeObject(bc));
                return (await DbContext.TimeTokens_v1
                        .Where(t => behaviorClassStrings.Contains(t.BehaviorClass))
                        .ToArrayAsync()
                    )
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
        }

        public async Task ClearTimeTokens(BehaviorId behaviorId, IEnumerable<string> ids)
        {
            {
                var tokens = await DbContext.TimeTokens_v1
                    .Where(e => ids.ToArray().Contains(e.Id.ToString()))
                    .ToArrayAsync();

                DbContext.TimeTokens_v1.RemoveRange(tokens);
                await DbContext.SaveChangesAsync();
            }
        }
    }
}

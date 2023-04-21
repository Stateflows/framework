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
        private IDbContextFactory<StateflowsDbContext> DbContextFactory { get; set; }

        public EntityFrameworkCoreStorage(IDbContextFactory<StateflowsDbContext> dbContextFactory)
        {
            DbContextFactory = dbContextFactory;
        }

        public async Task Dehydrate(StateflowsContext context)
        {
            using (var dbContext = DbContextFactory.CreateDbContext())
            {
                var contextEntity = await dbContext.Contexts_v1.FindOrCreate(context);
                contextEntity.Data = StateflowsJsonConverter.SerializeObject(context);
                if (contextEntity.Id == 0)
                {
                    dbContext.Contexts_v1.Add(contextEntity);
                }
                else
                {
                    dbContext.Contexts_v1.Update(contextEntity);
                }

                await dbContext.SaveChangesAsync();
            }
        }

        public async Task<StateflowsContext> Hydrate(BehaviorId id)
        {
            StateflowsContext? result = null;
            try
            {
                using (var dbContext = DbContextFactory.CreateDbContext())
                {
                    var c = await dbContext.Contexts_v1.FindOrCreate(id);

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
            using (var dbContext = DbContextFactory.CreateDbContext())
            {
                foreach (var timeToken in timeTokens)
                {
                    var t = new TimeTokenEntity(
                        timeToken.TargetId.BehaviorClass.ToString(),
                        StateflowsJsonConverter.SerializeObject(timeToken)
                    );

                    tokens.Add(timeToken, t);

                    dbContext.TimeTokens_v1.Add(t);

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
            using (var dbContext = DbContextFactory.CreateDbContext())
            {
                var behaviorClassStrings = behaviorClasses.Select(bc => StateflowsJsonConverter.SerializeObject(bc));
                return (await dbContext.TimeTokens_v1
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
            using (var dbContext = DbContextFactory.CreateDbContext())
            {
                var tokens = await dbContext.TimeTokens_v1
                    .Where(e => ids.ToArray().Contains(e.Id.ToString()))
                    .ToArrayAsync();

                dbContext.TimeTokens_v1.RemoveRange(tokens);
                await dbContext.SaveChangesAsync();
            }
        }
    }
}

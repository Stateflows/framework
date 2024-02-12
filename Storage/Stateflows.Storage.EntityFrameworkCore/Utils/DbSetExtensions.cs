using Stateflows.Common.Context;
using Microsoft.EntityFrameworkCore;
using Stateflows.Storage.EntityFrameworkCore.EntityFrameworkCore.Entities;

namespace Stateflows.Storage.EntityFrameworkCore.Utils
{
    internal static class DbSetExtensions
    {
        public static async Task<Context_v1> FindOrCreate(this DbSet<Context_v1> dbSet, StateflowsContext context, bool track = false)
            => await FindOrCreate(dbSet, context.Id, track);

        public static async Task<Context_v1> FindOrCreate(this DbSet<Context_v1> dbSet, BehaviorId id, bool track = false)
        {
            var query = dbSet.Where(c => c.BehaviorId == id.ToString());
            if (!track)
            {
                query = query.AsNoTracking();
            }
                
            return await query.FirstOrDefaultAsync() ?? new Context_v1(id.BehaviorClass.ToString(), id.ToString(), null, "");
        }

        public static async Task<IEnumerable<Context_v1>> FindByClassesAsync(this DbSet<Context_v1> dbSet, IEnumerable<BehaviorClass> behaviorClasses)
        {
            var behaviorClassStrings = behaviorClasses.Select(bc => bc.ToString());

            return await dbSet
                .Where(c => behaviorClassStrings.Contains(c.BehaviorClass))
                .AsNoTracking()
                .ToArrayAsync();
        }

        public static async Task<IEnumerable<Context_v1>> FindByTriggerTimeAsync(this DbSet<Context_v1> dbSet, IEnumerable<BehaviorClass> behaviorClasses)
        {
            var behaviorClassStrings = behaviorClasses.Select(bc => bc.ToString());
            var now = DateTime.Now;
            return await dbSet
                .Where(c =>
                    behaviorClassStrings.Contains(c.BehaviorClass) &&
                    c.TriggerTime != null &&
                    c.TriggerTime < now
                )
                .AsNoTracking()
                .ToArrayAsync();
        }

        public static IEnumerable<Context_v1> FindByTriggerTime(this DbSet<Context_v1> dbSet, IEnumerable<BehaviorClass> behaviorClasses)
        {
            var behaviorClassStrings = behaviorClasses.Select(bc => bc.ToString());
            var now = DateTime.Now;
            return dbSet
                .Where(c =>
                    behaviorClassStrings.Contains(c.BehaviorClass) &&
                    c.TriggerTime != null &&
                    c.TriggerTime < now
                )
                .AsNoTracking()
                .ToArray();
        }

        public static async Task<IEnumerable<Trace_v1>> FindByBehaviorIdAsync(this DbSet<Trace_v1> dbSet, BehaviorId behaviorId)
            => await dbSet
                .Where(c => c.BehaviorId == behaviorId.ToString())
                .AsNoTracking()
                .ToArrayAsync();
    }
}

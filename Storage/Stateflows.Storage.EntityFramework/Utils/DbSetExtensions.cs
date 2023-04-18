using System.Linq;
using System.Data.Entity;
using System.Threading.Tasks;
using Stateflows.Common.Context;
using Stateflows.Storage.EntityFramework.EntityFramework.Entities;

namespace Stateflows.Storage.EntityFramework.Utils
{
    internal static class DbSetExtensions
    {
        public static async Task<Context_v1> FindOrCreate(this DbSet<Context_v1> dbSet, StateflowsContext context)
            => await FindOrCreate(dbSet, context.Id);

        public static async Task<Context_v1> FindOrCreate(this DbSet<Context_v1> dbSet, BehaviorId id)
        {
            var stringId = id.ToString();
            return await dbSet
                .Where(c => c.BehaviorId == stringId)
                .FirstOrDefaultAsync() ?? new Context_v1() { BehaviorId = id.ToString() };
        }
    }
}

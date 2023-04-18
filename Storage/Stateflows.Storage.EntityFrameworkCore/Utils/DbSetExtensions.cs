using System.Linq;
using Stateflows.Common.Context;
using Microsoft.EntityFrameworkCore;
using Stateflows.Storage.EntityFrameworkCore.EntityFrameworkCore.Entities;

namespace Stateflows.Storage.EntityFrameworkCore.Utils
{
    internal static class DbSetExtensions
    {
        public static async Task<Context_v1> FindOrCreate(this DbSet<Context_v1> dbSet, StateflowsContext context)
            => await FindOrCreate(dbSet, context.Id);

        public static async Task<Context_v1> FindOrCreate(this DbSet<Context_v1> dbSet, BehaviorId id)
        {
            return await dbSet
                .Where(c => c.BehaviorId == id.ToString())
                .FirstOrDefaultAsync() ?? new Context_v1(id.ToString(), "");
        }
    }
}

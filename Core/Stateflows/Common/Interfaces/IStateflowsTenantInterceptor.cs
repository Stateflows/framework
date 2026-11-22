using System;
using System.Threading.Tasks;

namespace Stateflows.Common
{
    /// <summary>Wraps tenant-scoped execution.</summary>
    public interface IStateflowsTenantInterceptor
    {
        /// <summary>Wraps an operation executed within a tenant scope.</summary>
        /// <param name="tenantId">The tenant identifier.</param>
        /// <param name="next">The remaining interceptors and tenant operation; await it at most once to continue.</param>
        Task ExecuteAsync(string tenantId, Func<Task> next);
    }
}

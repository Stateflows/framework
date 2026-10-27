using System;
using System.Threading.Tasks;

namespace Stateflows.Common
{
    /// <summary>Executes operations within one or more tenant scopes.</summary>
    public interface IStateflowsTenantExecutor
    {
        /// <summary>Executes an operation once for each available tenant.</summary>
        /// <param name="tenantAction">The operation to execute for each tenant.</param>
        Task ExecuteByTenantsAsync(Func<Task> tenantAction);

        /// <summary>Executes an operation within a specified tenant scope.</summary>
        /// <param name="tenantId">The tenant identifier.</param>
        /// <param name="tenantAction">The operation to execute within that tenant.</param>
        Task ExecuteByTenantAsync(string tenantId, Func<Task> tenantAction);
    }
}

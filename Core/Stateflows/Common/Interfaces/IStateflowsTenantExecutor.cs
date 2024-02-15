using System;
using System.Threading.Tasks;

namespace Stateflows.Common
{
    public interface IStateflowsTenantExecutor
    {
        Task ExecuteByTenantsAsync(Func<Task> tenantAction);

        Task ExecuteByTenantAsync(string tenantId, Func<Task> tenantAction);
    }
}

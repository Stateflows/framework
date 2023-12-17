using System;
using System.Threading.Tasks;
using Stateflows.Common.Interfaces;

namespace Stateflows.Common.Tenant
{
    internal class SingleTenantManager : IStateflowsTenantsManager
    {
        private readonly string TenantId = "mainTenant";

        public Task ExecuteByTenantsAsync(Func<string, Task> tenantAction)
            => tenantAction?.Invoke(TenantId);
    }
}

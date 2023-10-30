using System;
using System.Threading.Tasks;
using Stateflows.Common.Interfaces;

namespace Stateflows.Common.Classes
{
    public class SingleTenantManager : IStateflowsTenantsManager
    {
        private readonly string TenantId = "mainTenant";

        public Task ExecuteByTenants(Func<string, Task> tenantAction)
            => tenantAction?.Invoke(TenantId);
    }
}

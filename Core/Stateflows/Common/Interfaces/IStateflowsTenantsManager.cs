using System;
using System.Threading.Tasks;

namespace Stateflows.Common.Interfaces
{
    public interface IStateflowsTenantsManager
    {
        Task ExecuteByTenants(Func<string, Task> tenantAction);
    }
}
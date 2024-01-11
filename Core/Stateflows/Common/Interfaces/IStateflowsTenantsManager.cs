using System;
using System.Threading.Tasks;

namespace Stateflows.Common.Interfaces
{
    public interface IStateflowsTenantsManager
    {
        Task ExecuteByTenantsAsync(Func<string, Task> tenantAction);
    }
}
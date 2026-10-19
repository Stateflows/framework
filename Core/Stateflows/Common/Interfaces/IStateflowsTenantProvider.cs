using System.Threading.Tasks;
using System.Collections.Generic;

namespace Stateflows.Common.Interfaces
{
    public interface IStateflowsTenantProvider
    {
        Task<IEnumerable<string>> GetAllTenantsAsync();

        string GetCurrentTenantId();
    }
}
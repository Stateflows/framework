using System.Threading.Tasks;
using System.Collections.Generic;

namespace Stateflows.Common.Interfaces
{
    /// <summary>Provides tenant identifiers for tenant-aware execution.</summary>
    public interface IStateflowsTenantProvider
    {
        /// <summary>Gets the identifiers of all available tenants.</summary>
        /// <returns>The tenant identifiers.</returns>
        Task<IEnumerable<string>> GetAllTenantsAsync();

        /// <summary>Gets the identifier of the tenant associated with the current execution context.</summary>
        /// <returns>The current tenant identifier.</returns>
        string GetCurrentTenantId();
    }
}
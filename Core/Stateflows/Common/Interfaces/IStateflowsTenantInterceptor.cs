namespace Stateflows.Common
{
    /// <summary>Provides callbacks around tenant-scoped execution.</summary>
    public interface IStateflowsTenantInterceptor
    {
        /// <summary>Runs before execution enters a tenant scope.</summary>
        /// <param name="tenantId">The tenant identifier being entered.</param>
        /// <returns><see langword="true"/> when execution should proceed.</returns>
        bool BeforeExecute(string tenantId);

        /// <summary>Runs after execution leaves a tenant scope.</summary>
        /// <param name="tenantId">The tenant identifier being left.</param>
        void AfterExecute(string tenantId);
    }
}

namespace Stateflows.Common.Interfaces
{
    /// <summary>Exposes the tenant identifier associated with the current execution.</summary>
    public interface ITenantAccessor
    {
        /// <summary>Gets or sets the current tenant identifier.</summary>
        string CurrentTenantId { get; set; }
    }
}
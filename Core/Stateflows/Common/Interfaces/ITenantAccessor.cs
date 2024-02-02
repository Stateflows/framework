namespace Stateflows.Common.Interfaces
{
    public interface ITenantAccessor
    {
        string CurrentTenantId { get; set; }
    }
}
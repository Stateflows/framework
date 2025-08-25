using System;
using Stateflows.Common.Interfaces;

namespace Stateflows.Common.Tenant
{
    internal class SingleTenantAccessor : ITenantAccessor
    {
        public SingleTenantAccessor(string tenantId)
        {
            TenantId = tenantId;;
        }
        
        private readonly string TenantId;

        public string CurrentTenantId
        {
            get => TenantId;
            set => throw new InvalidOperationException("CurrentTenant cannot be changed.");
        }
    }
}

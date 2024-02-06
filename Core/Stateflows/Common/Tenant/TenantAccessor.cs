using System.Threading;
using Stateflows.Common.Interfaces;

namespace Stateflows.Common.Tenant
{
    internal class TenantAccessor : ITenantAccessor
    {
        private readonly AsyncLocal<string> TenantId = new AsyncLocal<string>();

        public string CurrentTenantId
        {
            get => TenantId.Value;
            set => TenantId.Value = value;
        }
    }
}

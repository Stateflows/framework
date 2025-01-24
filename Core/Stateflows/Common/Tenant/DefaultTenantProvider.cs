﻿using System.Threading.Tasks;
using System.Collections.Generic;
using Stateflows.Common.Interfaces;

namespace Stateflows.Common.Tenant
{
    internal class DefaultTenantProvider : IStateflowsTenantProvider
    {
        public static readonly string TenantId = "host";

        public Task<string> GetCurrentTenantIdAsync()
            => Task.FromResult(TenantId);

        public Task<IEnumerable<string>> GetAllTenantsAsync()
            => Task.FromResult(new string[] { TenantId } as IEnumerable<string>);
    }
}

using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Stateflows.Common.Interfaces;

namespace Stateflows.Common.Cache
{
    public class InMemoryCache : IStateflowsCache
    {
        private readonly Dictionary<string, string> Cache = new Dictionary<string, string>();
        private readonly IStateflowsTenantProvider TenantProvider;

        public InMemoryCache(IStateflowsTenantProvider tenantProvider)
        {
            TenantProvider = tenantProvider;
        }

        private string GetKey(string tenantId, string key)
            => $"{tenantId}-{key}";
        
        public async Task SetAsync(string key, string value)
        {
            var tenantId = await TenantProvider.GetCurrentTenantIdAsync();

            lock (Cache)
            {
                Cache[GetKey(tenantId, key)] = value;
            }
        }

        public async Task UpdateAsync(string key, Func<string, string> valueUpdater, string defaultValue = null)
        {
            var tenantId = await TenantProvider.GetCurrentTenantIdAsync();

            lock (Cache)
            {
                if (!Cache.TryGetValue(GetKey(tenantId, key), out var value))
                {
                    value = defaultValue;
                }
                
                value = valueUpdater(value);
                
                Cache[GetKey(tenantId, key)] = value;
            }
        }

        public async Task<(bool Success, string Value)> TryGetAsync(string key)
        {
            var tenantId = await TenantProvider.GetCurrentTenantIdAsync();
            bool result = false;
            string value = null;

            lock (Cache)
            {
                result = Cache.TryGetValue(GetKey(tenantId, key), out value);
            }

            return (result, value);
        }

        public async Task RemoveAsync(string key)
        {
            var tenantId = await TenantProvider.GetCurrentTenantIdAsync();

            lock (Cache)
            {
                Cache.Remove(GetKey(tenantId, key));
            }
        }
    }
}
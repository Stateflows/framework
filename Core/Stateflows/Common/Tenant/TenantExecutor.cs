using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Stateflows.Common.Engine;
using Stateflows.Common.Interfaces;

namespace Stateflows.Common.Tenant
{
    internal class TenantExecutor : IStateflowsTenantExecutor
    {
        private readonly IStateflowsTenantProvider TenantsProvider;
        private readonly ITenantAccessor TenantAccessor;
        private readonly CommonInterceptor Interceptor;
        private readonly ILogger<TenantExecutor> Logger;

        public TenantExecutor(IStateflowsTenantProvider tenantsProvider, ITenantAccessor tenantAccessor, CommonInterceptor interceptor, ILogger<TenantExecutor> logger)
        {
            TenantsProvider = tenantsProvider;
            TenantAccessor = tenantAccessor;
            Interceptor = interceptor;
            Logger = logger;
        }

        public async Task ExecuteByTenantsAsync(Func<Task> tenantAction)
        {
            var tenantIds = await TenantsProvider.GetAllTenantsAsync();

            foreach (var tenantId in tenantIds)
            {
                await ExecuteByTenantAsync(tenantId, tenantAction);
            }
        }

        public async Task ExecuteByTenantAsync(string tenantId, Func<Task> tenantAction)
        {
            TenantAccessor.CurrentTenantId = tenantId;
            try
            {
                await Interceptor.ExecuteAsync(tenantId, tenantAction);
            }
            catch (Exception e)
            {
                Logger.LogError(LogTemplates.ExceptionLogTemplate, typeof(TenantExecutor).FullName, nameof(ExecuteByTenantAsync), e);
            }
            finally
            {
                TenantAccessor.CurrentTenantId = null;
            }
        }
    }
}

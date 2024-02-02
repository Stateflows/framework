using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Stateflows.Common.Engine;
using Stateflows.Common.Interfaces;

namespace Stateflows.Common.Tenant
{
    internal class TenantsExecutor
    {
        private readonly IStateflowsTenantProvider TenantsProvider;
        private readonly ITenantAccessor TenantAccessor;
        private readonly CommonInterceptor Interceptor;
        private readonly ILogger<TenantsExecutor> Logger;

        public TenantsExecutor(IStateflowsTenantProvider tenantsProvider, ITenantAccessor tenantAccessor, CommonInterceptor interceptor, ILogger<TenantsExecutor> logger)
        {
            TenantsProvider = tenantsProvider;
            TenantAccessor = tenantAccessor;
            Interceptor = interceptor;
            Logger = logger;
        }

        public async Task ExecuteByTenantsAsync(Func<Task> tenantAction)
        {
            var tenantIds = await TenantsProvider.GetAllTenantsAsync();

            await Task.WhenAll(
                tenantIds.Select(
                    tenantId =>
                    {
                        Task result = null;

                        TenantAccessor.CurrentTenantId = tenantId;

                        if (Interceptor.BeforeExecute(tenantId))
                        {
                            try
                            {
                                try
                                {
                                    result = tenantAction();
                                }
                                catch (Exception e)
                                {
                                    Logger.LogError(LogTemplates.ExceptionLogTemplate, typeof(TenantsExecutor).FullName, nameof(ExecuteByTenantsAsync), e.GetType().Name, e.Message);
                                }
                            }
                            finally
                            {
                                Interceptor.AfterExecute(tenantId);
                                TenantAccessor.CurrentTenantId = null;
                            }
                        }

                        return result ?? Task.CompletedTask;
                    }
                )
            );
        }
    }
}

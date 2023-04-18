using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Stateflows.Common.Extensions
{
    public static class IServiceCollectionExtensions
    {
        public static bool IsServiceRegistered<TType>(this IServiceCollection services)
            => services.Any(x => x.ServiceType == typeof(TType));

        public static bool IsImplementationRegistered<TType>(this IServiceCollection services)
            => services.Any(x => x.ImplementationType == typeof(TType));

        public static IServiceCollection AddServiceType<TType>(this IServiceCollection services)
            where TType : class
        {
            if (!services.IsServiceRegistered<TType>())
            {
                services.AddScoped<TType>();
            }

            return services;
        }
    }
}

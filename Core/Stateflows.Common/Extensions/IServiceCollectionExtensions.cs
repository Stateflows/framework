using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Stateflows.Common.Extensions
{
    public static class IServiceCollectionExtensions
    {
        public static bool IsServiceRegistered(this IServiceCollection services, Type type)
            => services.Any(x => x.ServiceType == type);

        public static bool IsServiceRegistered<TType>(this IServiceCollection services)
            => services.IsServiceRegistered(typeof(TType));

        public static IServiceCollection AddServiceType(this IServiceCollection services, Type type)
        {
            if (!services.IsServiceRegistered(type))
            {
                services.AddScoped(type);
            }

            return services;
        }

        public static IServiceCollection AddServiceType<TType>(this IServiceCollection services)
            where TType : class
            => services.AddServiceType(typeof(TType));

        public static IServiceCollection RegisterClientInterceptor<TClientInterceptor>(this IServiceCollection services)
            where TClientInterceptor : class, IBehaviorClientInterceptor
            => services?.AddServiceType<TClientInterceptor>();
    }
}

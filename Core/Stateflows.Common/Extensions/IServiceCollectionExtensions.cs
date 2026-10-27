using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Stateflows.Common.Extensions
{
    /// <summary>Provides service-registration helpers used when configuring Stateflows integrations.</summary>
    public static class IServiceCollectionExtensions
    {
        /// <summary>Checks whether the collection contains a descriptor for an exact service type.</summary>
        /// <param name="services">The service collection to inspect.</param>
        /// <param name="type">The service type to find.</param>
        /// <returns>Whether a descriptor with the specified service type is registered.</returns>
        public static bool IsServiceRegistered(this IServiceCollection services, Type type)
            => services.Any(x => x.ServiceType == type);

        /// <summary>Checks whether the collection contains a descriptor for a service type.</summary>
        /// <typeparam name="TType">The exact service type to find.</typeparam>
        /// <param name="services">The service collection to inspect.</param>
        /// <returns>Whether a descriptor for the type is registered.</returns>
        public static bool IsServiceRegistered<TType>(this IServiceCollection services)
            => services.IsServiceRegistered(typeof(TType));

        /// <summary>Registers a concrete type as a scoped service only if its service type is not already registered.</summary>
        /// <param name="services">The collection to update.</param>
        /// <param name="type">The concrete service type to register.</param>
        /// <returns>The supplied collection for fluent configuration.</returns>
        public static IServiceCollection AddServiceType(this IServiceCollection services, Type type)
        {
            if (!services.IsServiceRegistered(type))
            {
                services.AddScoped(type);
            }

            return services;
        }

        /// <summary>Registers a concrete type as scoped only if its service type is not already registered.</summary>
        /// <typeparam name="TType">The concrete type to register.</typeparam>
        /// <param name="services">The collection to update.</param>
        /// <returns>The supplied collection for fluent configuration.</returns>
        public static IServiceCollection AddServiceType<TType>(this IServiceCollection services)
            where TType : class
            => services.AddServiceType(typeof(TType));

        /// <summary>Registers a client interceptor implementation as a scoped service if absent.</summary>
        /// <typeparam name="TClientInterceptor">The client interceptor implementation.</typeparam>
        /// <param name="services">The collection to update.</param>
        /// <returns>The supplied collection, or <see langword="null"/> if the collection is null.</returns>
        /// <remarks>This helper registers the concrete service type. Client interceptor factory configuration is a separate registration step.</remarks>
        public static IServiceCollection RegisterClientInterceptor<TClientInterceptor>(this IServiceCollection services)
            where TClientInterceptor : class, IStateflowsClientInterceptor
            => services?.AddServiceType<TClientInterceptor>();
    }
}

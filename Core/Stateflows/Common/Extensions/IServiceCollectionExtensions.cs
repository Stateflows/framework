using Microsoft.Extensions.DependencyInjection;

namespace Stateflows.Common.Extensions
{
    internal static class IServiceCollectionExtensions
    {
        public static IServiceCollection RegisterInterceptor<TInterceptor>(this IServiceCollection services)
            where TInterceptor : class, IBehaviorInterceptor
            => services?.AddServiceType<TInterceptor>();
    }
}

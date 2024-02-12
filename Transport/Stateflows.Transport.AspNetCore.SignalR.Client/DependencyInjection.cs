using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Registration.Interfaces;
using Stateflows.Transport.SignalR.Client;

namespace Stateflows
{
    public static class DependencyInjection
    {
        public static IStateflowsClientBuilder AddSignalRTransport(this IStateflowsClientBuilder builder, Func<IServiceProvider, string> baseUriProvider)
        {
            if (baseUriProvider == null)
                throw new ArgumentNullException(nameof(baseUriProvider));

            builder.ServiceCollection
                .AddSingleton<IBehaviorProvider>(provider => new BehaviorProvider(baseUriProvider(provider)))
                ;

            return builder;
        }

        public static IStateflowsClientBuilder AddSignalRTransport(this IStateflowsClientBuilder builder, Func<IServiceProvider, Task<string>> baseUriProviderAsync)
        {
            return builder.AddSignalRTransport(serviceProvider => baseUriProviderAsync(serviceProvider).GetAwaiter().GetResult());
        }
    }
}

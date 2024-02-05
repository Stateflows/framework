using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Registration.Interfaces;
using Stateflows.Transport.Http.Client;

namespace Stateflows
{
    public static class DependencyInjection
    {
        public static IStateflowsClientBuilder AddHttpTransport(this IStateflowsClientBuilder builder, Func<IServiceProvider, Uri> baseUriProvider, Action<IHttpClientBuilder>? clientBuilderAction = null)
        {
            if (baseUriProvider == null)
                throw new ArgumentNullException(nameof(baseUriProvider));


            var clientBuilder = builder.ServiceCollection
                .AddSingleton<IBehaviorProvider>(provider => new BehaviorProvider(provider.GetRequiredService<StateflowsApiClient>()))
                .AddHttpClient<StateflowsApiClient>((provider, client) => client.BaseAddress = baseUriProvider(provider))
                ;

            if (clientBuilder != null)
            {
                clientBuilderAction?.Invoke(clientBuilder);
            }

            return builder;
        }

        public static IStateflowsClientBuilder AddHttpTransport(this IStateflowsClientBuilder builder, Func<IServiceProvider, Task<Uri>> baseUriProviderAsync, Action<IHttpClientBuilder>? clientBuilderAction = null)
        {
            return builder.AddHttpTransport(serviceProvider => baseUriProviderAsync(serviceProvider).GetAwaiter().GetResult(), clientBuilderAction);
        }
    }
}

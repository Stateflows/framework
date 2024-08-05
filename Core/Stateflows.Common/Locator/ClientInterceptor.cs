using System.Threading.Tasks;
using System.Collections.Generic;
using Stateflows.Common.Extensions;
using Microsoft.Extensions.Logging;

namespace Stateflows.Common.Engine
{
    internal class ClientInterceptor : IStateflowsClientInterceptor
    {
        private readonly IEnumerable<IStateflowsClientInterceptor> ClientInterceptors;
        private readonly ILogger Logger;

        public ClientInterceptor(
            IEnumerable<IStateflowsClientInterceptor> clientInterceptors,
            ILogger<ClientInterceptor> logger
        )
        {
            ClientInterceptors = clientInterceptors;
            Logger = logger;
        }

        public Task<bool> BeforeDispatchEventAsync<TEvent>(TEvent @event, List<EventHeader> headers)
            => ClientInterceptors.RunSafe(i => i.BeforeDispatchEventAsync(@event, headers), nameof(BeforeDispatchEventAsync), Logger);

        public Task AfterDispatchEventAsync<TEvent>(TEvent @event)
            => ClientInterceptors.RunSafe(i => i.AfterDispatchEventAsync(@event), nameof(AfterDispatchEventAsync), Logger);
    }
}

using System.Threading.Tasks;
using System.Collections.Generic;
using Stateflows.Common.Extensions;

namespace Stateflows.Common.Engine
{
    internal class ClientInterceptor : IClientInterceptor
    {
        IEnumerable<IClientInterceptor> ClientInterceptors { get; }

        public ClientInterceptor(
            IEnumerable<IClientInterceptor> clientInterceptors
        )
        {
            ClientInterceptors = clientInterceptors;
        }

        public Task<bool> BeforeDispatchEventAsync(Event @event)
            => ClientInterceptors.RunSafe(i => i.BeforeDispatchEventAsync(@event), nameof(BeforeDispatchEventAsync));

        public Task AfterDispatchEventAsync(Event @event)
            => ClientInterceptors.RunSafe(i => i.AfterDispatchEventAsync(@event), nameof(AfterDispatchEventAsync));
    }
}

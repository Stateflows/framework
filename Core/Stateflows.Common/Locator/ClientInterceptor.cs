using System.Threading.Tasks;
using System.Collections.Generic;
using Stateflows.Common.Extensions;

namespace Stateflows.Common.Engine
{
    internal class ClientInterceptor : IStateflowsClientInterceptor
    {
        IEnumerable<IStateflowsClientInterceptor> ClientInterceptors { get; }

        public ClientInterceptor(
            IEnumerable<IStateflowsClientInterceptor> clientInterceptors
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

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

        public bool BeforeDispatchEvent(EventHolder eventHolder)
            => ClientInterceptors.RunSafe(i => i.BeforeDispatchEvent(eventHolder), nameof(BeforeDispatchEvent), Logger);

        public void AfterDispatchEvent(EventHolder eventHolder)
            => ClientInterceptors.RunSafe(i => i.AfterDispatchEvent(eventHolder), nameof(AfterDispatchEvent), Logger);
    }
}

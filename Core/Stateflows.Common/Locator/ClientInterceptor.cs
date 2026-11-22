using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Stateflows.Common.Engine;

namespace Stateflows.Common.Engine
{
    internal class ClientInterceptor
    {
        private static readonly MethodInfo DispatchTypedMethod = typeof(ClientInterceptor)
            .GetMethod(nameof(DispatchTypedAsync), BindingFlags.Instance | BindingFlags.NonPublic)!;
        private readonly IStateflowsClientInterceptor[] clientInterceptors;

        public ClientInterceptor(IEnumerable<IStateflowsClientInterceptor> clientInterceptors)
        {
            this.clientInterceptors = clientInterceptors.ToArray();
        }

        public Task DispatchEventAsync(BehaviorId targetBehaviorId, EventHolder eventHolder, Func<Task> next)
            => (Task)DispatchTypedMethod.MakeGenericMethod(eventHolder.PayloadType).Invoke(this,
                [targetBehaviorId, eventHolder.BoxedPayload, eventHolder.Headers, next])!;

        private Task DispatchTypedAsync<TEvent>(BehaviorId targetBehaviorId, TEvent @event,
            IDictionary<string, EventHeader> headers, Func<Task> next)
            => AsyncAroundPipeline.RunAsync(clientInterceptors, next,
                (interceptor, inner) => interceptor.DispatchEventAsync(targetBehaviorId, @event, headers, inner));
    }
}

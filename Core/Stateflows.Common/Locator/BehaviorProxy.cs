using System.Threading.Tasks;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Classes;
using Stateflows.Common.Engine;

namespace Stateflows.Common.Locator
{
    internal class BehaviorProxy : IBehavior
    {
        private IBehavior Behavior { get; }

        private ClientInterceptor Interceptor { get; }

        public BehaviorId Id => Behavior.Id;

        public BehaviorProxy(IBehavior behavior, ClientInterceptor interceptor)
        {
            Behavior = behavior;
            Interceptor = interceptor;
        }

        public async Task<SendResult> SendAsync<TEvent>(TEvent @event)
            where TEvent : Event, new()
        {
            await Interceptor.BeforeDispatchEventAsync(@event);

            var result = await Behavior.SendAsync(@event);

            await Interceptor.AfterDispatchEventAsync(@event);

            return result;
        }

        public async Task<RequestResult<TResponse>> RequestAsync<TResponse>(Request<TResponse> request)
            where TResponse : Response, new()
        {
            await Interceptor.BeforeDispatchEventAsync(@request);

            var result = await Behavior.RequestAsync(@request);

            await Interceptor.AfterDispatchEventAsync(@request);

            return result;
        }
    }
}

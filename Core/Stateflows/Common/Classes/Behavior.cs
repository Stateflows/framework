using System;
using System.Threading.Tasks;
using Stateflows.Common.Engine;
using Stateflows.Common.Utilities;
using Stateflows.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Stateflows.Common.Classes
{
    internal class Behavior : IBehavior
    {
        public BehaviorId Id { get; }

        private StateflowsEngine Engine { get; }

        private IServiceProvider ServiceProvider { get; }

        private CommonInterceptor CommonInterceptor { get; }

        public Behavior(StateflowsEngine engine, IServiceProvider serviceProvider, BehaviorId id)
        {
            Engine = engine;
            CommonInterceptor = serviceProvider.GetRequiredService<CommonInterceptor>();
            ServiceProvider = serviceProvider;
            Id = id;
        }

        public async Task<SendResult> SendAsync<TEvent>(TEvent @event)
            where TEvent : Event, new()
        {
            var holder = Engine.EnqueueEvent(Id, @event, ServiceProvider);
            await holder.Handled.WaitOneAsync();

            return new SendResult(@event, holder.Status, holder.Validation);
        }

        public async Task<RequestResult<TResponse>> RequestAsync<TResponse>(Request<TResponse> request)
            where TResponse : Response, new()
        {
            var holder = Engine.EnqueueEvent(Id, request, ServiceProvider);
            await holder.Handled.WaitOneAsync();

            return new RequestResult<TResponse>(request, holder.Status, holder.Validation);
        }
    }
}

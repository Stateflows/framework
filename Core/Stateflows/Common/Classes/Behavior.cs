using System.Threading.Tasks;
using Stateflows.Common.Utilities;
using Stateflows.Common.Interfaces;
using System;
using Stateflows.Common.Engine;
using Microsoft.Extensions.Logging;
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

        //public async Task<bool> SendAsync<TEvent>(TEvent @event)
        //    where TEvent : Event
        //{
        //    var holder = Engine.EnqueueEvent(Id, @event);
        //    await holder.Handled.WaitOneAsync();
        //    return holder.Consumed;
        //}

        //public async Task<TResponse> RequestAsync<TResponse>(Request<TResponse> request)
        //    where TResponse : Response
        //{
        //    var holder = Engine.EnqueueEvent(Id, request);
        //    await holder.Handled.WaitOneAsync();
        //    return request.Response;
        //}

        public async Task<SendResult> SendAsync<TEvent>(TEvent @event)
            where TEvent : Event
        {
            //await CommonInterceptor.BeforeDispatchEventAsync(@event);
            var holder = Engine.EnqueueEvent(Id, @event, ServiceProvider);
            //await CommonInterceptor.AfterDispatchEventAsync(@event);
            await holder.Handled.WaitOneAsync();

            return new SendResult(holder.Status, holder.Validation);
        }

        public async Task<RequestResult<TResponse>> RequestAsync<TResponse>(Request<TResponse> request)
            where TResponse : Response
        {
            //await CommonInterceptor.BeforeDispatchEventAsync(request);
            var holder = Engine.EnqueueEvent(Id, request, ServiceProvider);
            //await CommonInterceptor.AfterDispatchEventAsync(request);
            await holder.Handled.WaitOneAsync();

            return new RequestResult<TResponse>(holder.Status, holder.Validation, request.Response);
        }
    }
}

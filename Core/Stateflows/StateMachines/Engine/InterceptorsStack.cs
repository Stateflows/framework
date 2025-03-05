using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Engine
{
    internal class InterceptorsStack : StateMachineInterceptor
    {
        public InterceptorsStack(IStateMachineInterceptor[] interceptors)
        {
            foreach (var interceptor in interceptors)
            {
                Interceptors.Push(interceptor);
            }
        }
        
        private readonly Stack<IStateMachineInterceptor> Interceptors = new Stack<IStateMachineInterceptor>();

        public override async Task<EventStatus> ProcessEventAsync<TEvent>(IEventActionContext<TEvent> context,
            Func<IEventActionContext<TEvent>, Task<EventStatus>> next)
        {
            while (true)
            {
                if (!Interceptors.TryPop(out var interceptor)) return await next(context);

                await interceptor.ProcessEventAsync(context, c => ProcessEventAsync(c, next));
            }
        }
    }
}
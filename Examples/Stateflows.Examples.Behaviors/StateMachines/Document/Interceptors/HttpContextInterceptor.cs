using Stateflows.Common;
using System;
using System.Threading.Tasks;
using Stateflows.Examples.Common.Headers;
using Stateflows.Extensions.MinimalAPIs.Headers;
using Stateflows.StateMachines;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.Examples.Behaviors.StateMachines.Document.Interceptors;

public class HttpContextInterceptor: StateMachineInterceptor
{
    public override async Task<EventStatus> ProcessEventAsync<TEvent>(IEventContext<TEvent> context, Func<Task<EventStatus>> next)
    {
        var httpContextHeader = (HttpContextHeader?)context.Headers.Values.FirstOrDefault(h => h is HttpContextHeader);
        if (httpContextHeader != null && httpContextHeader.Headers.TryGetValue("X-Role", out var role))
        {
            switch (role)
            {
                case "Manager":
                    context.Headers.Add("X-Role", new Manager());
                    break;

                case "Finance":
                    context.Headers.Add("X-Role", new Finance());
                    break;
            }
        }

        return await next();
    }
}
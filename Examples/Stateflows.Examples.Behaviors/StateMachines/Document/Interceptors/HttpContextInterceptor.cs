using Stateflows.Examples.Common.Headers;
using Stateflows.Extensions.MinimalAPIs.Headers;
using Stateflows.StateMachines;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.Examples.Behaviors.StateMachines.Document.Interceptors;

public class HttpContextInterceptor: StateMachineInterceptor
{
    public override bool BeforeProcessEvent<TEvent>(IEventContext<TEvent> context)
    {
        var httpContextHeader = (HttpContextHeader?)context.Headers.FirstOrDefault(h => h is HttpContextHeader);
        if (httpContextHeader != null && httpContextHeader.Context.Request.Headers.TryGetValue("X-Role", out var role))
        {
            switch (role)
            {
                case "Manager":
                    context.Headers.Add(new Manager());
                    break;
                
                case "Finance":
                    context.Headers.Add(new Finance());
                    break;
            }
        }
        
        return base.BeforeProcessEvent(context);
    }
}
using Stateflows.Common;
using Stateflows.Common.Classes;
using Stateflows.StateMachines;

namespace Stateflows.Examples.Behaviors.StateMachines.Document.Interceptors;

public class InfoEnhanceInterceptor : BehaviorInterceptor
{
    public override async Task NotificationPublishedAsync<TNotification>(IBehaviorActionContext context, TNotification notification)
    {
        if (notification is StateMachineInfo stateMachineInfo)
        {
            stateMachineInfo.Metadata.Add("the-answer", await context.Behavior.Values.GetOrDefaultAsync("the-answer", 0));
        }
    }

    public override async Task RequestRespondedAsync<TRequest, TResponse>(IBehaviorActionContext context, TRequest request, TResponse response)
    {
        if (response is StateMachineInfo stateMachineInfo)
        {
            stateMachineInfo.Metadata.Add("the-answer", await context.Behavior.Values.GetOrDefaultAsync("the-answer", 0));
        }
    }
}
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Actions;
using Stateflows.Activities;
using Stateflows.StateMachines;

namespace Stateflows.Common.Classes;

public class BehaviorContextProvider(IServiceProvider serviceProvider) : IBehaviorContextProvider
{
    public async Task<(bool Success, IBehaviorContextHolder ContextHolder)> TryProvideAsync(BehaviorId behaviorId)
    {
        switch (behaviorId.Type)
        {
            case BehaviorType.StateMachine:
            {
                var provider = serviceProvider.GetService<IStateMachineContextProvider>();
                if (provider == null)
                {
                    return (false, null);
                }
                var (success, contextHolder) = await provider.TryProvideAsync(behaviorId);
                return success
                    ? (true, new BehaviorContextHolder(stateMachineContextHolder: contextHolder))
                    : (false, null);
            }
            case BehaviorType.Action:
            {
                var provider = serviceProvider.GetService<IActionContextProvider>();
                if (provider == null)
                {
                    return (false, null);
                }
                var (success, contextHolder) = await provider.TryProvideAsync(behaviorId);
                return success
                    ? (true, new BehaviorContextHolder(actionContextHolder: contextHolder))
                    : (false, null);
            }
            case BehaviorType.Activity:
            {
                var provider = serviceProvider.GetService<IActivityContextProvider>();
                if (provider == null)
                {
                    return (false, null);
                }
                var (success, contextHolder) = await provider.TryProvideAsync(behaviorId);
                return success
                    ? (true, new BehaviorContextHolder(activityContextHolder: contextHolder))
                    : (false, null);
            }

            default:
            {
                return (false, null);
            }
        }
    }
}
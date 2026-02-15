using Stateflows.Common.Extensions;
using Stateflows.StateMachines;

namespace Stateflows.Extensions.MinimalAPIs;

internal class StateMachineConfigurationVisitor(MinimalAPIsBuilder minimalApisBuilder) : StateMachines.StateMachineVisitor
{
    private BehaviorClass? OwnerClass = null;
    
    public override Task StateMachineTypeAddedAsync<TStateMachine>(string stateMachineName, int stateMachineVersion)
    {
        if (OwnerClass != null)
        {
            return Task.CompletedTask;
        }
        
        var stateMachineType = typeof(TStateMachine);
        if (typeof(IStateMachineEndpointsConfiguration).IsAssignableFrom(stateMachineType))
        {
            minimalApisBuilder.CurrentClass = new StateMachineClass(stateMachineName);
            stateMachineType.CallStaticMethod(nameof(IStateMachineEndpointsConfiguration.ConfigureEndpoints), [typeof(IBehaviorClassEndpointsConfiguration)], [minimalApisBuilder]);
            minimalApisBuilder.CurrentClass = null;
        }

        return Task.CompletedTask;
    }

    public override Task StateMachineAddedAsync(string stateMachineName, int stateMachineVersion, BehaviorClass? ownerClass = null, BehaviorClass? parentClass = null, bool hasDefaultInstance = false)
    {
        OwnerClass = ownerClass;
        if (OwnerClass != null)
        {
            minimalApisBuilder.ConfigureStateMachines(b =>
                b.ConfigureStateMachine(
                    stateMachineName,
                    b => b.Disable()
                )
            );
        }
        
        return base.StateMachineAddedAsync(stateMachineName, stateMachineVersion, ownerClass, parentClass, hasDefaultInstance);
    }
}
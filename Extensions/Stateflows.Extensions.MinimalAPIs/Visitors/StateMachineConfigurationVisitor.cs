using Stateflows.Common.Classes;
using Stateflows.Common.Extensions;
using Stateflows.StateMachines;

namespace Stateflows.Extensions.MinimalAPIs;

internal class StateMachineConfigurationVisitor(MinimalAPIsBuilder minimalApisBuilder) : StateMachines.StateMachineVisitor
{
    public override Task StateMachineTypeAddedAsync<TStateMachine>(string stateMachineName, int stateMachineVersion)
    {
        var stateMachineType = typeof(TStateMachine);
        if (typeof(IStateMachineEndpointsConfiguration).IsAssignableFrom(stateMachineType))
        {
            minimalApisBuilder.CurrentClass = new StateMachineClass(stateMachineName);
            stateMachineType.CallStaticMethod(nameof(IStateMachineEndpointsConfiguration.ConfigureEndpoints), [ typeof(IBehaviorClassEndpointsConfiguration) ], [ minimalApisBuilder ]);
            minimalApisBuilder.CurrentClass = null;
        }
        
        return Task.CompletedTask;
    }
}
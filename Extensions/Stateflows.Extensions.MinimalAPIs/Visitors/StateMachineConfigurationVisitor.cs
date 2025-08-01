using Stateflows.Common.Classes;
using Stateflows.StateMachines;

namespace Stateflows.Extensions.MinimalAPIs;

internal class StateMachineConfigurationVisitor(MinimalAPIsBuilder minimalApisBuilder) : StateMachines.StateMachineVisitor
{
    public override Task StateMachineTypeAddedAsync<TStateMachine>(string stateMachineName, int stateMachineVersion)
    {
        if (typeof(IStateMachineEndpointsConfiguration).IsAssignableFrom(typeof(TStateMachine)))
        {
            var stateMachine = (IStateMachineEndpointsConfiguration)StateflowsActivator.CreateUninitializedInstance<TStateMachine>();
            // var stateMachine = (IStateMachineEndpointsConfiguration)StateflowsActivator.CreateModelElementInstanceAsync<TStateMachine>(serviceProvider);
        
            minimalApisBuilder.CurrentClass = new StateMachineClass(stateMachineName);
            stateMachine.ConfigureEndpoints(minimalApisBuilder);
            minimalApisBuilder.CurrentClass = null;
        }
        
        return Task.CompletedTask;
    }
}
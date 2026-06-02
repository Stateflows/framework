using System;
using System.Threading.Tasks;

namespace Stateflows.StateMachines;

public interface IStateMachineProvider
{
    Task ProvideStateMachinesAsync(Action<string> addingCallback);
    
    Task ProvideStatesAsync(StateMachineClass stateMachineClass, int version, string? parentState, Action<string> addingCallback);
    Task ProvideOnEntriesAsync(StateMachineClass stateMachineClass, int version, string state, Action<Delegate> addingCallback);
    Task ProvideOnExitsAsync(StateMachineClass stateMachineClass, int version, string state, Action<Delegate> addingCallback);
    Task ProvideOnInitializesAsync(StateMachineClass stateMachineClass, int version, string state, Action<Delegate> addingCallback);
    Task ProvideOnFinalizesAsync(StateMachineClass stateMachineClass, int version, string state, Action<Delegate> addingCallback);

    Task ProvideTransitionsAsync(StateMachineClass stateMachineClass, int version, string state, Action<(Type?, string?)> addingCallback);
    Task ProvideTransitionGuardsAsync(StateMachineClass stateMachineClass, int version, string state, Type? trigger, string? target, Action<Delegate> addingCallback);
    Task ProvideTransitionEffectsAsync(StateMachineClass stateMachineClass, int version, string state, Type? trigger, string? target, Action<Delegate> addingCallback);

    Task ProvideDeferralsAsync(StateMachineClass stateMachineClass, int version, string state, Action<Type> addingCallback);
    Task ProvideDeferralGuardsAsync(StateMachineClass stateMachineClass, int version, string state, Type trigger, Action<Delegate> addingCallback);
}
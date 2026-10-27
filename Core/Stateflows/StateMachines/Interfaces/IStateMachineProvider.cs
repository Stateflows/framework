using System;
using System.Threading.Tasks;

namespace Stateflows.StateMachines;

/// <summary>Supplies machine definition fragments through callbacks for consumers building or examining machine catalogs.</summary>
/// <remarks>Provider callbacks describe definitions; they do not execute lifecycle or transition behavior.</remarks>
public interface IStateMachineProvider
{
    /// <summary>Enumerates the names of available machine definitions.</summary>
    /// <param name="addingCallback">Receives each machine name.</param>
    /// <returns>A task completing when all names have been supplied.</returns>
    Task ProvideStateMachinesAsync(Action<string> addingCallback);
    
    /// <summary>Enumerates states in a machine or within a specified containing state.</summary>
    /// <param name="stateMachineClass">The machine definition identity.</param>
    /// <param name="version">The definition version.</param>
    /// <param name="parentState">The containing state name, or null for top-level states.</param>
    /// <param name="addingCallback">Receives each state name.</param>
    /// <returns>A task completing when the states have been supplied.</returns>
    Task ProvideStatesAsync(StateMachineClass stateMachineClass, int version, string? parentState, Action<string> addingCallback);
    /// <summary>Supplies the delegates defining a state's entry actions.</summary>
    /// <param name="stateMachineClass">The machine definition identity.</param>
    /// <param name="version">The definition version.</param>
    /// <param name="state">The registered state name.</param>
    /// <param name="addingCallback">Receives each entry-action delegate.</param>
    /// <returns>A task completing when the delegates have been supplied.</returns>
    Task ProvideOnEntriesAsync(StateMachineClass stateMachineClass, int version, string state, Action<Delegate> addingCallback);
    /// <summary>Supplies the delegates defining a state's exit actions.</summary>
    /// <param name="stateMachineClass">The machine definition identity.</param>
    /// <param name="version">The definition version.</param>
    /// <param name="state">The registered state name.</param>
    /// <param name="addingCallback">Receives each exit-action delegate.</param>
    /// <returns>A task completing when the delegates have been supplied.</returns>
    Task ProvideOnExitsAsync(StateMachineClass stateMachineClass, int version, string state, Action<Delegate> addingCallback);
    /// <summary>Supplies the delegates defining initialization of a state's nested configuration.</summary>
    /// <param name="stateMachineClass">The machine definition identity.</param>
    /// <param name="version">The definition version.</param>
    /// <param name="state">The registered state name.</param>
    /// <param name="addingCallback">Receives each initialization delegate.</param>
    /// <returns>A task completing when the delegates have been supplied.</returns>
    Task ProvideOnInitializesAsync(StateMachineClass stateMachineClass, int version, string state, Action<Delegate> addingCallback);
    /// <summary>Supplies the delegates defining finalization of a state's nested configuration.</summary>
    /// <param name="stateMachineClass">The machine definition identity.</param>
    /// <param name="version">The definition version.</param>
    /// <param name="state">The registered state name.</param>
    /// <param name="addingCallback">Receives each finalization delegate.</param>
    /// <returns>A task completing when the delegates have been supplied.</returns>
    Task ProvideOnFinalizesAsync(StateMachineClass stateMachineClass, int version, string state, Action<Delegate> addingCallback);

    /// <summary>Supplies outgoing transition trigger types and target names for a state.</summary>
    /// <param name="stateMachineClass">The machine definition identity.</param>
    /// <param name="version">The definition version.</param>
    /// <param name="state">The registered source state.</param>
    /// <param name="addingCallback">Receives each trigger-type and target-name pair; null components represent unspecified endpoints or triggers.</param>
    /// <returns>A task completing when transition metadata has been supplied.</returns>
    Task ProvideTransitionsAsync(StateMachineClass stateMachineClass, int version, string state, Action<(Type?, string?)> addingCallback);
    /// <summary>Supplies guard delegates for the identified outgoing transition.</summary>
    /// <param name="stateMachineClass">The machine definition identity.</param>
    /// <param name="version">The definition version.</param>
    /// <param name="state">The registered source state.</param>
    /// <param name="trigger">The transition trigger type, if specified.</param>
    /// <param name="target">The destination state name, if specified.</param>
    /// <param name="addingCallback">Receives each guard delegate.</param>
    /// <returns>A task completing when the delegates have been supplied.</returns>
    Task ProvideTransitionGuardsAsync(StateMachineClass stateMachineClass, int version, string state, Type? trigger, string? target, Action<Delegate> addingCallback);
    /// <summary>Supplies effect delegates for the identified outgoing transition.</summary>
    /// <param name="stateMachineClass">The machine definition identity.</param>
    /// <param name="version">The definition version.</param>
    /// <param name="state">The registered source state.</param>
    /// <param name="trigger">The transition trigger type, if specified.</param>
    /// <param name="target">The destination state name, if specified.</param>
    /// <param name="addingCallback">Receives each effect delegate.</param>
    /// <returns>A task completing when the delegates have been supplied.</returns>
    Task ProvideTransitionEffectsAsync(StateMachineClass stateMachineClass, int version, string state, Type? trigger, string? target, Action<Delegate> addingCallback);

    /// <summary>Supplies the event types that a state can defer.</summary>
    /// <param name="stateMachineClass">The machine definition identity.</param>
    /// <param name="version">The definition version.</param>
    /// <param name="state">The registered deferring state.</param>
    /// <param name="addingCallback">Receives each deferred event type.</param>
    /// <returns>A task completing when deferral metadata has been supplied.</returns>
    Task ProvideDeferralsAsync(StateMachineClass stateMachineClass, int version, string state, Action<Type> addingCallback);
    /// <summary>Supplies predicates controlling a state's deferral of the specified event type.</summary>
    /// <param name="stateMachineClass">The machine definition identity.</param>
    /// <param name="version">The definition version.</param>
    /// <param name="state">The registered deferring state.</param>
    /// <param name="trigger">The deferred event type.</param>
    /// <param name="addingCallback">Receives each deferral predicate delegate.</param>
    /// <returns>A task completing when the delegates have been supplied.</returns>
    Task ProvideDeferralGuardsAsync(StateMachineClass stateMachineClass, int version, string state, Type trigger, Action<Delegate> addingCallback);
}
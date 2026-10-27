namespace Stateflows.Actions.Registration.Interfaces;

/// <summary>Configures a delegate-based action registration.</summary>
/// <param name="builder">The action configuration builder.</param>
public delegate void ActionBuildAction(IActionBuilder builder);

/// <summary>Configures a typed action registration and its implementation instance.</summary>
/// <typeparam name="TAction">The action implementation type.</typeparam>
/// <param name="builder">The typed action configuration builder.</param>
public delegate void ActionBuildAction<in TAction>(IActionBuilder<TAction> builder)
    where TAction : class, IAction;
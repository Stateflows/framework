namespace Stateflows.Actions.Registration.Interfaces;

public delegate void ActionBuildAction(IActionBuilder builder);

public delegate void ActionBuildAction<in TAction>(IActionBuilder<TAction> builder)
    where TAction : class, IAction;
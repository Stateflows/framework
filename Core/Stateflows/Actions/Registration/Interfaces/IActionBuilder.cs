using Stateflows.Actions.Registration.Interfaces.Base;
using Stateflows.Common.Interfaces;

namespace Stateflows.Actions.Registration.Interfaces;

/// <summary>Fluent options available while registering a delegate-based action.</summary>
public interface IActionBuilder :
    IActionUtils<IActionBuilder>,
    IActionObservability<IActionBuilder>;

/// <summary>Fluent options available while registering and configuring a typed action.</summary>
/// <typeparam name="TAction">The typed action implementation.</typeparam>
public interface IActionBuilder<out TAction> :
    IActionUtils<IActionBuilder<TAction>>,
    IActionObservability<IActionBuilder<TAction>>,
    IElementBuilderBase<TAction, IActionBuilder<TAction>>
    where TAction : class, IAction;

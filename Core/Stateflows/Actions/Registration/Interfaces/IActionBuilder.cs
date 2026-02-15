using Stateflows.Actions.Registration.Interfaces.Base;
using Stateflows.Common.Interfaces;

namespace Stateflows.Actions.Registration.Interfaces;

public interface IActionBuilder :
    IActionUtils<IActionBuilder>,
    IActionObservability<IActionBuilder>;

public interface IActionBuilder<out TAction> :
    IActionBuilder,
    IActionUtils<IActionBuilder<TAction>>,
    IActionObservability<IActionBuilder<TAction>>,
    IElementBuilderBase<TAction, IActionBuilder<TAction>>
    where TAction : class, IAction;

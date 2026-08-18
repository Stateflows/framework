using Stateflows.Activities.Registration.Interfaces.Base;
using Stateflows.Common.Interfaces;

namespace Stateflows.Activities.Registration.Interfaces;

public interface IActionBuilder :
    IObjectFlowBase<IActionBuilder>,
    IControlFlowBase<IActionBuilder>,
    IExceptionHandlerBase<IActionBuilder>,
    INodeOptions<IActionBuilder>;

public interface ITypedActionBuilder<out TAction> :
    IObjectFlowBase<ITypedActionBuilder<TAction>>,
    IControlFlowBase<ITypedActionBuilder<TAction>>,
    IExceptionHandlerBase<ITypedActionBuilder<TAction>>,
    IElementBuilderBase<TAction, ITypedActionBuilder<TAction>>,
    INodeOptions<ITypedActionBuilder<TAction>>
    where TAction : class, IActionNode;

public interface IOverridenActionBuilder :
    IObjectFlowBase<IOverridenActionBuilder>,
    IControlFlowBase<IOverridenActionBuilder>,
    IExceptionHandlerBase<IOverridenActionBuilder>,
    INodeOptions<IOverridenActionBuilder>;

public interface IOverridenTypedActionBuilder<out TAction> :
    IObjectFlowBase<IOverridenTypedActionBuilder<TAction>>,
    IControlFlowBase<IOverridenTypedActionBuilder<TAction>>,
    IExceptionHandlerBase<IOverridenTypedActionBuilder<TAction>>,
    IElementBuilderBase<TAction, IOverridenTypedActionBuilder<TAction>>,
    INodeOptions<IOverridenTypedActionBuilder<TAction>>
    where TAction : class, IActionNode;

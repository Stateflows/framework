using Stateflows.Activities.Registration.Interfaces.Base;
using Stateflows.Common.Interfaces;

namespace Stateflows.Activities.Registration.Interfaces;

public interface IActionBuilder :
    IObjectFlowBase<IActionBuilder>,
    IControlFlowBase<IActionBuilder>,
    IExceptionHandlerBase<IActionBuilder>,
    INodeOptions<IActionBuilderWithOptions>;

public interface IActionBuilderWithOptions : 
    IObjectFlowBase<IActionBuilderWithOptions>,
    IControlFlowBase<IActionBuilderWithOptions>,
    IExceptionHandlerBase<IActionBuilderWithOptions>;

public interface ITypedActionBuilder<out TAction> :
    IObjectFlowBase<ITypedActionBuilder<TAction>>,
    IControlFlowBase<ITypedActionBuilder<TAction>>,
    IExceptionHandlerBase<ITypedActionBuilder<TAction>>,
    IElementBuilderBase<TAction, ITypedActionBuilder<TAction>>
    where TAction : class, IActionNode;

using Stateflows.Activities.Registration.Interfaces.Base;
using Stateflows.Common.Interfaces;

namespace Stateflows.Activities.Registration.Interfaces;

public interface ITimeEventActionBuilder : 
    IObjectFlowBase<ITimeEventActionBuilder>,
    IControlFlowBase<ITimeEventActionBuilder>,
    IExceptionHandlerBase<ITimeEventActionBuilder>;

public interface ITimeEventActionBuilder<out TTimeEventAction> :
    IObjectFlowBase<ITimeEventActionBuilder<TTimeEventAction>>,
    IControlFlowBase<ITimeEventActionBuilder<TTimeEventAction>>,
    IExceptionHandlerBase<ITimeEventActionBuilder<TTimeEventAction>>,
    IElementBuilderBase<TTimeEventAction, ITimeEventActionBuilder<TTimeEventAction>>
    where TTimeEventAction : class, ITimeEventActionNode;

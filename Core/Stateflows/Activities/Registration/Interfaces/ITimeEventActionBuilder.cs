using Stateflows.Activities.Registration.Interfaces.Base;
using Stateflows.Common.Interfaces;

namespace Stateflows.Activities.Registration.Interfaces;

public interface ITimeEventActionBuilder : 
    IObjectFlowBase<ITimeEventActionBuilder>,
    IControlFlowBase<ITimeEventActionBuilder>,
    IExceptionHandlerBase<ITimeEventActionBuilder>;

public interface IOverridenTimeEventActionBuilder : 
    IObjectFlowBase<IOverridenTimeEventActionBuilder>,
    IOverridenObjectFlowBase<IOverridenTimeEventActionBuilder>,
    IControlFlowBase<IOverridenTimeEventActionBuilder>,
    IOverridenControlFlowBase<IOverridenTimeEventActionBuilder>,
    IExceptionHandlerBase<IOverridenTimeEventActionBuilder>;

public interface ITimeEventActionBuilder<out TTimeEventAction> :
    IObjectFlowBase<ITimeEventActionBuilder<TTimeEventAction>>,
    IControlFlowBase<ITimeEventActionBuilder<TTimeEventAction>>,
    IExceptionHandlerBase<ITimeEventActionBuilder<TTimeEventAction>>,
    IElementBuilderBase<TTimeEventAction, ITimeEventActionBuilder<TTimeEventAction>>
    where TTimeEventAction : class, ITimeEventActionNode;

public interface IOverridenTimeEventActionBuilder<out TTimeEventAction> :
    IObjectFlowBase<IOverridenTimeEventActionBuilder<TTimeEventAction>>,
    IOverridenObjectFlowBase<IOverridenTimeEventActionBuilder<TTimeEventAction>>,
    IControlFlowBase<IOverridenTimeEventActionBuilder<TTimeEventAction>>,
    IOverridenControlFlowBase<IOverridenTimeEventActionBuilder<TTimeEventAction>>,
    IExceptionHandlerBase<IOverridenTimeEventActionBuilder<TTimeEventAction>>,
    IElementBuilderBase<TTimeEventAction, IOverridenTimeEventActionBuilder<TTimeEventAction>>
    where TTimeEventAction : class, ITimeEventActionNode;

using Stateflows.Activities.Registration.Interfaces.Base;
using Stateflows.Common.Interfaces;

namespace Stateflows.Activities.Registration.Interfaces;

public interface IAcceptEventActionBuilder :
    IObjectFlowBase<IAcceptEventActionBuilder>,
    IControlFlowBase<IAcceptEventActionBuilder>,
    IExceptionHandlerBase<IAcceptEventActionBuilder>;

public interface IAcceptEventActionBuilder<in TEvent, out TAcceptEventAction> :
    IObjectFlowBase<IAcceptEventActionBuilder<TEvent, TAcceptEventAction>>,
    IControlFlowBase<IAcceptEventActionBuilder<TEvent, TAcceptEventAction>>,
    IExceptionHandlerBase<IAcceptEventActionBuilder<TEvent, TAcceptEventAction>>,
    IElementBuilderBase<TAcceptEventAction, IAcceptEventActionBuilder<TEvent, TAcceptEventAction>>
    where TAcceptEventAction : class, IAcceptEventActionNode<TEvent>;
using Stateflows.Activities.Registration.Interfaces.Base;
using Stateflows.Common.Interfaces;

namespace Stateflows.Activities.Registration.Interfaces;

public interface IAcceptEventActionBuilder<TEvent> :
    IObjectFlowBase<IAcceptEventActionBuilder<TEvent>>,
    IControlFlowBase<IAcceptEventActionBuilder<TEvent>>,
    IExceptionHandlerBase<IAcceptEventActionBuilder<TEvent>>;

public interface IOverridenAcceptEventActionBuilder<in TEvent> :
    IObjectFlowBase<IOverridenAcceptEventActionBuilder<TEvent>>,
    IOverridenObjectFlowBase<IOverridenAcceptEventActionBuilder<TEvent>>,
    IControlFlowBase<IOverridenAcceptEventActionBuilder<TEvent>>,
    IOverridenControlFlowBase<IOverridenAcceptEventActionBuilder<TEvent>>,
    IExceptionHandlerBase<IOverridenAcceptEventActionBuilder<TEvent>>
{
    IOverridenAcceptEventActionBuilder<TAcceptedEvent> ChangeAcceptedEvent<TAcceptedEvent>()
        where TAcceptedEvent : TEvent
    {
        return null;
        // var builder = (TransitionBuilder<TEvent>)this;
        // builder.Edge.TriggerType = typeof(TTrigger);
        // builder.Edge.Trigger = typeof(TTrigger).GetEventName();
        //     
        // return new TransitionBuilder<TTrigger>(builder.Edge);
    }
}

public interface IAcceptEventActionBuilder<in TEvent, out TAcceptEventAction> :
    IObjectFlowBase<IAcceptEventActionBuilder<TEvent, TAcceptEventAction>>,
    IControlFlowBase<IAcceptEventActionBuilder<TEvent, TAcceptEventAction>>,
    IExceptionHandlerBase<IAcceptEventActionBuilder<TEvent, TAcceptEventAction>>,
    IElementBuilderBase<TAcceptEventAction, IAcceptEventActionBuilder<TEvent, TAcceptEventAction>>
    where TAcceptEventAction : class, IAcceptEventActionNode<TEvent>;

public interface IOverridenAcceptEventActionBuilder<in TEvent, out TAcceptEventAction> :
    IObjectFlowBase<IOverridenAcceptEventActionBuilder<TEvent, TAcceptEventAction>>,
    IOverridenObjectFlowBase<IOverridenAcceptEventActionBuilder<TEvent, TAcceptEventAction>>,
    IControlFlowBase<IOverridenAcceptEventActionBuilder<TEvent, TAcceptEventAction>>,
    IOverridenControlFlowBase<IOverridenAcceptEventActionBuilder<TEvent, TAcceptEventAction>>,
    IExceptionHandlerBase<IOverridenAcceptEventActionBuilder<TEvent, TAcceptEventAction>>,
    IElementBuilderBase<TAcceptEventAction, IOverridenAcceptEventActionBuilder<TEvent, TAcceptEventAction>>
    where TAcceptEventAction : class, IAcceptEventActionNode<TEvent>
{
    IOverridenAcceptEventActionBuilder<TEvent, TAcceptEventAction> ChangeAcceptedEvent<TAcceptedEvent>()
        where TAcceptedEvent : TEvent
    {
        return null;
        // var builder = (TransitionBuilder<TEvent>)this;
        // builder.Edge.TriggerType = typeof(TTrigger);
        // builder.Edge.Trigger = typeof(TTrigger).GetEventName();
        //     
        // return new TransitionBuilder<TTrigger>(builder.Edge);
    }
}
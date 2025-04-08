using Stateflows.Common;
using Stateflows.StateMachines.Registration.Builders;
using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    public interface ITransitionBuilder<TEvent> :
        ITransitionUtils<ITransitionBuilder<TEvent>>,
        IEffect<TEvent, ITransitionBuilder<TEvent>>,
        IGuard<TEvent, ITransitionBuilder<TEvent>>
    { }


    public interface IOverridenTransitionBuilder<TEvent> :
        ITransitionUtils<IOverridenTransitionBuilder<TEvent>>,
        IEffect<TEvent, IOverridenTransitionBuilder<TEvent>>,
        IGuard<TEvent, IOverridenTransitionBuilder<TEvent>>
    {
        IOverridenTransitionBuilder<TTrigger> ChangeTrigger<TTrigger>()
            where TTrigger : TEvent
        {
            var builder = (TransitionBuilder<TEvent>)this;
            builder.Edge.TriggerType = typeof(TTrigger);
            builder.Edge.Trigger = typeof(TTrigger).GetEventName();
            
            return new TransitionBuilder<TTrigger>(builder.Edge, builder.Services);
        }
    }
}

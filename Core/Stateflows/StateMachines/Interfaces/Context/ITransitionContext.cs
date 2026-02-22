using System;

namespace Stateflows.StateMachines
{
    public interface ITransitionContext
    {
        IStateContext Source { get; }

        IStateContext Target { get; }
        
        Type TriggerType { get; }
        
        object Trigger { get; }
    }
    
    public interface ITransitionContextX<TEvent> : ITransitionContext
    {
        IStateContext Source { get; }

        IStateContext Target { get; }
        
        Type TriggerType { get; }

        new TEvent Trigger { get; }
    }
}

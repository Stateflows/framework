using System.Threading.Tasks;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines
{
    public abstract class StateMachineObserver : IStateMachineObserver
    {
        public virtual void BeforeStateMachineInitialize(IStateMachineInitializationContext context, bool implicitInitialization)
        { }

        public virtual void AfterStateMachineInitialize(IStateMachineInitializationContext context, bool implicitInitialization, bool initialized)
        { }

        public virtual void BeforeStateMachineFinalize(IStateMachineActionContext context)
        { }

        public virtual void AfterStateMachineFinalize(IStateMachineActionContext context)
        { }

        public virtual void BeforeStateInitialize(IStateActionContext context)
        { }

        public virtual void AfterStateInitialize(IStateActionContext context)
        { }

        public virtual void BeforeStateFinalize(IStateActionContext context)
        { }

        public virtual void AfterStateFinalize(IStateActionContext context)
        { }

        public virtual void BeforeStateEntry(IStateActionContext context)
        { }

        public virtual void AfterStateEntry(IStateActionContext context)
        { }

        public virtual void BeforeStateExit(IStateActionContext context)
        { }

        public virtual void AfterStateExit(IStateActionContext context)
        { }

        public virtual void BeforeTransitionGuard<TEvent>(ITransitionContext<TEvent> context)
        { }

        public virtual void AfterTransitionGuard<TEvent>(ITransitionContext<TEvent> context, bool guardResult)
        { }

        public virtual void BeforeTransitionEffect<TEvent>(ITransitionContext<TEvent> context)
        { }

        public virtual void AfterTransitionEffect<TEvent>(ITransitionContext<TEvent> context)
        { }
    }
}
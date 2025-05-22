using System;
using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines
{
    internal abstract class StateMachinePlugin : IStateMachinePlugin
    {
        public virtual void AfterHydrate(IStateMachineActionContext context)
        { }

        public virtual void BeforeDehydrate(IStateMachineActionContext context)
        { }

        public virtual bool BeforeProcessEvent<TEvent>(IEventContext<TEvent> context)
            => true;

        public virtual void AfterProcessEvent<TEvent>(IEventContext<TEvent> context, EventStatus eventStatus)
        { }

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

        public virtual bool OnStateMachineInitializationException(IStateMachineInitializationContext context, Exception exception)
            => false;

        public virtual bool OnStateMachineFinalizationException(IStateMachineActionContext context, Exception exception)
            => false;

        public virtual bool OnTransitionGuardException<TEvent>(ITransitionContext<TEvent> context, Exception exception)
            => false;

        public virtual bool OnTransitionEffectException<TEvent>(ITransitionContext<TEvent> context, Exception exception)
            => false;

        public virtual bool OnStateInitializationException(IStateActionContext context, Exception exception)
            => false;

        public virtual bool OnStateFinalizationException(IStateActionContext context, Exception exception)
            => false;

        public virtual bool OnStateEntryException(IStateActionContext context, Exception exception)
            => false;

        public virtual bool OnStateExitException(IStateActionContext context, Exception exception)
            => false;
    }
}
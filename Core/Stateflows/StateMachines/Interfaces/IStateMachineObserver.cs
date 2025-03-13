using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines
{
    public interface IStateMachineObserver
    {
        void BeforeStateMachineInitialize(IStateMachineInitializationContext context, bool implicitInitialization);

        void AfterStateMachineInitialize(IStateMachineInitializationContext context, bool initialized);

        void BeforeStateMachineFinalize(IStateMachineActionContext context);

        void AfterStateMachineFinalize(IStateMachineActionContext context);

        void BeforeStateInitialize(IStateActionContext context);

        void AfterStateInitialize(IStateActionContext context);

        void BeforeStateFinalize(IStateActionContext context);

        void AfterStateFinalize(IStateActionContext context);

        void BeforeStateEntry(IStateActionContext context);

        void AfterStateEntry(IStateActionContext context);

        void BeforeStateExit(IStateActionContext context);

        void AfterStateExit(IStateActionContext context);

        void BeforeTransitionGuard<TEvent>(ITransitionContext<TEvent> context);

        void AfterTransitionGuard<TEvent>(ITransitionContext<TEvent> context, bool guardResult);

        void BeforeTransitionEffect<TEvent>(ITransitionContext<TEvent> context);

        void AfterTransitionEffect<TEvent>(ITransitionContext<TEvent> context);
    }
}

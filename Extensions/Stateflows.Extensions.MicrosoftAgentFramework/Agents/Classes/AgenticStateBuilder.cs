using Stateflows.MAF.AIAgents;
using Stateflows.StateMachines;
using Stateflows.Common;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.Extensions.MicrosoftAgentFramework.Agents.Classes;

internal class AgenticStateBuilder(IBehaviorStateBuilder behaviorStateBuilder) : IAgenticStateBuilder
{
    public IBehaviorStateBuilder AddOnEntry(params Func<IStateActionContext, Task>[] actionsAsync)
        => behaviorStateBuilder.AddOnEntry(actionsAsync);

    public IBehaviorStateBuilder AddOnExit(params Func<IStateActionContext, Task>[] actionsAsync)
        => behaviorStateBuilder.AddOnExit(actionsAsync);

    public IBehaviorStateBuilder AddDeferredEvent<TEvent>(DeferralBuildAction<TEvent> buildAction = null)
        => behaviorStateBuilder.AddDeferredEvent<TEvent>(buildAction);

    IBehaviorStateBuilder IStateUtils<IBehaviorStateBuilder>.AddDeferredEvent(string eventName, DeferralBuildAction<Payload> buildAction)
        => behaviorStateBuilder.AddDeferredEvent(eventName, buildAction);

    IBehaviorStateBuilder IStateUtils<IBehaviorStateBuilder>.AddDeferredEvent(IReadOnlyList<string> eventNames, DeferralBuildAction<Payload> buildAction)
        => behaviorStateBuilder.AddDeferredEvent(eventNames, buildAction);

    IBehaviorStateBuilder IStateTransitions<IBehaviorStateBuilder>.AddTransition(string eventName, string targetStateName, TransitionBuildAction<Payload> transitionBuildAction)
        => behaviorStateBuilder.AddTransition(eventName, targetStateName, transitionBuildAction);

    IBehaviorStateBuilder IStateTransitions<IBehaviorStateBuilder>.AddTransition(IReadOnlyList<string> eventNames, string targetStateName, TransitionBuildAction<Payload> transitionBuildAction)
        => behaviorStateBuilder.AddTransition(eventNames, targetStateName, transitionBuildAction);

    IBehaviorStateBuilder IStateTransitions<IBehaviorStateBuilder>.AddInternalTransition(string eventName, InternalTransitionBuildAction<Payload> transitionBuildAction)
        => behaviorStateBuilder.AddInternalTransition(eventName, transitionBuildAction);

    IBehaviorStateBuilder IStateTransitions<IBehaviorStateBuilder>.AddInternalTransition(IReadOnlyList<string> eventNames, InternalTransitionBuildAction<Payload> transitionBuildAction)
        => behaviorStateBuilder.AddInternalTransition(eventNames, transitionBuildAction);

    IBehaviorStateBuilder IStateTransitions<IBehaviorStateBuilder>.AddElseTransition(string eventName, string targetStateName, ElseTransitionBuildAction<Payload> transitionBuildAction)
        => behaviorStateBuilder.AddElseTransition(eventName, targetStateName, transitionBuildAction);

    IBehaviorStateBuilder IStateTransitions<IBehaviorStateBuilder>.AddElseTransition(IReadOnlyList<string> eventNames, string targetStateName, ElseTransitionBuildAction<Payload> transitionBuildAction)
        => behaviorStateBuilder.AddElseTransition(eventNames, targetStateName, transitionBuildAction);

    IBehaviorStateBuilder IStateTransitions<IBehaviorStateBuilder>.AddElseInternalTransition(string eventName, ElseInternalTransitionBuildAction<Payload> transitionBuildAction)
        => behaviorStateBuilder.AddElseInternalTransition(eventName, transitionBuildAction);

    IBehaviorStateBuilder IStateTransitions<IBehaviorStateBuilder>.AddElseInternalTransition(IReadOnlyList<string> eventNames, ElseInternalTransitionBuildAction<Payload> transitionBuildAction)
        => behaviorStateBuilder.AddElseInternalTransition(eventNames, transitionBuildAction);

    public IBehaviorStateBuilder AddTransition<TEvent>(string targetStateName, TransitionBuildAction<TEvent> transitionBuildAction = null)
        => behaviorStateBuilder.AddTransition<TEvent>(targetStateName, transitionBuildAction);

    public IBehaviorStateBuilder AddDefaultTransition(string targetStateName, DefaultTransitionBuildAction transitionBuildAction = null)
        => behaviorStateBuilder.AddDefaultTransition(targetStateName, transitionBuildAction);

    public IBehaviorStateBuilder AddInternalTransition<TEvent>(InternalTransitionBuildAction<TEvent> transitionBuildAction)
        => behaviorStateBuilder.AddInternalTransition<TEvent>(transitionBuildAction);

    public IBehaviorStateBuilder AddElseTransition<TEvent>(string targetStateName, ElseTransitionBuildAction<TEvent> transitionBuildAction = null)
        => behaviorStateBuilder.AddElseTransition<TEvent>(targetStateName, transitionBuildAction);

    public IBehaviorStateBuilder AddElseDefaultTransition(string targetStateName, ElseDefaultTransitionBuildAction transitionBuildAction = null)
        => behaviorStateBuilder.AddElseDefaultTransition(targetStateName, transitionBuildAction);

    public IBehaviorStateBuilder AddElseInternalTransition<TEvent>(ElseInternalTransitionBuildAction<TEvent> transitionBuildAction)
        => behaviorStateBuilder.AddElseInternalTransition<TEvent>(transitionBuildAction);
}
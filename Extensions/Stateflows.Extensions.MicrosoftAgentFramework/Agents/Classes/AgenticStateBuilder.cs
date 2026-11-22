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

    IBehaviorStateBuilder IStateUtils<IBehaviorStateBuilder>.AddDeferredPayloadEvent(IReadOnlyList<string> eventNames, DeferralBuildAction<Payload> buildAction)
        => behaviorStateBuilder.AddDeferredPayloadEvent(eventNames, buildAction);

    IBehaviorStateBuilder IStateTransitions<IBehaviorStateBuilder>.AddPayloadTransition(IReadOnlyList<string> eventNames, string targetStateName, TransitionBuildAction<Payload> transitionBuildAction)
        => behaviorStateBuilder.AddPayloadTransition(eventNames, targetStateName, transitionBuildAction);

    IBehaviorStateBuilder IStateTransitions<IBehaviorStateBuilder>.AddInternalPayloadTransition(IReadOnlyList<string> eventNames, InternalTransitionBuildAction<Payload> transitionBuildAction)
        => behaviorStateBuilder.AddInternalPayloadTransition(eventNames, transitionBuildAction);

    IBehaviorStateBuilder IStateTransitions<IBehaviorStateBuilder>.AddElsePayloadTransition(IReadOnlyList<string> eventNames, string targetStateName, ElseTransitionBuildAction<Payload> transitionBuildAction)
        => behaviorStateBuilder.AddElsePayloadTransition(eventNames, targetStateName, transitionBuildAction);

    IBehaviorStateBuilder IStateTransitions<IBehaviorStateBuilder>.AddElseInternalPayloadTransition(IReadOnlyList<string> eventNames, ElseInternalTransitionBuildAction<Payload> transitionBuildAction)
        => behaviorStateBuilder.AddElseInternalPayloadTransition(eventNames, transitionBuildAction);

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
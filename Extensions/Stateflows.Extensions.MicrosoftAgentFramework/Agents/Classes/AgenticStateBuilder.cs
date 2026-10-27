using Stateflows.MAF.AIAgents;
using Stateflows.StateMachines;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.Extensions.MicrosoftAgentFramework.Agents.Classes;

internal class AgenticStateBuilder(IBehaviorStateBuilder behaviorStateBuilder) : IAgenticStateBuilder
{
    public IBehaviorStateBuilder AddOnEntry(params Func<IStateActionContext, Task>[] actionsAsync)
        => behaviorStateBuilder.AddOnEntry(actionsAsync);

    public IBehaviorStateBuilder AddOnExit(params Func<IStateActionContext, Task>[] actionsAsync)
        => behaviorStateBuilder.AddOnExit(actionsAsync);

    public IBehaviorStateBuilder AddDeferredEvent<TEvent>(DeferralBuildAction<TEvent> buildAction = null)
        => behaviorStateBuilder.AddDeferredEvent<TEvent>(buildAction);

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
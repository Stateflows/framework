using Stateflows.Activities;
using Stateflows.Common.Interfaces;
using Stateflows.MAF.AIAgents.Classes;
using Stateflows.MAF.AIAgents.Registration;

namespace Stateflows.MAF.AIAgents.Extensions;

public static class ActivityBuilderExtensions
{
    public static IActivityBuilder AddAgenticAction(this IActivityBuilder activityBuilder, string agenticActionNodeName, AIAgentFactory aiAgentFactory, AgentBuildAction? buildAction = null)
        => AddAgenticAction(activityBuilder, agenticActionNodeName, sp => Task.FromResult(aiAgentFactory(sp)), buildAction);
    
    public static IActivityBuilder AddAgenticAction(this IActivityBuilder activityBuilder, AIAgentFactory aiAgentFactory, AgentBuildAction? buildAction = null)
        => AddAgenticAction(activityBuilder, ActivityNode<AIAgentActionNode>.Name, aiAgentFactory, buildAction);

    public static IActivityBuilder AddAgenticAction(this IActivityBuilder activityBuilder,
        string agenticActionNodeName, AIAgentFactoryAsync aiAgentFactoryAsync, AgentBuildAction? buildAction = null)
        => activityBuilder.AddStructuredActivity(agenticActionNodeName, b => b
            .AddInitial(b => b
                .AddControlFlow<AIAgentActionNode>()
            )
            .AddAcceptEventAction<string>(c => { 
                    c.Output(c.Event);
                    return Task.CompletedTask;
                }, b => b
                .AddFlow<string, AIAgentActionNode>(b => b.SetWeight(0))
            )
            .AddAcceptEventAction<AgenticChatMessage>(c => { 
                    c.Output(c.Event);
                    return Task.CompletedTask;
                }, b => b
                .AddFlow<AgenticChatMessage, AIAgentActionNode>(b => b.SetWeight(0))
            )
            .AddAction<AIAgentActionNode>(b => b
                .AddConfiguration(aiAgentFactoryAsync)
            )
        );
    
    public static IActivityBuilder AddAgenticAction(this IActivityBuilder activityBuilder, AIAgentFactoryAsync aiAgentFactoryAsync, AgentBuildAction? buildAction = null)
        => AddAgenticAction(activityBuilder, ActivityNode<AIAgentActionNode>.Name, aiAgentFactoryAsync, buildAction);
    
    public static IActivityBuilder AddAgenticAction<TAgent>(this IActivityBuilder activityBuilder, string agenticActionNodeName, AgentBuildAction? buildAction = null)
        where TAgent : class, IAIAgent
        => activityBuilder.AddStructuredActivity(agenticActionNodeName, b => b
            .AddInitial(b => b
                .AddControlFlow<AIAgentActionNode<TAgent>>()
            )
            .AddInput(b => b
                .AddFlow<string, AIAgentActionNode<TAgent>>(b => b.SetWeight(0))
                .AddFlow<AgenticChatMessage, AIAgentActionNode<TAgent>>(b => b.SetWeight(0))
            )
            .AddAcceptEventAction<string>(c => {
                    c.Output(c.Event);
                    return Task.CompletedTask;
                }, b => b
                .AddFlow<string, AIAgentActionNode<TAgent>>(b => b.SetWeight(0))
            )
            .AddAcceptEventAction<AgenticChatMessage>(c =>
                {
                    c.Output(c.Event);
                    return Task.CompletedTask;
                }, b => b
                .AddFlow<AgenticChatMessage, AIAgentActionNode<TAgent>>(b => b.SetWeight(0))
            )
            .AddAction<AIAgentActionNode<TAgent>>()
        );

    public static IActivityBuilder AddAgenticAction<TAgent>(this IActivityBuilder activityBuilder, AgentBuildAction? buildAction = null)
        where TAgent : class, IAIAgent
        => AddAgenticAction<TAgent>(activityBuilder, ActivityNode<AIAgentActionNode<TAgent>>.Name, buildAction);

    // public static IActivityBuilder AddAgenticTransition(this IActivityBuilder activityBuilder, string targetStateName, DefaultTransitionBuildAction? transitionBuildAction = null)
    //     => activityBuilder.AddDefaultTransition(targetStateName, transitionBuildAction);
    //
    // public static IActivityBuilder AddAgenticTransition<TTargetState>(this IActivityBuilder activityBuilder, DefaultTransitionBuildAction? transitionBuildAction = null)
    //     where TTargetState : class, IVertex
    //     => activityBuilder.AddDefaultTransition<TTargetState>(transitionBuildAction);
    //
    // public static IBehaviorOverridenStateBuilder AddAgenticTransition(this IBehaviorOverridenStateBuilder activityBuilder, string targetStateName, DefaultTransitionBuildAction? transitionBuildAction = null)
    //     => activityBuilder.AddDefaultTransition(targetStateName, transitionBuildAction);
    //
    // public static IBehaviorOverridenStateBuilder AddAgenticTransition<TTargetState>(this IBehaviorOverridenStateBuilder activityBuilder, DefaultTransitionBuildAction? transitionBuildAction = null)
    //     where TTargetState : class, IVertex
    //     => activityBuilder.AddDefaultTransition<TTargetState>(transitionBuildAction);
}
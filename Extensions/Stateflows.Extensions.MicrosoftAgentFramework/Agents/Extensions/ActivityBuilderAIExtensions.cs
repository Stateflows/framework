using Stateflows.Activities;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.Common.Interfaces;
using Stateflows.MAF.AIAgents.Classes;
using Stateflows.MAF.AIAgents.Registration;

namespace Stateflows.MAF.AIAgents.Extensions;

public static class ActivityBuilderAIExtensions
{
    public static IActivityBuilder AddAIAgentAction(this IActivityBuilder activityBuilder, string aiAgentActionNodeName, AIAgentFactory aiAgentFactory, ActionBuildAction? buildAction = null)
        => AddAIAgentAction(activityBuilder, aiAgentActionNodeName, (sp, tools) => Task.FromResult(aiAgentFactory(sp, tools)), buildAction);
    
    public static IActivityBuilder AddAIAgentAction(this IActivityBuilder activityBuilder, AIAgentFactory aiAgentFactory, ActionBuildAction? buildAction = null)
        => AddAIAgentAction(activityBuilder, ActivityNode<AIAgentActionNode>.Name, aiAgentFactory, buildAction);

    public static IActivityBuilder AddAIAgentAction(this IActivityBuilder activityBuilder,
        string aiAgentActionNodeName, AIAgentFactoryAsync aiAgentFactoryAsync, ActionBuildAction? buildAction = null)
        => activityBuilder.AddAction<AIAgentActionNode>(b =>
        {
            b.AddConfiguration(aiAgentFactoryAsync);
            
            buildAction?.Invoke(b as IActionBuilder);
        });
    
    public static IActivityBuilder AddAIAgentAction(this IActivityBuilder activityBuilder, AIAgentFactoryAsync aiAgentFactoryAsync, ActionBuildAction? buildAction = null)
        => AddAIAgentAction(activityBuilder, ActivityNode<AIAgentActionNode>.Name, aiAgentFactoryAsync, buildAction);
    
    public static IActivityBuilder AddAIAgentAction<TAIAgent>(this IActivityBuilder activityBuilder, string aiAgentActionNodeName, ActionBuildAction? buildAction = null)
        where TAIAgent : class, IAIAgent
        => activityBuilder.AddAction<AIAgentActionNode<TAIAgent>>(b =>
        {
            buildAction?.Invoke(b as IActionBuilder);
        });

    public static IActivityBuilder AddAIAgentAction<TAIAgent>(this IActivityBuilder activityBuilder, ActionBuildAction? buildAction = null)
        where TAIAgent : class, IAIAgent
        => AddAIAgentAction<TAIAgent>(activityBuilder, ActivityNode<AIAgentActionNode<TAIAgent>>.Name, buildAction);
    
    
    
    
    public static IOverridenActivityBuilder AddAIAgentAction(this IOverridenActivityBuilder activityBuilder, string aiAgentActionNodeName, AIAgentFactory aiAgentFactory, ActionBuildAction? buildAction = null)
        => AddAIAgentAction(activityBuilder, aiAgentActionNodeName, (sp, tools) => Task.FromResult(aiAgentFactory(sp, tools)), buildAction);
    
    public static IOverridenActivityBuilder AddAIAgentAction(this IOverridenActivityBuilder activityBuilder, AIAgentFactory aiAgentFactory, ActionBuildAction? buildAction = null)
        => AddAIAgentAction(activityBuilder, ActivityNode<AIAgentActionNode>.Name, aiAgentFactory, buildAction);

    public static IOverridenActivityBuilder AddAIAgentAction(this IOverridenActivityBuilder activityBuilder,
        string aiAgentActionNodeName, AIAgentFactoryAsync aiAgentFactoryAsync, ActionBuildAction? buildAction = null)
        => ((IActivityBuilder)activityBuilder).AddAIAgentAction(aiAgentActionNodeName, aiAgentFactoryAsync, buildAction) as IOverridenActivityBuilder;
    
    public static IOverridenActivityBuilder AddAIAgentAction(this IOverridenActivityBuilder activityBuilder, AIAgentFactoryAsync aiAgentFactoryAsync, ActionBuildAction? buildAction = null)
        => AddAIAgentAction(activityBuilder, ActivityNode<AIAgentActionNode>.Name, aiAgentFactoryAsync, buildAction);
    
    public static IOverridenActivityBuilder AddAIAgentAction<TAIAgent>(this IOverridenActivityBuilder activityBuilder, string aiAgentActionNodeName, ActionBuildAction? buildAction = null)
        where TAIAgent : class, IAIAgent
        => ((IActivityBuilder)activityBuilder).AddAIAgentAction<TAIAgent>(aiAgentActionNodeName, buildAction) as IOverridenActivityBuilder;

    public static IOverridenActivityBuilder AddAIAgentAction<TAIAgent>(this IOverridenActivityBuilder activityBuilder, ActionBuildAction? buildAction = null)
        where TAIAgent : class, IAIAgent
        => AddAIAgentAction<TAIAgent>(activityBuilder, ActivityNode<AIAgentActionNode<TAIAgent>>.Name, buildAction);
}
using Stateflows.Activities;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.Common.Interfaces;
using Stateflows.MAF.AIAgents.Classes;
using Stateflows.MAF.AIAgents.Registration;

namespace Stateflows.MAF.AIAgents.Extensions;

public static class ReactiveStructuredActivityBuilderAIExtensions
{
    public static IReactiveStructuredActivityBuilder AddAIAgentAction(this IReactiveStructuredActivityBuilder activityBuilder, string aiAgentActionNodeName, AIAgentFactory aiAgentFactory, ActionBuildAction? buildAction = null)
        => AddAIAgentAction(activityBuilder, aiAgentActionNodeName, (sp, tools) => Task.FromResult(aiAgentFactory(sp, tools)), buildAction);
    
    public static IReactiveStructuredActivityBuilder AddAIAgentAction(this IReactiveStructuredActivityBuilder activityBuilder, AIAgentFactory aiAgentFactory, ActionBuildAction? buildAction = null)
        => AddAIAgentAction(activityBuilder, ActivityNode<AIAgentActionNode>.Name, aiAgentFactory, buildAction);

    public static IReactiveStructuredActivityBuilder AddAIAgentAction(this IReactiveStructuredActivityBuilder activityBuilder,
        string aiAgentActionNodeName, AIAgentFactoryAsync aiAgentFactoryAsync, ActionBuildAction? buildAction = null)
        => activityBuilder.AddAction<AIAgentActionNode>(b =>
        {
            b.AddConfiguration(aiAgentFactoryAsync);
            
            buildAction?.Invoke(b as IActionBuilder);
        });
    
    public static IReactiveStructuredActivityBuilder AddAIAgentAction(this IReactiveStructuredActivityBuilder activityBuilder, AIAgentFactoryAsync aiAgentFactoryAsync, ActionBuildAction? buildAction = null)
        => AddAIAgentAction(activityBuilder, ActivityNode<AIAgentActionNode>.Name, aiAgentFactoryAsync, buildAction);
    
    public static IReactiveStructuredActivityBuilder AddAIAgentAction<TAIAgent>(this IReactiveStructuredActivityBuilder activityBuilder, string aiAgentActionNodeName, ActionBuildAction? buildAction = null)
        where TAIAgent : class, IAIAgent
        => activityBuilder.AddAction<AIAgentActionNode<TAIAgent>>(b =>
        {
            buildAction?.Invoke(b as IActionBuilder);
        });

    public static IReactiveStructuredActivityBuilder AddAIAgentAction<TAIAgent>(this IReactiveStructuredActivityBuilder activityBuilder, ActionBuildAction? buildAction = null)
        where TAIAgent : class, IAIAgent
        => AddAIAgentAction<TAIAgent>(activityBuilder, ActivityNode<AIAgentActionNode<TAIAgent>>.Name, buildAction);
    
    
    
    
    public static IOverridenReactiveStructuredActivityBuilder AddAIAgentAction(this IOverridenReactiveStructuredActivityBuilder activityBuilder, string aiAgentActionNodeName, AIAgentFactory aiAgentFactory, ActionBuildAction? buildAction = null)
        => AddAIAgentAction(activityBuilder, aiAgentActionNodeName, (sp, tools) => Task.FromResult(aiAgentFactory(sp, tools)), buildAction);
    
    public static IOverridenReactiveStructuredActivityBuilder AddAIAgentAction(this IOverridenReactiveStructuredActivityBuilder activityBuilder, AIAgentFactory aiAgentFactory, ActionBuildAction? buildAction = null)
        => AddAIAgentAction(activityBuilder, ActivityNode<AIAgentActionNode>.Name, aiAgentFactory, buildAction);

    public static IOverridenReactiveStructuredActivityBuilder AddAIAgentAction(this IOverridenReactiveStructuredActivityBuilder activityBuilder,
        string aiAgentActionNodeName, AIAgentFactoryAsync aiAgentFactoryAsync, ActionBuildAction? buildAction = null)
        => ((IReactiveStructuredActivityBuilder)activityBuilder).AddAIAgentAction(aiAgentActionNodeName, aiAgentFactoryAsync, buildAction) as IOverridenReactiveStructuredActivityBuilder;
    
    public static IOverridenReactiveStructuredActivityBuilder AddAIAgentAction(this IOverridenReactiveStructuredActivityBuilder activityBuilder, AIAgentFactoryAsync aiAgentFactoryAsync, ActionBuildAction? buildAction = null)
        => AddAIAgentAction(activityBuilder, ActivityNode<AIAgentActionNode>.Name, aiAgentFactoryAsync, buildAction);
    
    public static IOverridenReactiveStructuredActivityBuilder AddAIAgentAction<TAIAgent>(this IOverridenReactiveStructuredActivityBuilder activityBuilder, string aiAgentActionNodeName, ActionBuildAction? buildAction = null)
        where TAIAgent : class, IAIAgent
        => ((IReactiveStructuredActivityBuilder)activityBuilder).AddAIAgentAction<TAIAgent>(aiAgentActionNodeName, buildAction) as IOverridenReactiveStructuredActivityBuilder;

    public static IOverridenReactiveStructuredActivityBuilder AddAIAgentAction<TAIAgent>(this IOverridenReactiveStructuredActivityBuilder activityBuilder, ActionBuildAction? buildAction = null)
        where TAIAgent : class, IAIAgent
        => AddAIAgentAction<TAIAgent>(activityBuilder, ActivityNode<AIAgentActionNode<TAIAgent>>.Name, buildAction);
}
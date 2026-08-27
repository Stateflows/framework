using Stateflows.Activities;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.Common.Interfaces;
using Stateflows.MAF.AIAgents.Classes;
using Stateflows.MAF.AIAgents.Registration;

namespace Stateflows.MAF.AIAgents.Extensions;

public static class ActivityBuilderAgenticExtensions
{
    public static IActivityBuilder AddAgenticActivity(this IActivityBuilder activityBuilder, string agenticActivityNodeName, AIAgentFactory aiAgentFactory, ReactiveStructuredActivityExternalsBuildAction? buildAction = null)
        => AddAgenticActivity(activityBuilder, agenticActivityNodeName, (sp, tools) => Task.FromResult(aiAgentFactory(sp, tools)), buildAction);
    
    public static IActivityBuilder AddAgenticActivity(this IActivityBuilder activityBuilder, AIAgentFactory aiAgentFactory, ReactiveStructuredActivityExternalsBuildAction? buildAction = null)
        => AddAgenticActivity(activityBuilder, ActivityNode<AIAgentActionNode>.Name, aiAgentFactory, buildAction);

    public static IActivityBuilder AddAgenticActivity(this IActivityBuilder activityBuilder,
        string agenticActivityNodeName, AIAgentFactoryAsync aiAgentFactoryAsync, ReactiveStructuredActivityExternalsBuildAction? buildAction = null)
        => activityBuilder.AddStructuredActivity(agenticActivityNodeName, b =>
        {
            b
                .AddInitial(b => b
                    .AddControlFlow<AIAgentActionNode>()
                )
                .AddAcceptEventAction<string>(c =>
                    {
                        c.Output(c.Event);
                        return Task.CompletedTask;
                    }, b => b
                        .AddFlow<string, AIAgentActionNode>(b => b.SetWeight(0))
                )
                .AddAcceptEventAction<AgenticMessage>(c =>
                    {
                        c.Output(c.Event);
                        return Task.CompletedTask;
                    }, b => b
                        .AddFlow<AgenticMessage, AIAgentActionNode>(b => b.SetWeight(0))
                )
                .AddAction<AIAgentActionNode>(b => b
                    .AddConfiguration(aiAgentFactoryAsync)
                );
            
            buildAction?.Invoke(b);
        });
    
    public static IActivityBuilder AddAgenticActivity(this IActivityBuilder activityBuilder, AIAgentFactoryAsync aiAgentFactoryAsync, ReactiveStructuredActivityExternalsBuildAction? buildAction = null)
        => AddAgenticActivity(activityBuilder, ActivityNode<AIAgentActionNode>.Name, aiAgentFactoryAsync, buildAction);
    
    public static IActivityBuilder AddAgenticActivity<TAIAgent>(this IActivityBuilder activityBuilder, string agenticActivityNodeName, ReactiveStructuredActivityExternalsBuildAction? buildAction = null)
        where TAIAgent : class, IAIAgent
        => activityBuilder.AddStructuredActivity(agenticActivityNodeName, b =>
        {
            b
                .AddInitial(b => b
                    .AddControlFlow<AIAgentActionNode<TAIAgent>>()
                )
                .AddInput(b => b
                    .AddFlow<string, AIAgentActionNode<TAIAgent>>(b => b.SetWeight(0))
                    .AddFlow<AgenticMessage, AIAgentActionNode<TAIAgent>>(b => b.SetWeight(0))
                )
                .AddAcceptEventAction<string>(c =>
                    {
                        c.Output(c.Event);
                        return Task.CompletedTask;
                    }, b => b
                        .AddFlow<string, AIAgentActionNode<TAIAgent>>(b => b.SetWeight(0))
                )
                .AddAcceptEventAction<AgenticMessage>(c =>
                    {
                        c.Output(c.Event);
                        return Task.CompletedTask;
                    }, b => b
                        .AddFlow<AgenticMessage, AIAgentActionNode<TAIAgent>>(b => b.SetWeight(0))
                )
                .AddAction<AIAgentActionNode<TAIAgent>>();
            
            buildAction?.Invoke(b);
        });

    public static IActivityBuilder AddAgenticActivity<TAIAgent>(this IActivityBuilder activityBuilder, ReactiveStructuredActivityExternalsBuildAction? buildAction = null)
        where TAIAgent : class, IAIAgent
        => AddAgenticActivity<TAIAgent>(activityBuilder, ActivityNode<AIAgentActionNode<TAIAgent>>.Name, buildAction);
    
    
    
    
    public static IOverridenActivityBuilder AddAgenticActivity(this IOverridenActivityBuilder activityBuilder, string agenticActivityNodeName, AIAgentFactory aiAgentFactory, OverridenReactiveStructuredActivityExternalsBuildAction? buildAction = null)
        => AddAgenticActivity(activityBuilder, agenticActivityNodeName, (sp, tools) => Task.FromResult(aiAgentFactory(sp, tools)), buildAction);
    
    public static IOverridenActivityBuilder AddAgenticActivity(this IOverridenActivityBuilder activityBuilder, AIAgentFactory aiAgentFactory, OverridenReactiveStructuredActivityExternalsBuildAction? buildAction = null)
        => AddAgenticActivity(activityBuilder, ActivityNode<AIAgentActionNode>.Name, aiAgentFactory, buildAction);

    public static IOverridenActivityBuilder AddAgenticActivity(this IOverridenActivityBuilder activityBuilder,
        string agenticActivityNodeName, AIAgentFactoryAsync aiAgentFactoryAsync, OverridenReactiveStructuredActivityExternalsBuildAction? buildAction = null)
        => ((IActivityBuilder)activityBuilder).AddAgenticActivity(agenticActivityNodeName, aiAgentFactoryAsync, b => buildAction?.Invoke(b as IOverridenReactiveStructuredActivityExternalsBuilder)) as IOverridenActivityBuilder;
    
    public static IOverridenActivityBuilder AddAgenticActivity(this IOverridenActivityBuilder activityBuilder, AIAgentFactoryAsync aiAgentFactoryAsync, OverridenReactiveStructuredActivityExternalsBuildAction? buildAction = null)
        => AddAgenticActivity(activityBuilder, ActivityNode<AIAgentActionNode>.Name, aiAgentFactoryAsync, buildAction);
    
    public static IOverridenActivityBuilder AddAgenticActivity<TAIAgent>(this IOverridenActivityBuilder activityBuilder, string agenticActivityNodeName, OverridenReactiveStructuredActivityExternalsBuildAction? buildAction = null)
        where TAIAgent : class, IAIAgent
        => ((IActivityBuilder)activityBuilder).AddAgenticActivity<TAIAgent>(agenticActivityNodeName, b => buildAction?.Invoke(b as IOverridenReactiveStructuredActivityExternalsBuilder)) as IOverridenActivityBuilder;

    public static IOverridenActivityBuilder AddAgenticActivity<TAIAgent>(this IOverridenActivityBuilder activityBuilder, OverridenReactiveStructuredActivityExternalsBuildAction? buildAction = null)
        where TAIAgent : class, IAIAgent
        => AddAgenticActivity<TAIAgent>(activityBuilder, ActivityNode<AIAgentActionNode<TAIAgent>>.Name, buildAction);
}
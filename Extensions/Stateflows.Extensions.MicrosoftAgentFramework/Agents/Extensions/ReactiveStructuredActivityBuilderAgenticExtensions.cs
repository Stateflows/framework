using Stateflows.Activities;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.Common.Interfaces;
using Stateflows.MAF.AIAgents.Classes;
using Stateflows.MAF.AIAgents.Registration;

namespace Stateflows.MAF.AIAgents.Extensions;

public static class ReactiveStructuredActivityBuilderAgenticExtensions
{
    public static IReactiveStructuredActivityBuilder AddAgenticActivity(this IReactiveStructuredActivityBuilder activityBuilder, string agenticActivityNodeName, AIAgentFactory aiAgentFactory, ReactiveStructuredActivityExternalsBuildAction? buildAction = null)
        => AddAgenticActivity(activityBuilder, agenticActivityNodeName, (sp, tools) => Task.FromResult(aiAgentFactory(sp, tools)), buildAction);
    
    public static IReactiveStructuredActivityBuilder AddAgenticActivity(this IReactiveStructuredActivityBuilder activityBuilder, AIAgentFactory aiAgentFactory, ReactiveStructuredActivityExternalsBuildAction? buildAction = null)
        => AddAgenticActivity(activityBuilder, ActivityNode<AIAgentActionNode>.Name, aiAgentFactory, buildAction);

    public static IReactiveStructuredActivityBuilder AddAgenticActivity(this IReactiveStructuredActivityBuilder activityBuilder,
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
    
    public static IReactiveStructuredActivityBuilder AddAgenticActivity(this IReactiveStructuredActivityBuilder activityBuilder, AIAgentFactoryAsync aiAgentFactoryAsync, ReactiveStructuredActivityExternalsBuildAction? buildAction = null)
        => AddAgenticActivity(activityBuilder, ActivityNode<AIAgentActionNode>.Name, aiAgentFactoryAsync, buildAction);
    
    public static IReactiveStructuredActivityBuilder AddAgenticActivity<TAIAgent>(this IReactiveStructuredActivityBuilder activityBuilder, string agenticActivityNodeName, ReactiveStructuredActivityExternalsBuildAction? buildAction = null)
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

    public static IReactiveStructuredActivityBuilder AddAgenticActivity<TAIAgent>(this IReactiveStructuredActivityBuilder activityBuilder, ReactiveStructuredActivityExternalsBuildAction? buildAction = null)
        where TAIAgent : class, IAIAgent
        => AddAgenticActivity<TAIAgent>(activityBuilder, ActivityNode<AIAgentActionNode<TAIAgent>>.Name, buildAction);
    
    
    
    public static IOverridenReactiveStructuredActivityBuilder AddAgenticActivity(this IOverridenReactiveStructuredActivityBuilder activityBuilder, string agenticActivityNodeName, AIAgentFactory aiAgentFactory, OverridenReactiveStructuredActivityExternalsBuildAction? buildAction = null)
        => AddAgenticActivity(activityBuilder, agenticActivityNodeName, (sp, tools) => Task.FromResult(aiAgentFactory(sp, tools)), buildAction);
    
    public static IOverridenReactiveStructuredActivityBuilder AddAgenticActivity(this IOverridenReactiveStructuredActivityBuilder activityBuilder, AIAgentFactory aiAgentFactory, OverridenReactiveStructuredActivityExternalsBuildAction? buildAction = null)
        => AddAgenticActivity(activityBuilder, ActivityNode<AIAgentActionNode>.Name, aiAgentFactory, buildAction);

    public static IOverridenReactiveStructuredActivityBuilder AddAgenticActivity(this IOverridenReactiveStructuredActivityBuilder activityBuilder,
        string agenticActivityNodeName, AIAgentFactoryAsync aiAgentFactoryAsync, OverridenReactiveStructuredActivityExternalsBuildAction? buildAction = null)
        => ((IReactiveStructuredActivityBuilder)activityBuilder).AddAgenticActivity(agenticActivityNodeName, aiAgentFactoryAsync, b => buildAction(b as IOverridenReactiveStructuredActivityExternalsBuilder)) as IOverridenReactiveStructuredActivityBuilder;
    
    public static IOverridenReactiveStructuredActivityBuilder AddAgenticActivity(this IOverridenReactiveStructuredActivityBuilder activityBuilder, AIAgentFactoryAsync aiAgentFactoryAsync, OverridenReactiveStructuredActivityExternalsBuildAction? buildAction = null)
        => AddAgenticActivity(activityBuilder, ActivityNode<AIAgentActionNode>.Name, aiAgentFactoryAsync, buildAction);
    
    public static IOverridenReactiveStructuredActivityBuilder AddAgenticActivity<TAIAgent>(this IOverridenReactiveStructuredActivityBuilder activityBuilder, string agenticActivityNodeName, OverridenReactiveStructuredActivityExternalsBuildAction? buildAction = null)
        where TAIAgent : class, IAIAgent
        => ((IReactiveStructuredActivityBuilder)activityBuilder).AddAgenticActivity<TAIAgent>(agenticActivityNodeName, b => buildAction(b as IOverridenReactiveStructuredActivityExternalsBuilder)) as IOverridenReactiveStructuredActivityBuilder;

    public static IOverridenReactiveStructuredActivityBuilder AddAgenticActivity<TAIAgent>(this IOverridenReactiveStructuredActivityBuilder activityBuilder, OverridenReactiveStructuredActivityExternalsBuildAction? buildAction = null)
        where TAIAgent : class, IAIAgent
        => AddAgenticActivity<TAIAgent>(activityBuilder, ActivityNode<AIAgentActionNode<TAIAgent>>.Name, buildAction);
}
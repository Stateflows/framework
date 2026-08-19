using System.Reflection;
using Microsoft.Extensions.AI;
using Stateflows.Common.Interfaces;
using Stateflows.MAF.AIAgents.Registration;
using Stateflows.MAF.AIAgents.Classes;
using Stateflows.StateMachines;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.MAF.AIAgents.Extensions;

public static class StateBuilderExtensions
{
    public static IStateBuilder AddDoAgent(this IStateBuilder stateBuilder, AIAgentFactory aiAgentFactory, AgentBuildAction? buildAction = null)
        => AddDoAgent(stateBuilder, sp => Task.FromResult(aiAgentFactory(sp)), buildAction);
    
    public static IStateBuilder AddDoAgent(this IStateBuilder stateBuilder, AIAgentFactoryAsync aiAgentFactoryAsync, AgentBuildAction? buildAction = null)
        => stateBuilder.AddDoAction<AIAgentAction>(b =>
        {
            b.AddConfiguration(aiAgentFactoryAsync);
            b.AddConfiguration((IMetadataBuilder)stateBuilder);
            buildAction?.Invoke(new AIAgentBuilder(b));
            b.SetCustomBehaviorClassType(MAFBehaviorType.AIAgent);
        });
    
    private static Task<ChatMessage> FormatTokenAsync<TTokenConsumerAgent, TToken>(IAIAgentContext iaiAgentContext, TToken token)
        where TTokenConsumerAgent : class, ITokenConsumerAiAgent<TToken>
        => TTokenConsumerAgent.FormatTokenAsync(iaiAgentContext, token);
    
    public static IStateBuilder AddDoAgent<TAgent>(this IStateBuilder stateBuilder, AgentBuildAction? buildAction = null)
        where TAgent : class, IAIAgent
        => stateBuilder.AddDoAction<AIAgentAction<TAgent>>(b =>
        {
            var agentType = typeof(TAgent);
            var consumedTypes = agentType
                .GetInterfaces()
                .Where(
                    i => i.IsGenericType && (
                        i.GetGenericTypeDefinition() == typeof(ITokenConsumerAiAgent<>) || 
                        i.GetGenericTypeDefinition() == typeof(IEventConsumerAIAgent<>)
                    )
                )
                .Select(i => i.GetGenericArguments().First())
                .ToArray();

            b.AddConfiguration((IMetadataBuilder)stateBuilder);
            var agentBuilder = new AIAgentBuilder(b);
            buildAction?.Invoke(agentBuilder);
            var addConsumedTokenMethod = typeof(AIAgentBuilder).GetMethod(nameof(AIAgentBuilder.AddConsumedToken));
            foreach (var consumedTokenType in consumedTypes)
            {
                addConsumedTokenMethod.MakeGenericMethod(consumedTokenType).Invoke(agentBuilder, [ (IAIAgentContext c, object t) =>
                    {
                        var formatTokenAsyncMethod = typeof(StateBuilderExtensions).GetMethod(nameof(FormatTokenAsync), BindingFlags.Static | BindingFlags.NonPublic);
                        return (Task<ChatMessage>)formatTokenAsyncMethod
                            .MakeGenericMethod(typeof(TAgent), consumedTokenType).Invoke(null, [c, t]);
                    }
                ]);
            }
            b.SetCustomBehaviorClassType(MAFBehaviorType.AIAgent);
        });
    
    public static IOverridenRegionalizedStateBuilder AddDoAgent(this IOverridenRegionalizedStateBuilder stateBuilder, AIAgentFactory aiAgentFactory, AgentBuildAction? buildAction = null)
        => AddDoAgent((IStateBuilder)stateBuilder, aiAgentFactory, buildAction) as IOverridenRegionalizedStateBuilder;
    
    public static IOverridenRegionalizedStateBuilder AddDoAgent(this IOverridenRegionalizedStateBuilder stateBuilder, AIAgentFactoryAsync aiAgentFactoryAsync, AgentBuildAction? buildAction = null)
        => AddDoAgent((IStateBuilder)stateBuilder, aiAgentFactoryAsync, buildAction) as IOverridenRegionalizedStateBuilder;
    
    public static IOverridenRegionalizedStateBuilder AddDoAgent<TAgent>(this IOverridenRegionalizedStateBuilder stateBuilder, AgentBuildAction? buildAction = null)
        where TAgent : class, IAIAgent
        => AddDoAgent<TAgent>((IStateBuilder)stateBuilder, buildAction) as IOverridenRegionalizedStateBuilder;
    
    public static IInitializedCompositeStateBuilder AddDoAgent(this IInitializedCompositeStateBuilder stateBuilder, AIAgentFactory aiAgentFactory, AgentBuildAction? buildAction = null)
        => AddDoAgent((IStateBuilder)stateBuilder, aiAgentFactory, buildAction) as IInitializedCompositeStateBuilder;
    
    public static IInitializedCompositeStateBuilder AddDoAgent(this IInitializedCompositeStateBuilder stateBuilder, AIAgentFactoryAsync aiAgentFactoryAsync, AgentBuildAction? buildAction = null)
        => AddDoAgent((IStateBuilder)stateBuilder, aiAgentFactoryAsync, buildAction) as IInitializedCompositeStateBuilder;
    
    public static IInitializedCompositeStateBuilder AddDoAgent<TAgent>(this IInitializedCompositeStateBuilder stateBuilder, AgentBuildAction? buildAction = null)
        where TAgent : class, IAIAgent
        => AddDoAgent<TAgent>((IStateBuilder)stateBuilder, buildAction) as IInitializedCompositeStateBuilder;
    
    public static IFinalizedCompositeStateBuilder AddDoAgent(this IFinalizedCompositeStateBuilder stateBuilder, AIAgentFactory aiAgentFactory, AgentBuildAction? buildAction = null)
        => AddDoAgent((IStateBuilder)stateBuilder, aiAgentFactory, buildAction) as IFinalizedCompositeStateBuilder;
    
    public static IFinalizedCompositeStateBuilder AddDoAgent(this IFinalizedCompositeStateBuilder stateBuilder, AIAgentFactoryAsync aiAgentFactoryAsync, AgentBuildAction? buildAction = null)
        => AddDoAgent((IStateBuilder)stateBuilder, aiAgentFactoryAsync, buildAction) as IFinalizedCompositeStateBuilder;
    
    public static IFinalizedCompositeStateBuilder AddDoAgent<TAgent>(this IFinalizedCompositeStateBuilder stateBuilder, AgentBuildAction? buildAction = null)
        where TAgent : class, IAIAgent
        => AddDoAgent<TAgent>((IStateBuilder)stateBuilder, buildAction) as IFinalizedCompositeStateBuilder;
    
    public static ICompositeStateBuilder AddDoAgent(this ICompositeStateBuilder stateBuilder, AIAgentFactory aiAgentFactory, AgentBuildAction? buildAction = null)
        => AddDoAgent((IStateBuilder)stateBuilder, aiAgentFactory, buildAction) as ICompositeStateBuilder;
    
    public static ICompositeStateBuilder AddDoAgent(this ICompositeStateBuilder stateBuilder, AIAgentFactoryAsync aiAgentFactoryAsync, AgentBuildAction? buildAction = null)
        => AddDoAgent((IStateBuilder)stateBuilder, aiAgentFactoryAsync, buildAction) as ICompositeStateBuilder;
    
    public static ICompositeStateBuilder AddDoAgent<TAgent>(this ICompositeStateBuilder stateBuilder, AgentBuildAction? buildAction = null)
        where TAgent : class, IAIAgent
        => AddDoAgent<TAgent>((IStateBuilder)stateBuilder, buildAction) as ICompositeStateBuilder;
    
    public static IFinalizedOverridenCompositeStateBuilder AddDoAgent(this IFinalizedOverridenCompositeStateBuilder stateBuilder, AIAgentFactory aiAgentFactory, AgentBuildAction? buildAction = null)
        => AddDoAgent((IStateBuilder)stateBuilder, aiAgentFactory, buildAction) as IFinalizedOverridenCompositeStateBuilder;
    
    public static IFinalizedOverridenCompositeStateBuilder AddDoAgent(this IFinalizedOverridenCompositeStateBuilder stateBuilder, AIAgentFactoryAsync aiAgentFactoryAsync, AgentBuildAction? buildAction = null)
        => AddDoAgent((IStateBuilder)stateBuilder, aiAgentFactoryAsync, buildAction) as IFinalizedOverridenCompositeStateBuilder;
    
    public static IFinalizedOverridenCompositeStateBuilder AddDoAgent<TAgent>(this IFinalizedOverridenCompositeStateBuilder stateBuilder, AgentBuildAction? buildAction = null)
        where TAgent : class, IAIAgent
        => AddDoAgent<TAgent>((IStateBuilder)stateBuilder, buildAction) as IFinalizedOverridenCompositeStateBuilder;
    
    public static IFinalizedOverridenRegionalizedCompositeStateBuilder AddDoAgent(this IFinalizedOverridenRegionalizedCompositeStateBuilder stateBuilder, AIAgentFactory aiAgentFactory, AgentBuildAction? buildAction = null)
        => AddDoAgent((IStateBuilder)stateBuilder, aiAgentFactory, buildAction) as IFinalizedOverridenRegionalizedCompositeStateBuilder;
    
    public static IFinalizedOverridenRegionalizedCompositeStateBuilder AddDoAgent(this IFinalizedOverridenRegionalizedCompositeStateBuilder stateBuilder, AIAgentFactoryAsync aiAgentFactoryAsync, AgentBuildAction? buildAction = null)
        => AddDoAgent((IStateBuilder)stateBuilder, aiAgentFactoryAsync, buildAction) as IFinalizedOverridenRegionalizedCompositeStateBuilder;
    
    public static IFinalizedOverridenRegionalizedCompositeStateBuilder AddDoAgent<TAgent>(this IFinalizedOverridenRegionalizedCompositeStateBuilder stateBuilder, AgentBuildAction? buildAction = null)
        where TAgent : class, IAIAgent
        => AddDoAgent<TAgent>((IStateBuilder)stateBuilder, buildAction) as IFinalizedOverridenRegionalizedCompositeStateBuilder;
    
    public static IOverridenCompositeStateBuilder AddDoAgent(this IOverridenCompositeStateBuilder stateBuilder, AIAgentFactory aiAgentFactory, AgentBuildAction? buildAction = null)
        => AddDoAgent((IStateBuilder)stateBuilder, aiAgentFactory, buildAction) as IOverridenCompositeStateBuilder;
    
    public static IOverridenCompositeStateBuilder AddDoAgent(this IOverridenCompositeStateBuilder stateBuilder, AIAgentFactoryAsync aiAgentFactoryAsync, AgentBuildAction? buildAction = null)
        => AddDoAgent((IStateBuilder)stateBuilder, aiAgentFactoryAsync, buildAction) as IOverridenCompositeStateBuilder;
    
    public static IOverridenCompositeStateBuilder AddDoAgent<TAgent>(this IOverridenCompositeStateBuilder stateBuilder, AgentBuildAction? buildAction = null)
        where TAgent : class, IAIAgent
        => AddDoAgent<TAgent>((IStateBuilder)stateBuilder, buildAction) as IOverridenCompositeStateBuilder;
    
    public static IOverridenRegionalizedCompositeStateBuilder AddDoAgent(this IOverridenRegionalizedCompositeStateBuilder stateBuilder, AIAgentFactory aiAgentFactory, AgentBuildAction? buildAction = null)
        => AddDoAgent((IStateBuilder)stateBuilder, aiAgentFactory, buildAction) as IOverridenRegionalizedCompositeStateBuilder;
    
    public static IOverridenRegionalizedCompositeStateBuilder AddDoAgent(this IOverridenRegionalizedCompositeStateBuilder stateBuilder, AIAgentFactoryAsync aiAgentFactoryAsync, AgentBuildAction? buildAction = null)
        => AddDoAgent((IStateBuilder)stateBuilder, aiAgentFactoryAsync, buildAction) as IOverridenRegionalizedCompositeStateBuilder;
    
    public static IOverridenRegionalizedCompositeStateBuilder AddDoAgent<TAgent>(this IOverridenRegionalizedCompositeStateBuilder stateBuilder, AgentBuildAction? buildAction = null)
        where TAgent : class, IAIAgent
        => AddDoAgent<TAgent>((IStateBuilder)stateBuilder, buildAction) as IOverridenRegionalizedCompositeStateBuilder;
    
    private static DefaultTransitionBuildAction PrepareAgenticTransition(string targetStateName, string description, DefaultTransitionBuildAction? buildAction = null)
        => b =>
        {
            var stateMetadata = ((IParentMetadataBuilder)b).ParentMetadata;
            var transitionMetadata = ((IMetadataBuilder)b).Metadata;
            
            List<Dictionary<string, object>>? agenticTransitions;
            if (!stateMetadata.TryGetValue(AIAgentConstants.Transitions, out var agenticTransitionsObj))
            {
                agenticTransitions = [];
                stateMetadata[AIAgentConstants.Transitions] = agenticTransitions;
            }
            else
            {
                agenticTransitions = (List<Dictionary<string, object>>)agenticTransitionsObj;
            }
            
            agenticTransitions.Add(transitionMetadata);
            
            var marker = Guid.NewGuid().ToString();
            b
                .AddMetadata(AIAgentConstants.ExtensionMode, AIAgentConstants.AgenticTransition)
                .AddMetadata(AIAgentConstants.TransitionName, $"go_to_{targetStateName.Split('.').Last().ToSnakeCase()}_{Random.Shared.Next(1000, 9999)}")
                .AddMetadata(AIAgentConstants.TransitionDescription, description)
                .AddMetadata(AIAgentConstants.GuardValue, marker)
                .AddGuard(Guards.Global.Value(AIAgentConstants.GuardKey).IsEqualTo(marker));

            buildAction?.Invoke(b);
        };

    public static IStateBuilder AddAgenticTransition(this IStateBuilder stateBuilder, string targetStateName, string prompt, DefaultTransitionBuildAction? transitionBuildAction = null)
        => stateBuilder.AddDefaultTransition(targetStateName, PrepareAgenticTransition(targetStateName, prompt, transitionBuildAction));
    
    public static IStateBuilder AddAgenticTransition<TTargetState>(this IStateBuilder stateBuilder, string prompt, DefaultTransitionBuildAction? transitionBuildAction = null)
        where TTargetState : class, IVertex
        => stateBuilder.AddDefaultTransition<TTargetState>(PrepareAgenticTransition(State<TTargetState>.Name, prompt, transitionBuildAction));
    
    public static IOverridenStateBuilder AddAgenticTransition(this IOverridenStateBuilder stateBuilder, string targetStateName, string prompt, DefaultTransitionBuildAction? transitionBuildAction = null)
        => stateBuilder.AddDefaultTransition(targetStateName, PrepareAgenticTransition(targetStateName, prompt, transitionBuildAction));
    
    public static IOverridenStateBuilder AddAgenticTransition<TTargetState>(this IOverridenStateBuilder stateBuilder, string prompt, DefaultTransitionBuildAction? transitionBuildAction = null)
        where TTargetState : class, IVertex
        => stateBuilder.AddDefaultTransition<TTargetState>(PrepareAgenticTransition(State<TTargetState>.Name, prompt, transitionBuildAction));
    
    public static IOverridenRegionalizedStateBuilder AddAgenticTransition(this IOverridenRegionalizedStateBuilder stateBuilder, string targetStateName, string prompt, DefaultTransitionBuildAction? transitionBuildAction = null)
        => stateBuilder.AddDefaultTransition(targetStateName, PrepareAgenticTransition(targetStateName, prompt, transitionBuildAction));
    
    public static IOverridenRegionalizedStateBuilder AddAgenticTransition<TTargetState>(this IOverridenRegionalizedStateBuilder stateBuilder, string prompt, DefaultTransitionBuildAction? transitionBuildAction = null)
        where TTargetState : class, IVertex
        => stateBuilder.AddDefaultTransition<TTargetState>(PrepareAgenticTransition(State<TTargetState>.Name, prompt, transitionBuildAction));
    
    public static IInitializedCompositeStateBuilder AddAgenticTransition(this IInitializedCompositeStateBuilder stateBuilder, string targetStateName, string prompt, DefaultTransitionBuildAction? transitionBuildAction = null)
        => stateBuilder.AddDefaultTransition(targetStateName, PrepareAgenticTransition(targetStateName, prompt, transitionBuildAction));
    
    public static IInitializedCompositeStateBuilder AddAgenticTransition<TTargetState>(this IInitializedCompositeStateBuilder stateBuilder, string prompt, DefaultTransitionBuildAction? transitionBuildAction = null)
        where TTargetState : class, IVertex
        => stateBuilder.AddDefaultTransition<TTargetState>(PrepareAgenticTransition(State<TTargetState>.Name, prompt, transitionBuildAction));   
    
    public static IFinalizedCompositeStateBuilder AddAgenticTransition(this IFinalizedCompositeStateBuilder stateBuilder, string targetStateName, string prompt, DefaultTransitionBuildAction? transitionBuildAction = null)
        => stateBuilder.AddDefaultTransition(targetStateName, PrepareAgenticTransition(targetStateName, prompt, transitionBuildAction));
    
    public static IFinalizedCompositeStateBuilder AddAgenticTransition<TTargetState>(this IFinalizedCompositeStateBuilder stateBuilder, string prompt, DefaultTransitionBuildAction? transitionBuildAction = null)
        where TTargetState : class, IVertex
        => stateBuilder.AddDefaultTransition<TTargetState>(PrepareAgenticTransition(State<TTargetState>.Name, prompt, transitionBuildAction));   
    
    public static ICompositeStateBuilder AddAgenticTransition(this ICompositeStateBuilder stateBuilder, string targetStateName, string prompt, DefaultTransitionBuildAction? transitionBuildAction = null)
        => stateBuilder.AddDefaultTransition(targetStateName, PrepareAgenticTransition(targetStateName, prompt, transitionBuildAction));
    
    public static ICompositeStateBuilder AddAgenticTransition<TTargetState>(this ICompositeStateBuilder stateBuilder, string prompt, DefaultTransitionBuildAction? transitionBuildAction = null)
        where TTargetState : class, IVertex
        => stateBuilder.AddDefaultTransition<TTargetState>(PrepareAgenticTransition(State<TTargetState>.Name, prompt, transitionBuildAction));   
    
    public static IFinalizedOverridenCompositeStateBuilder AddAgenticTransition(this IFinalizedOverridenCompositeStateBuilder stateBuilder, string targetStateName, string prompt, DefaultTransitionBuildAction? transitionBuildAction = null)
        => stateBuilder.AddDefaultTransition(targetStateName, PrepareAgenticTransition(targetStateName, prompt, transitionBuildAction));
    
    public static IFinalizedOverridenCompositeStateBuilder AddAgenticTransition<TTargetState>(this IFinalizedOverridenCompositeStateBuilder stateBuilder, string prompt, DefaultTransitionBuildAction? transitionBuildAction = null)
        where TTargetState : class, IVertex
        => stateBuilder.AddDefaultTransition<TTargetState>(PrepareAgenticTransition(State<TTargetState>.Name, prompt, transitionBuildAction));   
    
    public static IFinalizedOverridenRegionalizedCompositeStateBuilder AddAgenticTransition(this IFinalizedOverridenRegionalizedCompositeStateBuilder stateBuilder, string targetStateName, string prompt, DefaultTransitionBuildAction? transitionBuildAction = null)
        => stateBuilder.AddDefaultTransition(targetStateName, PrepareAgenticTransition(targetStateName, prompt, transitionBuildAction));
    
    public static IFinalizedOverridenRegionalizedCompositeStateBuilder AddAgenticTransition<TTargetState>(this IFinalizedOverridenRegionalizedCompositeStateBuilder stateBuilder, string prompt, DefaultTransitionBuildAction? transitionBuildAction = null)
        where TTargetState : class, IVertex
        => stateBuilder.AddDefaultTransition<TTargetState>(PrepareAgenticTransition(State<TTargetState>.Name, prompt, transitionBuildAction));   
    
    public static IOverridenCompositeStateBuilder AddAgenticTransition(this IOverridenCompositeStateBuilder stateBuilder, string targetStateName, string prompt, DefaultTransitionBuildAction? transitionBuildAction = null)
        => stateBuilder.AddDefaultTransition(targetStateName, PrepareAgenticTransition(targetStateName, prompt, transitionBuildAction));
    
    public static IOverridenCompositeStateBuilder AddAgenticTransition<TTargetState>(this IOverridenCompositeStateBuilder stateBuilder, string prompt, DefaultTransitionBuildAction? transitionBuildAction = null)
        where TTargetState : class, IVertex
        => stateBuilder.AddDefaultTransition<TTargetState>(PrepareAgenticTransition(State<TTargetState>.Name, prompt, transitionBuildAction));   
    
    public static IOverridenRegionalizedCompositeStateBuilder AddAgenticTransition(this IOverridenRegionalizedCompositeStateBuilder stateBuilder, string targetStateName, string prompt, DefaultTransitionBuildAction? transitionBuildAction = null)
        => stateBuilder.AddDefaultTransition(targetStateName, PrepareAgenticTransition(targetStateName, prompt, transitionBuildAction));
    
    public static IOverridenRegionalizedCompositeStateBuilder AddAgenticTransition<TTargetState>(this IOverridenRegionalizedCompositeStateBuilder stateBuilder, string prompt, DefaultTransitionBuildAction? transitionBuildAction = null)
        where TTargetState : class, IVertex
        => stateBuilder.AddDefaultTransition<TTargetState>(PrepareAgenticTransition(State<TTargetState>.Name, prompt, transitionBuildAction));
}
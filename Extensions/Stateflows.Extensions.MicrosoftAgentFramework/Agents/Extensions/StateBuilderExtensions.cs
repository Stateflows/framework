using System.Reflection;
using Microsoft.Extensions.AI;
using Stateflows.Common;
using Stateflows.Common.Exceptions;
using Stateflows.Common.Extensions;
using Stateflows.Common.Interfaces;
using Stateflows.Extensions.MicrosoftAgentFramework.Agents.Classes;
using Stateflows.MAF.AIAgents.Registration;
using Stateflows.MAF.AIAgents.Classes;
using Stateflows.MAF.AIAgents.Events;
using Stateflows.StateMachines;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.MAF.AIAgents.Extensions;

/// <summary>Provides agent and agentic-transition operations for state-machine state builders.</summary>
public static class StateBuilderExtensions
{
    internal static IBehaviorStateBuilder AddDoAIAgent(this IStateBuilder stateBuilder, AIAgentFactory aiAgentFactory, AIAgentBuildAction? buildAction = null)
        => AddDoAIAgent(stateBuilder, (sp, tools) => Task.FromResult(aiAgentFactory(sp, tools)), buildAction);
    
    internal static IBehaviorStateBuilder AddDoAIAgent(this IStateBuilder stateBuilder, AIAgentFactoryAsync aiAgentFactoryAsync, AIAgentBuildAction? buildAction = null)
        => stateBuilder.AddDoAction<AIAgentAction>(b =>
        {
            b.AddConfiguration(aiAgentFactoryAsync);
            b.AddConfiguration((IMetadataBuilder)stateBuilder);
            buildAction?.Invoke(new AIAgentBuilder(b));
            b.SetCustomBehaviorClassType(MAFBehaviorType.AIAgent);
        });
    
    private static Task<ChatMessage> FormatTokenAsync<TTokenConsumerAgent, TToken>(IAIAgentContext iaiAgentContext, TToken token)
        where TTokenConsumerAgent : class, ITokenConsumerAIAgent<TToken>
        => TTokenConsumerAgent.FormatTokenAsync(iaiAgentContext, token);
    
    internal static IBehaviorStateBuilder AddDoAIAgent<TAIAgent>(this IStateBuilder stateBuilder, AIAgentBuildAction? buildAction = null)
        where TAIAgent : class, IAIAgent
        => stateBuilder.AddDoAction<AIAgentAction<TAIAgent>>(b =>
        {
            var agentType = typeof(TAIAgent);
            var consumedTypes = agentType
                .GetInterfaces()
                .Where(
                    i => i.IsGenericType && (
                        i.GetGenericTypeDefinition() == typeof(ITokenConsumerAIAgent<>) || 
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
                            .MakeGenericMethod(typeof(TAIAgent), consumedTokenType).Invoke(null, [c, t]);
                    }
                ]);
            }
            b.SetCustomBehaviorClassType(MAFBehaviorType.AIAgent);
        });
    
    private static TransitionBuildAction<AgenticTransition> PrepareAgenticTransition(string targetStateName, string description, DefaultTransitionBuildAction? buildAction = null)
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
                .AddMetadata(AIAgentConstants.TransitionName, $"transition_to_{targetStateName.Split('.').Last().ToSnakeCase().Replace('<', '_').Replace('>', '_')}_{Random.Shared.Next(1000, 9999)}")
                .AddMetadata(AIAgentConstants.TransitionDescription, description)
                .AddMetadata(AIAgentConstants.GuardValue, marker)
                .AddGuard(c => c.Event.TransitionMarker == marker);
            // .AddGuard(Guards.Global.Value(AIAgentConstants.GuardKey).IsEqualTo(marker));

            buildAction?.Invoke(b as IDefaultTransitionBuilder);
        };
    
    // private static TransitionBuildAction<AgenticHandoff> PrepareAgenticHandoff(string targetStateName, string description, DefaultTransitionBuildAction? buildAction = null)
    //     => b =>
    //     {
    //         var stateMetadata = ((IParentMetadataBuilder)b).ParentMetadata;
    //         var transitionMetadata = ((IMetadataBuilder)b).Metadata;
    //         
    //         List<Dictionary<string, object>>? agenticTransitions;
    //         if (!stateMetadata.TryGetValue(AIAgentConstants.Transitions, out var agenticTransitionsObj))
    //         {
    //             agenticTransitions = [];
    //             stateMetadata[AIAgentConstants.Transitions] = agenticTransitions;
    //         }
    //         else
    //         {
    //             agenticTransitions = (List<Dictionary<string, object>>)agenticTransitionsObj;
    //         }
    //         
    //         agenticTransitions.Add(transitionMetadata);
    //         
    //         var marker = Guid.NewGuid().ToString();
    //         b
    //             .AddMetadata(AIAgentConstants.ExtensionMode, AIAgentConstants.AgenticTransition)
    //             .AddMetadata(AIAgentConstants.TransitionName, $"handoff_to_{targetStateName.Split('.').Last().ToSnakeCase().Replace('<', '_').Replace('>', '_')}_{Random.Shared.Next(1000, 9999)}")
    //             .AddMetadata(AIAgentConstants.TransitionDescription, description)
    //             .AddMetadata(AIAgentConstants.GuardValue, marker)
    //             .AddGuard(c => c.Event.HandoffMarker == marker);
    //         // .AddGuard(Guards.Global.Value(AIAgentConstants.GuardKey).IsEqualTo(marker));
    //
    //         buildAction?.Invoke(b as IDefaultTransitionBuilder);
    //     };

    /// <summary>Adds a transition that asks the agent whether the configured target should be selected.</summary>
    /// <param name="stateBuilder">The agentic state builder receiving the transition.</param>
    /// <param name="targetStateName">The target state to enter when the transition inquiry is accepted.</param>
    /// <param name="prompt">The prompt presented to the agent to evaluate this transition.</param>
    /// <param name="transitionBuildAction">Optional configuration for the generated default transition.</param>
    /// <returns>The agentic state builder for further configuration.</returns>
    public static IAgenticStateBuilder AddAgenticTransition(this IAgenticStateBuilder stateBuilder, string targetStateName, string prompt, DefaultTransitionBuildAction? transitionBuildAction = null)
        => new AgenticStateBuilder(stateBuilder.AddTransition<AgenticTransition>(targetStateName, PrepareAgenticTransition(targetStateName, prompt, transitionBuildAction)));
    
    /// <summary>Adds a transition that asks the agent whether the typed target state should be selected.</summary>
    /// <typeparam name="TTargetState">The target state vertex type.</typeparam>
    /// <param name="stateBuilder">The agentic state builder receiving the transition.</param>
    /// <param name="prompt">The prompt presented to the agent to evaluate this transition.</param>
    /// <param name="transitionBuildAction">Optional configuration for the generated default transition.</param>
    /// <returns>The agentic state builder for further configuration.</returns>
    public static IAgenticStateBuilder AddAgenticTransition<TTargetState>(this IAgenticStateBuilder stateBuilder, string prompt, DefaultTransitionBuildAction? transitionBuildAction = null)
        where TTargetState : class, IVertex
        => new AgenticStateBuilder(stateBuilder.AddTransition<AgenticTransition, TTargetState>(PrepareAgenticTransition(State<TTargetState>.Name, prompt, transitionBuildAction)));
    
    // public static IAgenticStateBuilder AddAgenticHandoff<TTargetAgent>(this IAgenticStateBuilder stateBuilder, string prompt, DefaultTransitionBuildAction? transitionBuildAction = null)
    //     where TTargetAgent : class, IAIAgent
    //     => new AgenticStateBuilder(stateBuilder.AddTransition<AgenticHandoff, AgenticState<TTargetAgent>>(PrepareAgenticHandoff(State<AgenticState<TTargetAgent>>.Name, prompt, transitionBuildAction)));

    /// <summary>Adds an internal transition that sends a message generated from an event to the current agent behavior.</summary>
    /// <typeparam name="TEvent">The event type that triggers the internal transition.</typeparam>
    /// <param name="stateBuilder">The agentic state builder receiving the transition.</param>
    /// <param name="agenticMessageFactory">Creates the message sent to the agent from the transition event context.</param>
    /// <param name="transitionBuildAction">Optional configuration for the internal transition.</param>
    /// <returns>The agentic state builder for further configuration.</returns>
    public static IAgenticStateBuilder AddAgenticReaction<TEvent>(this IAgenticStateBuilder stateBuilder, Func<IEventContext<TEvent>, AgenticMessage> agenticMessageFactory, InternalTransitionBuildAction<TEvent>? transitionBuildAction = null)
        => new AgenticStateBuilder(stateBuilder.AddInternalTransition<TEvent>(b =>
        {
            b.AddEffect(c => c.Behavior.Send(agenticMessageFactory(c)));
            
            transitionBuildAction?.Invoke(b);
        }));
        
    // public static IAgenticStateBuilder AddAgentGuardedTransition<TEvent>(this IAgenticStateBuilder stateBuilder, string targetStateName, string prompt, DefaultTransitionBuildAction? transitionBuildAction = null)
    //     => new AgenticStateBuilder(stateBuilder.AddTransition<TEvent>(targetStateName, PrepareAgenticTransition(targetStateName, prompt, transitionBuildAction)));
    //
    // public static IAgenticStateBuilder AddAgentGuardedTransition<TEvent, TTargetState>(this IAgenticStateBuilder stateBuilder, string prompt, DefaultTransitionBuildAction? transitionBuildAction = null)
    //     where TTargetState : class
    // {
    //     if (!typeof(TTargetState).GetInterfaces().Any(i => i == typeof(IVertex) || i == typeof(IAIAgent)))
    //     {
    //         throw new StateflowsDefinitionException($"Type {typeof(TTargetState).FullName} must implement either {nameof(IVertex)} or {nameof(IAIAgent)} to be used as a target state in an agentic transition.");
    //     }
    //     
    //     var stateName = typeof(TTargetState).GetReadableName(TypedElements.StateMachineStates);
    //     return new AgenticStateBuilder(stateBuilder.AddTransition<TEvent>(stateName, PrepareAgenticTransition(stateName, prompt, transitionBuildAction)));
    // }
}
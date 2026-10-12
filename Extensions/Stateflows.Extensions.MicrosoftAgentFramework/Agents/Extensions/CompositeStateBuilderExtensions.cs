using Stateflows.Extensions.MicrosoftAgentFramework.Agents.Classes;
using Stateflows.MAF.AIAgents.Registration;
using Stateflows.StateMachines;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.MAF.AIAgents.Extensions;

public static class CompositeStateBuilderExtensions
{    
    public static IInitializedCompositeStateBuilder AddInitialAgenticState(this ICompositeStateBuilder builder, string stateName, AIAgentFactoryAsync aiAgentFactoryAsync, AIAgentBuildAction? buildAction = null, Action<IAgenticStateBuilder>? stateBuildAction = null)
        => builder.AddInitialState(stateName, b =>
        {
            var behaviorStateBuilder = b.AddDoAIAgent(aiAgentFactoryAsync, buildAction);
            
            stateBuildAction?.Invoke(new AgenticStateBuilder(behaviorStateBuilder));
        });
    
    public static IInitializedCompositeStateBuilder AddInitialAgenticState<TAIAgent>(this ICompositeStateBuilder builder, Action<IAgenticStateBuilder>? stateBuildAction = null)
        where TAIAgent : class, IAIAgent
        => builder.AddInitialAgenticState<TAIAgent>(State<AgenticState<TAIAgent>>.Name, stateBuildAction);
    
    public static IInitializedCompositeStateBuilder AddInitialAgenticState<TAIAgent>(this ICompositeStateBuilder builder, string stateName, Action<IAgenticStateBuilder>? stateBuildAction = null)
        where TAIAgent : class, IAIAgent
        => builder.AddInitialState<AgenticState<TAIAgent>>(stateName, b =>
        {
            var behaviorStateBuilder = b.AddDoAIAgent<TAIAgent>();
            
            stateBuildAction?.Invoke(new AgenticStateBuilder(behaviorStateBuilder));
        });
    
        
    public static IInitializedCompositeStateBuilder AddAgenticState(this IInitializedCompositeStateBuilder builder, string stateName, AIAgentFactoryAsync aiAgentFactoryAsync, AIAgentBuildAction? buildAction = null, Action<IAgenticStateBuilder>? stateBuildAction = null)
        => builder.AddState(stateName, b =>
        {
            var behaviorStateBuilder = b.AddDoAIAgent(aiAgentFactoryAsync, buildAction);
            
            stateBuildAction?.Invoke(new AgenticStateBuilder(behaviorStateBuilder));
        });
    
    public static IInitializedCompositeStateBuilder AddAgenticState<TAIAgent>(this IInitializedCompositeStateBuilder builder, Action<IAgenticStateBuilder>? stateBuildAction = null)
        where TAIAgent : class, IAIAgent
        => builder.AddAgenticState<TAIAgent>(State<AgenticState<TAIAgent>>.Name, stateBuildAction);

    public static IInitializedCompositeStateBuilder AddAgenticState<TAIAgent>(this IInitializedCompositeStateBuilder builder, string stateName, Action<IAgenticStateBuilder>? stateBuildAction = null)
        where TAIAgent : class, IAIAgent
        => builder.AddState<AgenticState<TAIAgent>>(stateName, b =>
        {
            var behaviorStateBuilder = b.AddDoAIAgent<TAIAgent>();
            
            stateBuildAction?.Invoke(new AgenticStateBuilder(behaviorStateBuilder));
        });
}
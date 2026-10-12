using Stateflows.Extensions.MicrosoftAgentFramework.Agents.Classes;
using Stateflows.MAF.AIAgents.Registration;
using Stateflows.StateMachines;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.MAF.AIAgents.Extensions;

public static class RegionBuilderExtensions
{    
    public static IInitializedRegionBuilder AddInitialAgenticState(this IRegionBuilder builder, string stateName, AIAgentFactoryAsync aiAgentFactoryAsync, AIAgentBuildAction? buildAction = null, Action<IAgenticStateBuilder>? stateBuildAction = null)
        => builder.AddInitialState(stateName, b =>
        {
            var behaviorStateBuilder = b.AddDoAIAgent(aiAgentFactoryAsync, buildAction);
            
            stateBuildAction?.Invoke(new AgenticStateBuilder(behaviorStateBuilder));
        });
    
    public static IInitializedRegionBuilder AddInitialAgenticState<TAIAgent>(this IRegionBuilder builder, Action<IAgenticStateBuilder>? stateBuildAction = null)
        where TAIAgent : class, IAIAgent
        => AddInitialAgenticState<TAIAgent>(builder, State<AgenticState<TAIAgent>>.Name, stateBuildAction);
    
    public static IInitializedRegionBuilder AddInitialAgenticState<TAIAgent>(this IRegionBuilder builder, string stateName, Action<IAgenticStateBuilder>? stateBuildAction = null)
        where TAIAgent : class, IAIAgent
        => builder.AddInitialState<AgenticState<TAIAgent>>(stateName, b =>
        {
            var behaviorStateBuilder = b.AddDoAIAgent<TAIAgent>();
            
            stateBuildAction?.Invoke(new AgenticStateBuilder(behaviorStateBuilder));
        });
    
        
    public static IInitializedRegionBuilder AddAgenticState(this IInitializedRegionBuilder builder, string stateName, AIAgentFactoryAsync aiAgentFactoryAsync, AIAgentBuildAction? buildAction = null, Action<IAgenticStateBuilder>? stateBuildAction = null)
        => builder.AddState(stateName, b =>
        {
            var behaviorStateBuilder = b.AddDoAIAgent(aiAgentFactoryAsync, buildAction);
            
            stateBuildAction?.Invoke(new AgenticStateBuilder(behaviorStateBuilder));
        });
    
    public static IInitializedRegionBuilder AddAgenticState<TAIAgent>(this IInitializedRegionBuilder builder, Action<IAgenticStateBuilder>? stateBuildAction = null)
        where TAIAgent : class, IAIAgent
        => builder.AddAgenticState<TAIAgent>(State<AgenticState<TAIAgent>>.Name, stateBuildAction);

    public static IInitializedRegionBuilder AddAgenticState<TAIAgent>(this IInitializedRegionBuilder builder, string stateName, Action<IAgenticStateBuilder>? stateBuildAction = null)
        where TAIAgent : class, IAIAgent
        => builder.AddState<AgenticState<TAIAgent>>(stateName, b =>
        {
            var behaviorStateBuilder = b.AddDoAIAgent<TAIAgent>();
            
            stateBuildAction?.Invoke(new AgenticStateBuilder(behaviorStateBuilder));
        });
}
using Stateflows.Extensions.MicrosoftAgentFramework.Agents.Classes;
using Stateflows.MAF.AIAgents.Registration;
using Stateflows.StateMachines;

namespace Stateflows.MAF.AIAgents.Extensions;

public static class StateMachineBuilderExtensions
{    
    public static IInitializedStateMachineBuilder AddInitialAgenticState(this IStateMachineBuilder builder, string stateName, AIAgentFactoryAsync aiAgentFactoryAsync, AIAgentBuildAction? buildAction = null, Action<IAgenticStateBuilder>? stateBuildAction = null)
        => builder.AddInitialState(stateName, b =>
        {
            var behaviorStateBuilder = b.AddDoAIAgent(aiAgentFactoryAsync, buildAction);
            
            stateBuildAction?.Invoke(new AgenticStateBuilder(behaviorStateBuilder));
        });
    
    public static IInitializedStateMachineBuilder AddInitialAgenticState<TAIAgent>(this IStateMachineBuilder builder, Action<IAgenticStateBuilder>? stateBuildAction = null)
        where TAIAgent : class, IAIAgent
        => AddInitialAgenticState<TAIAgent>(builder, State<AgenticState<TAIAgent>>.Name, stateBuildAction);
    
    public static IInitializedStateMachineBuilder AddInitialAgenticState<TAIAgent>(this IStateMachineBuilder builder, string stateName, Action<IAgenticStateBuilder>? stateBuildAction = null)
        where TAIAgent : class, IAIAgent
        => builder.AddInitialState<AgenticState<TAIAgent>>(stateName, b =>
        {
            var behaviorStateBuilder = b.AddDoAIAgent<TAIAgent>();
            
            stateBuildAction?.Invoke(new AgenticStateBuilder(behaviorStateBuilder));
        });
    
        
    public static IInitializedStateMachineBuilder AddAgenticState(this IInitializedStateMachineBuilder builder, string stateName, AIAgentFactoryAsync aiAgentFactoryAsync, AIAgentBuildAction? buildAction = null, Action<IAgenticStateBuilder>? stateBuildAction = null)
        => builder.AddState(stateName, b =>
        {
            var behaviorStateBuilder = b.AddDoAIAgent(aiAgentFactoryAsync, buildAction);
            
            stateBuildAction?.Invoke(new AgenticStateBuilder(behaviorStateBuilder));
        });
    
    public static IInitializedStateMachineBuilder AddAgenticState<TAIAgent>(this IInitializedStateMachineBuilder builder, Action<IAgenticStateBuilder>? stateBuildAction = null)
        where TAIAgent : class, IAIAgent
        => AddAgenticState<TAIAgent>(builder, State<AgenticState<TAIAgent>>.Name, stateBuildAction);

    public static IInitializedStateMachineBuilder AddAgenticState<TAIAgent>(this IInitializedStateMachineBuilder builder, string stateName, Action<IAgenticStateBuilder>? stateBuildAction = null)
        where TAIAgent : class, IAIAgent
        => builder.AddState<AgenticState<TAIAgent>>(stateName, b =>
        {
            var behaviorStateBuilder = b.AddDoAIAgent<TAIAgent>();
            
            stateBuildAction?.Invoke(new AgenticStateBuilder(behaviorStateBuilder));
        });
}
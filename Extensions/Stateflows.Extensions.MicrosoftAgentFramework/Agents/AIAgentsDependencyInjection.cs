using System.Diagnostics;
using Stateflows.Actions;
using Stateflows.Common.Initializer;
using Stateflows.Common.Registration.Interfaces;
using Stateflows.MAF.AIAgents.Registration;
using Stateflows.MAF.AIAgents.Registration.Builders;

namespace Stateflows.MAF.AIAgents
{
    public static class AIAgentsDependencyInjection
    {
        [DebuggerHidden]
        public static IStateflowsBuilder AddAIAgents(this IStateflowsBuilder stateflowsBuilder, AgentsBuildAction buildAction)
            => AddAIAgents(stateflowsBuilder, buildAction, false);
        
        [DebuggerHidden]
        internal static IStateflowsBuilder AddAIAgents(this IStateflowsBuilder stateflowsBuilder, AgentsBuildAction buildAction, bool systemRegistrations)
            => stateflowsBuilder
                .EnsureAgentsServices()
                .AddActions(b => buildAction.Invoke(new AIAgentsBuilder(b, systemRegistrations)));

        [DebuggerHidden]
        public static IStateflowsBuilder AddDefaultInstance<TAgent>(this IStateflowsBuilder stateflowsBuilder, DefaultInstanceInitializationRequestFactoryAsync initializationRequestFactoryAsync = null)
            where TAgent : class, IAIAgent
            => stateflowsBuilder.AddDefaultInstance(new AgentClass(Agent<TAgent>.Name).BehaviorClass, initializationRequestFactoryAsync);

        private static IStateflowsBuilder EnsureAgentsServices(this IStateflowsBuilder stateflowsBuilder)
        {
            //stateflowsBuilder
                // .ServiceCollection
                // .AddScoped<IAgentContextProvider, AgentContextProvider>()
                // .AddScoped<IEventProcessor, Processor>()
                
                // .AddTransient(_ =>
                //     AgentsContextHolder.ExecutionContext.Value ??
                //     throw new InvalidOperationException($"No service for type '{typeof(IExecutionContext).FullName}' is available in this context.")
                // )
                ;
            
            return stateflowsBuilder;
        }
    }
}

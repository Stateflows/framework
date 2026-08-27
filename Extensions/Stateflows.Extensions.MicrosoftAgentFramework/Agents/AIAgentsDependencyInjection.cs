using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Actions;
using Stateflows.Common.Initializer;
using Stateflows.Common.Registration.Interfaces;
using Stateflows.Extensions.MicrosoftAgentFramework;
using Stateflows.MAF.AIAgents.Registration;
using Stateflows.MAF.AIAgents.Registration.Builders;

namespace Stateflows.MAF.AIAgents
{
    public static class AIAgentsDependencyInjection
    {
        [DebuggerHidden]
        public static IStateflowsBuilder AddAIAgents(this IStateflowsBuilder stateflowsBuilder,
            AgentsBuildAction buildAction)
            => AddAIAgents(stateflowsBuilder, buildAction, false);

        [DebuggerHidden]
        internal static IStateflowsBuilder AddAIAgents(this IStateflowsBuilder stateflowsBuilder, AgentsBuildAction buildAction, bool systemRegistrations)
            => stateflowsBuilder
                .EnsureAgentsServices()
                .AddActions(b => buildAction.Invoke(new AIAgentsBuilder(b, systemRegistrations)));

        [DebuggerHidden]
        public static IStateflowsBuilder AddDefaultInstance<TAgent>(this IStateflowsBuilder stateflowsBuilder, DefaultInstanceInitializationRequestFactoryAsync initializationRequestFactoryAsync = null)
            where TAgent : class, IAIAgent
            => stateflowsBuilder.AddDefaultInstance(new AIAgentClass(AIAgent<TAgent>.Name).BehaviorClass, initializationRequestFactoryAsync);

        private static IStateflowsBuilder EnsureAgentsServices(this IStateflowsBuilder stateflowsBuilder)
        {
            stateflowsBuilder.ServiceCollection.AddMicrosoftAgentFrameworkServices();
            
            return stateflowsBuilder;
        }
    }
}

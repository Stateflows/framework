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
    /// <summary>Provides registration methods for Microsoft Agent Framework agents.</summary>
    public static class AIAgentsDependencyInjection
    {
        /// <summary>Registers agent actions and applies the supplied agent registrations.</summary>
        /// <param name="stateflowsBuilder">The Stateflows builder to configure.</param>
        /// <param name="buildAction">Registers agents using the agent collection builder.</param>
        /// <returns>The same Stateflows builder, for further configuration.</returns>
        [DebuggerHidden]
        public static IStateflowsBuilder AddAIAgents(this IStateflowsBuilder stateflowsBuilder,
            AgentsBuildAction buildAction)
            => AddAIAgents(stateflowsBuilder, buildAction, false);

        [DebuggerHidden]
        internal static IStateflowsBuilder AddAIAgents(this IStateflowsBuilder stateflowsBuilder, AgentsBuildAction buildAction, bool systemRegistrations)
            => stateflowsBuilder
                .EnsureAgentsServices()
                .AddActions(b => buildAction.Invoke(new AIAgentsBuilder(b, systemRegistrations)));

        /// <summary>Registers a default instance of a typed Microsoft Agent Framework agent for startup initialization.</summary>
        /// <typeparam name="TAgent">The agent behavior type whose default instance is initialized.</typeparam>
        /// <param name="stateflowsBuilder">The Stateflows builder to configure.</param>
        /// <param name="initializationRequestFactoryAsync">Optionally creates the initialization request for the default instance.</param>
        /// <returns>The same Stateflows builder, for further configuration.</returns>
        [DebuggerHidden]
        public static IStateflowsBuilder AddDefaultInstance<TAgent>(this IStateflowsBuilder stateflowsBuilder, DefaultInstanceInitializationRequestFactoryAsync initializationRequestFactoryAsync = null)
            where TAgent : class, IAIAgent
            => stateflowsBuilder.AddDefaultInstance(new AIAgentClass(AIAgent<TAgent>.Name).BehaviorClass, initializationRequestFactoryAsync);

        private static IStateflowsBuilder EnsureAgentsServices(this IStateflowsBuilder stateflowsBuilder)
        {
            stateflowsBuilder.AddAIAgents();
            
            return stateflowsBuilder;
        }
    }
}

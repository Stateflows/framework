using System.Reflection;
using System.Diagnostics;
using Stateflows.Actions;
using Stateflows.Common.Extensions;
using Stateflows.Common.Interfaces;
using Stateflows.MAF.AIAgents.Classes;

namespace Stateflows.MAF.AIAgents.Registration.Builders
{
    internal class AIAgentsBuilder : IAIAgentsBuilder
    {
        private readonly IActionsBuilder ActionsBuilder;
        private readonly bool SystemRegistrations;

        public AIAgentsBuilder(IActionsBuilder actionsBuilder, bool systemRegistrations)
        {
            ActionsBuilder = actionsBuilder;
            SystemRegistrations = systemRegistrations;
        }

        [DebuggerHidden]
        public IAIAgentsBuilder AddFromAssembly(Assembly assembly)
        {
            assembly.GetAttributedTypes<AIAgentBehaviorAttribute>().ToList().ForEach(@type =>
            {
                if (typeof(IAIAgent).IsAssignableFrom(@type))
                {
                    var attribute = @type.GetCustomAttribute<AIAgentBehaviorAttribute>();
                    
                }
            });

            return this;
        }

        [DebuggerHidden]
        public IAIAgentsBuilder AddFromAssemblies(IEnumerable<Assembly> assemblies)
        {
            foreach (var assembly in assemblies)
            {
                AddFromAssembly(assembly);
            }

            return this;
        }


        [DebuggerHidden]
        public IAIAgentsBuilder AddFromLoadedAssemblies()
            => AddFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());

        [DebuggerHidden]
        public IAIAgentsBuilder AddAIAgent(string agentName, AIAgentFactoryAsync aiAgentFactoryAsync, AgentBuildAction? agentBuildAction = null)
            => AddAIAgent(agentName, 1, aiAgentFactoryAsync, agentBuildAction);

        [DebuggerHidden]
        public IAIAgentsBuilder AddAIAgent(string agentName, int version, AIAgentFactoryAsync aiAgentFactoryAsync, AgentBuildAction? agentBuildAction = null)
        {
            ActionsBuilder.AddAction<AIAgentAction>(agentName, version, b => b
                .AddConfiguration(aiAgentFactoryAsync)
                .AddConfiguration(agentBuildAction)
            );

            return this;
        }

        [DebuggerHidden]
        public IAIAgentsBuilder AddAIAgent<TAgent>(string? agentName = null, int version = 1, AgentBuildAction? agentBuildAction = null)
            where TAgent : class, IAIAgent
        {
            ActionsBuilder.AddAction<AIAgentAction<TAgent>>(
                agentName,
                version,
                b => b
                    .AddConfiguration(agentBuildAction)
                    .SetCustomBehaviorClassType(MAFBehaviorType.AIAgent)
            );

            return this;
        }

        // #region Observability
        // [DebuggerHidden]
        // public IAgentBuilder AddInterceptor<TInterceptor>()
        //     where TInterceptor : class, IAgentInterceptor
        // {
        //     Register.AddInterceptor<TInterceptor>();
        //
        //     return this;
        // }
        //
        // [DebuggerHidden]
        // public IAgentBuilder AddInterceptor(AgentInterceptorFactoryAsync interceptorFactoryAsync)
        // {
        //     Register.AddInterceptor(interceptorFactoryAsync);
        //
        //     return this;
        // }
        //
        // [DebuggerHidden]
        // public IAgentBuilder AddExceptionHandler<TExceptionHandler>()
        //     where TExceptionHandler : class, IAgentExceptionHandler
        // {
        //     Register.AddExceptionHandler<TExceptionHandler>();
        //
        //     return this;
        // }
        //
        // [DebuggerHidden]
        // public IAgentBuilder AddExceptionHandler(AgentExceptionHandlerFactoryAsync exceptionHandlerFactoryAsync)
        // {
        //     Register.AddExceptionHandler(exceptionHandlerFactoryAsync);
        //
        //     return this;
        // }
        // #endregion
    }
}

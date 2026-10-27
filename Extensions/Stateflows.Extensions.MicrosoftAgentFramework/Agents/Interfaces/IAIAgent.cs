using System.Reflection;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Stateflows.Activities;
using Stateflows.Common.Classes;
using Stateflows.Common.Extensions;
using Stateflows.StateMachines;

namespace Stateflows.MAF.AIAgents
{
    /// <summary>Describes a Microsoft Agent Framework agent that can be built for a Stateflows execution context.</summary>
    public interface IAIAgent
    {
        /// <summary>Gets the optional name supplied to the framework agent.</summary>
        string? Name => null;
        /// <summary>Gets the optional description supplied to the framework agent.</summary>
        string? Description => null;
        /// <summary>Gets the optional initial prompt supplied to the framework agent.</summary>
        string? InitialPrompt => null;

        /// <summary>Builds the framework agent using the Stateflows context and integration tools.</summary>
        /// <param name="aiAgentContext">The current agent behavior and activity input/output context.</param>
        /// <param name="frameworkTools">Tools made available by the Stateflows integration.</param>
        /// <returns>A task producing the framework agent instance.</returns>
        Task<AIAgent> BuildAgentAsync(IAIAgentContext aiAgentContext, AITool[] frameworkTools);
    }

    /// <summary>Agent contract that formats consumed events of a specific CLR type as chat messages.</summary>
    /// <typeparam name="TEvent">The consumed event type.</typeparam>
    public interface IEventConsumerAIAgent<in TEvent> : IAIAgent
    {
        /// <summary>Formats one consumed event for the agent's conversation.</summary>
        /// <param name="aiAgentContext">The agent context used to access Stateflows services and inputs.</param>
        /// <param name="event">The consumed event instance.</param>
        /// <returns>A task containing the chat message representing the event.</returns>
        static abstract Task<ChatMessage> FormatEventAsync(IAIAgentContext aiAgentContext, TEvent @event);
    }

    /// <summary>Agent contract that formats consumed tokens of a specific CLR type as chat messages.</summary>
    /// <typeparam name="TToken">The consumed token type.</typeparam>
    public interface ITokenConsumerAIAgent<in TToken> : IAIAgent
    {
        /// <summary>Formats one consumed token for the agent's conversation.</summary>
        /// <param name="aiAgentContext">The agent context used to access Stateflows services and inputs.</param>
        /// <param name="token">The consumed token instance.</param>
        /// <returns>A task containing the chat message representing the token.</returns>
        static abstract Task<ChatMessage> FormatTokenAsync(IAIAgentContext aiAgentContext, TToken token);
    }

    /// <summary>Agent contract that applies its own registration-time configuration through an agent builder.</summary>
    public interface IAIAgentConfiguration : IAIAgent
    {
        /// <summary>Configures the agent's registered name, prompt, event formatters, and token formatters.</summary>
        /// <param name="builder">The agent action builder.</param>
        void Configure(IAIAgentBuilder builder);
    }

    /// <summary>Agent contract that creates a specified Microsoft Agent Framework agent type through dependency injection.</summary>
    /// <typeparam name="TAIAgent">The concrete framework agent type to instantiate.</typeparam>
    public interface IAIAgent<TAIAgent> : IAIAgent
        where TAIAgent : AIAgent, new()
    {
        /// <summary>Builds the configured framework agent using the agent context's service provider.</summary>
        /// <param name="aiAgentContext">The current agent execution context.</param>
        /// <returns>A task containing the framework agent.</returns>
        Task<AIAgent> BuildAgentAsync(IAIAgentContext aiAgentContext)
            => Task.FromResult<AIAgent>(StateflowsActivator.CreateClassInstance<TAIAgent>(aiAgentContext.ServiceProvider));
    }

    // public interface IChatCompletionAiAgent : IAiAgent<ChatCompletionAgent>
    // {
    //     Task<Agent> IAIAgent.BuildAgentAsync(Kernel kernel, IAIAgentContext iaiAgentContext)
    //         => Task.FromResult<Agent>(new ChatCompletionAgent()
    //         {
    //             Name = Name,
    //             Description = Description,
    //             Instructions = Instructions,
    //             Kernel = kernel,
    //             Arguments = KernelArguments,
    //         });
    // }

    /// <summary>Provides identity and registration helpers for an agent behavior type.</summary>
    /// <typeparam name="TAIAgent">The type implementing <see cref="IAIAgent"/>.</typeparam>
    public static class AIAgent<TAIAgent>
        where TAIAgent : class, IAIAgent
    {
        /// <summary>Gets the registered name from <see cref="AIAgentBehaviorAttribute"/> or the type's readable name.</summary>
        public static string Name
        {
            get
            {
                var agentType = typeof(TAIAgent);
                var attribute = agentType.GetCustomAttribute<AIAgentBehaviorAttribute>();
                return attribute is { Name: not null }
                    ? attribute.Name
                    : agentType.GetReadableName(TypedElements.Actions);
            }
        }

        /// <summary>Creates the behavior class identity for the agent type.</summary>
        /// <returns>The agent's Stateflows behavior class.</returns>
        public static BehaviorClass ToClass()
            => new(MAFBehaviorType.AIAgent, Name);

        /// <summary>Creates an agent behavior identifier for a specific instance.</summary>
        /// <param name="instance">The agent instance key; an empty value identifies the default instance.</param>
        /// <returns>The agent's Stateflows behavior identifier.</returns>
        public static BehaviorId ToId(string instance)
            => new(ToClass(), instance);
    }

    /// <summary>Marker state type for a state-machine state that hosts a typed agent.</summary>
    /// <typeparam name="TAIAgent">The agent behavior type hosted by the state.</typeparam>
    public sealed class AgenticState<TAIAgent> : IState
        where TAIAgent : class, IAIAgent;
    
    //public static class AgenticState
    //{
    //    public static string GetName(Type aiAgentType) => State.GetName(typeof(AgenticState<>).MakeGenericType(aiAgentType));
    //}
    
    /// <summary>Marker node type for an activity node that hosts a typed agent conversation.</summary>
    /// <typeparam name="TAIAgent">The agent behavior type hosted by the activity node.</typeparam>
    public sealed class AgenticActivity<TAIAgent> : IStructuredActivityNode
        where TAIAgent : class, IAIAgent;
    
    //public static class AgenticActivity
    //{
    //    public static string GetName(Type aiAgentType) => State.GetName(typeof(AgenticActivity<>).MakeGenericType(aiAgentType));
    //}
}

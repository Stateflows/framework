using System.Reflection;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Stateflows.Common.Classes;
using Stateflows.Common.Extensions;

namespace Stateflows.MAF.AIAgents
{
    public interface IAIAgent
    {
        string? Name { get; }
        string? Description { get; }
        string? Instructions { get; }
        string? Arguments { get; }
        string? Template { get; }
        string? InitialPrompt { get; }

        Task<AIAgent> BuildAgentAsync(IAIAgentContext aiAgentContext);
    }

    public interface IEventConsumerAIAgent<in TEvent> : IAIAgent
    {
        static abstract Task<ChatMessage> FormatEventAsync(IAIAgentContext aiAgentContext, TEvent @event);
    }

    public interface ITokenConsumerAiAgent<in TToken> : IAIAgent
    {
        static abstract Task<ChatMessage> FormatTokenAsync(IAIAgentContext aiAgentContext, TToken token);
    }

    public interface IAIAgentConfiguration : IAIAgent
    {
        void Configure(IAIAgentBuilder builder);
    }

    public interface IAiAgent<TAIAgent> : IAIAgent
        where TAIAgent : AIAgent, new()
    {
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

    public static class Agent<TAgent>
        where TAgent : class, IAIAgent
    {
        public static string Name
        {
            get
            {
                var agentType = typeof(TAgent);
                var attribute = agentType.GetCustomAttribute<AIAgentBehaviorAttribute>();
                return attribute is { Name: not null }
                    ? attribute.Name
                    : agentType.GetReadableName(TypedElements.Actions);
            }
        }

        public static BehaviorClass ToClass()
            => new(MAFBehaviorType.AIAgent, Name);

        public static BehaviorId ToId(string instance)
            => new(ToClass(), instance);
    }
}

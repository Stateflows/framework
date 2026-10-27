using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Stateflows.Activities;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Common;
using Stateflows.Common.Classes;
using Stateflows.Common.Interfaces;
using Stateflows.Extensions.MicrosoftAgentFramework.AIAgents.Classes;
using Stateflows.MAF.AIAgents.Registration;

namespace Stateflows.MAF.AIAgents.Classes;

/// <summary>Runs a framework agent within an activity action node and emits its response as an output token.</summary>
/// <param name="activityContext">The activity context used to resolve services and access the current behavior.</param>
/// <param name="agentResponses">The output-token sink for generated agent responses.</param>
/// <param name="chatMessages">The incoming chat message tokens supplied to the agent.</param>
/// <param name="stringMessages">The incoming string tokens converted to user chat messages.</param>
public class AIAgentActionNode(
    IActivityContext activityContext,
    IOutputTokens<AgentResponse> agentResponses,
    IInputTokens<ChatMessage> chatMessages,
    IInputTokens<string> stringMessages
) : IActionNode,
    IConfigurable<AIAgentFactoryAsync>,
    IConfigurable<AIAgentBuildAction?>
{
    /// <summary>Creates an agent, runs it with the available chat or string inputs, and adds its response token.</summary>
    /// <param name="cancellationToken">A token that can cancel action execution.</param>
    /// <returns>A task that completes after the response is added to the output token stream.</returns>
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        // const string agentThreadKey = "system::agentThread";
        // var agentThread = await activityContext.Values.GetOrDefaultAsync(agentThreadKey, new ChatHistoryAgentThread());
        // var kernelBuilder = Kernel.CreateBuilder();
        // kernelBuilder.Services.AddSingleton<IFunctionInvocationFilter, ApprovalFilterExample>();
        var agent = await AIAgentFactoryAsync(activityContext.ServiceProvider, []);
        // var session = await agent.CreateSessionAsync(cancellationToken: cancellationToken);
        
        // var sessionDataString = await activityContext.Values.GetOrDefaultAsync(agentThreadKey, string.Empty);
        // var sessionData = JsonElement.Parse(sessionDataString ?? string.Empty);
        // await agent.DeserializeSessionAsync(sessionData, cancellationToken: cancellationToken);
        
        var response = chatMessages.Any()
            ? await agent.RunAsync(chatMessages.ToArray())
            : stringMessages.Any()
                ? await agent.RunAsync(stringMessages.Select(s => new ChatMessage(ChatRole.User, s)).ToArray())
                : await agent.RunAsync();
                
        // await foreach (var response in responseStream.WithCancellation(cancellationToken))
        {
            agentResponses.Add(response);
            // activityContext.Publish(response);
        }
        
        // sessionData = await agent.SerializeSessionAsync(session, cancellationToken: cancellationToken);
        // sessionDataString = sessionData.GetString();
        // await activityContext.Values.SetAsync(agentThreadKey, sessionDataString);
                
        // await activityContext.Values.SetAsync(agentThreadKey, agentThread);
    }

    /// <summary>Gets the factory used to create the framework agent for this action node.</summary>
    protected virtual AIAgentFactoryAsync AIAgentFactoryAsync { get; private set; } = null!;

    // protected virtual AgentThreadFactoryAsync AgentThreadFactoryAsync { get; private set; } =
    //     chatHistory => Task.FromResult<AgentThread>(new ChatHistoryAgentThread(chatHistory));

    AIAgentFactoryAsync IConfigurable<AIAgentFactoryAsync>.Configuration
    {
        set => AIAgentFactoryAsync = value;
    }

    /// <summary>Gets the optional configuration action supplied during agent registration.</summary>
    protected virtual AIAgentBuildAction? AgentBuildAction { get; private set; }
    AIAgentBuildAction? IConfigurable<AIAgentBuildAction?>.Configuration
    {
        set => AgentBuildAction = value;
    }
}

/// <summary>Creates a typed Stateflows agent behavior and runs it within an activity action node.</summary>
/// <typeparam name="TAgent">The Stateflows agent implementation type.</typeparam>
/// <param name="activityContext">The activity context used to resolve services and access the current behavior.</param>
/// <param name="agentResponses">The output-token sink for generated agent responses.</param>
/// <param name="chatMessages">The incoming chat message tokens supplied to the agent.</param>
/// <param name="stringMessages">The incoming string tokens converted to user chat messages.</param>
/// <param name="input">The activity input used to provide agent-consumed tokens.</param>
/// <param name="output">The activity output used to publish agent outputs.</param>
public class AIAgentActionNode<TAgent>(
    IActivityContext activityContext, 
    IOutputTokens<AgentResponse> agentResponses,
    IInputTokens<ChatMessage> chatMessages,
    IInputTokens<string> stringMessages,
    IInput input,
    IOutput output
)
    : AIAgentActionNode(activityContext, agentResponses, chatMessages, stringMessages),
    ITokenConsumer<string>,
    ITokenConsumer<ChatMessage>
    where TAgent : class, IAIAgent
{
    /// <summary>Creates the typed agent behavior, builds its framework agent, and supplies the available activity tools.</summary>
    protected override AIAgentFactoryAsync AIAgentFactoryAsync
        => async (_, tools) =>
        {
            var agentContext = new AIAgentContext(activityContext, input, output);
            var agentBehavior = await StateflowsActivator.CreateModelElementInstanceAsync<TAgent>(activityContext.ServiceProvider);
            var agent = await agentBehavior.BuildAgentAsync(agentContext, tools);

            return agent;
        };
}
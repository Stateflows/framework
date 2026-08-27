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

public class AIAgentActionNode(
    IActivityContext activityContext,
    IOutputTokens<AgentResponse> agentResponses,
    IInputTokens<ChatMessage> chatMessages,
    IInputTokens<string> stringMessages
) : IActionNode,
    IConfigurable<AIAgentFactoryAsync>,
    IConfigurable<AIAgentBuildAction?>
{
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

    protected virtual AIAgentFactoryAsync AIAgentFactoryAsync { get; private set; } = null!;

    // protected virtual AgentThreadFactoryAsync AgentThreadFactoryAsync { get; private set; } =
    //     chatHistory => Task.FromResult<AgentThread>(new ChatHistoryAgentThread(chatHistory));

    AIAgentFactoryAsync IConfigurable<AIAgentFactoryAsync>.Configuration
    {
        set => AIAgentFactoryAsync = value;
    }

    protected virtual AIAgentBuildAction? AgentBuildAction { get; private set; }
    AIAgentBuildAction? IConfigurable<AIAgentBuildAction?>.Configuration
    {
        set => AgentBuildAction = value;
    }
}

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
    protected override AIAgentFactoryAsync AIAgentFactoryAsync
        => async (_, tools) =>
        {
            var agentContext = new AIAgentContext(activityContext, input, output);
            var agentBehavior = await StateflowsActivator.CreateModelElementInstanceAsync<TAgent>(activityContext.ServiceProvider);
            var agent = await agentBehavior.BuildAgentAsync(agentContext, tools);

            return agent;
        };
}
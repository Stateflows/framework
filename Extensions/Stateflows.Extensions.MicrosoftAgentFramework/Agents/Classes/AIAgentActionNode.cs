using System.Text.Json;
using Microsoft.Extensions.AI;
using Stateflows.Activities;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Common;
using Stateflows.Common.Classes;
using Stateflows.Common.Interfaces;
using Stateflows.Extensions.MicrosoftAgentFramework.AIAgents.Classes;
using Stateflows.MAF.AIAgents.Registration;

namespace Stateflows.MAF.AIAgents.Classes;

public class AIAgentActionNode(IActivityContext activityContext, IInput input, IOutput output) : IActionNode, IConfigurable<AIAgentFactoryAsync>, IConfigurable<AgentBuildAction?>
{
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        const string agentThreadKey = "system::agentThread";
        // var agentThread = await activityContext.Values.GetOrDefaultAsync(agentThreadKey, new ChatHistoryAgentThread());
        // var kernelBuilder = Kernel.CreateBuilder();
        // kernelBuilder.Services.AddSingleton<IFunctionInvocationFilter, ApprovalFilterExample>();
        var agent = await AIAgentFactoryAsync(activityContext.ServiceProvider);
        var session = await agent.CreateSessionAsync(cancellationToken: cancellationToken);
        
        var sessionDataString = await activityContext.Values.GetOrDefaultAsync(agentThreadKey, string.Empty);
        var sessionData = JsonElement.Parse(sessionDataString ?? string.Empty);
        await agent.DeserializeSessionAsync(sessionData, cancellationToken: cancellationToken);
        
        var responseStream = input.HasTokensOfType<string>()
            ? agent.RunStreamingAsync(input.GetTokensOfType<string>().First(), cancellationToken: cancellationToken)
            : input.HasTokensOfType<ChatMessage>()
                ? agent.RunStreamingAsync(input.GetTokensOfType<ChatMessage>().ToArray(), cancellationToken: cancellationToken)
                : agent.RunStreamingAsync(cancellationToken: cancellationToken);
                
        await foreach (var response in responseStream.WithCancellation(cancellationToken))
        {
            activityContext.Publish(response);
        }
        
        sessionData = await agent.SerializeSessionAsync(session, cancellationToken: cancellationToken);
        sessionDataString = sessionData.GetString();
        await activityContext.Values.SetAsync(agentThreadKey, sessionDataString);
                
        // await activityContext.Values.SetAsync(agentThreadKey, agentThread);
    }

    protected virtual AIAgentFactoryAsync AIAgentFactoryAsync { get; private set; } = null!;

    // protected virtual AgentThreadFactoryAsync AgentThreadFactoryAsync { get; private set; } =
    //     chatHistory => Task.FromResult<AgentThread>(new ChatHistoryAgentThread(chatHistory));

    AIAgentFactoryAsync IConfigurable<AIAgentFactoryAsync>.Configuration
    {
        set => AIAgentFactoryAsync = value;
    }

    protected virtual AgentBuildAction? AgentBuildAction { get; private set; }
    AgentBuildAction? IConfigurable<AgentBuildAction?>.Configuration
    {
        set => AgentBuildAction = value;
    }
}

public class AIAgentActionNode<TAgent>(IActivityContext activityContext, IInput input, IOutput output)
    : AIAgentActionNode(activityContext, input, output),
    ITokenConsumer<string>,
    ITokenConsumer<AgenticChatMessage>
    where TAgent : class, IAIAgent
{
    protected override AIAgentFactoryAsync AIAgentFactoryAsync
        => async _ =>
        {
            var agentContext = new AIAgentContext(activityContext, input, output);
            var agentBehavior = await StateflowsActivator.CreateModelElementInstanceAsync<TAgent>(activityContext.ServiceProvider);
            var agent = await agentBehavior.BuildAgentAsync(agentContext);

            return agent;
        };
}
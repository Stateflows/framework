using Stateflows.Common;
using Stateflows.Common.Classes;
using Stateflows.Common.Interfaces;
using Stateflows.Extensions.MicrosoftAgentFramework.AIAgents.Classes;
using Stateflows.MAF.AIAgents.Registration;
using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.AI;
using Stateflows.Actions;
using Stateflows.Extensions.MicrosoftAgentFramework.Agents.Classes;

namespace Stateflows.MAF.AIAgents.Classes;

internal class AIAgentAction(
    IActionContext actionContext,
    IBehaviorContext behaviorContext,
    IExecutionContext executionContext) : IAction,
    IConfigurable<AIAgentFactoryAsync>,
    IConfigurable<AgentBuildAction?>,
    IConfigurable<IMetadataBuilder>
    // IEventConsumer<string>,
    // IEventConsumer<ChatMessage>,
    // IEventConsumer<AgenticChatMessage>,
    // IEventConsumer<AgenticChatInquiry>,
    // IEventProducer<AgenticChatMessage>
{
    public Dictionary<Type, ChatMessageConverterHolder> EventFormatters { get; } = new();
    public Dictionary<Type, ChatMessageConverterHolder> TokenFormatters { get; } = new();
    public string? InitialPrompt { get; set; }
    public virtual async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        const string agentThreadKey = "system::agentThread";
        
        // var chatHistory = await actionContext.Values.GetOrDefaultAsync(agentThreadKey, new ChatHistory());

        // if (Metadata.Metadata.TryGetValue(AIAgentConstants.Transitions, out var agenticTransitionsObj))
        // {
        //     var agenticTransitions = agenticTransitionsObj as List<Dictionary<string, object>>;
        //     if (agenticTransitions.Any())
        //     {
        //         kernelBuilder.Plugins.AddFromFunctions(
        //             AIAgentConstants.AgenticWorkflowDecision,
        //             agenticTransitions.Select(agenticTransition =>
        //                 KernelFunctionFactory.CreateFromMethod(
        //                     method: () => actionContext.Values.SetAsync(AIAgentConstants.GuardKey, agenticTransition.GetValueOrDefault(AIAgentConstants.GuardValue) as string),
        //                     functionName: agenticTransition.GetValueOrDefault(AIAgentConstants.TransitionName) as string,
        //                     description: agenticTransition.GetValueOrDefault(AIAgentConstants.TransitionDescription) as string
        //                 )
        //             )
        //         );
        //     }
        // }

        // if (actionContext.HasTokensOfType<AgenticChatInquiry>())
        // {
        //     var inquiry = actionContext.GetTokensOfType<AgenticChatInquiry>().First();
        //     var headers = inquiry.GuardTriggerHolder.Headers;
        //     headers[nameof(TransitionGuardInquiryAcceptance)] = new TransitionGuardInquiryAcceptance();
        //     kernelBuilder.Plugins.AddFromFunctions(
        //         AIAgentConstants.AgenticInquiryTools,
        //         [
        //             KernelFunctionFactory.CreateFromMethod(
        //                 method: () => actionContext.GetType().GetMethod("Send").MakeGenericMethod(inquiry.GuardTriggerHolder.PayloadType).Invoke(actionContext, [inquiry.GuardTriggerHolder.BoxedPayload, headers]),
        //                 functionName: AIAgentConstants.AgenticInquiryAcceptance,
        //                 description: $"Call it if this statement is true: {inquiry.Message.Text}"
        //             )
        //         ]
        //     );
        // }
        
        var agent = await AIAgentFactoryAsync(actionContext.ServiceProvider);
        var session = await agent.CreateSessionAsync(cancellationToken: cancellationToken);
        
        var sessionDataString = await actionContext.Values.GetOrDefaultAsync(agentThreadKey, string.Empty);
        if (!string.IsNullOrWhiteSpace(sessionDataString))
        {
            var sessionData = JsonElement.Parse(sessionDataString);
            await agent.DeserializeSessionAsync(sessionData, cancellationToken: cancellationToken);
        }

        List<ChatMessage> chatMessages = [];
        // chatMessages.AddRange(
        //     TokenFormatters.Values.SelectMany(async formatter => await formatter.ConvertAsync(agentContext))
        //     actionContext.GetTokens()
        //         .Where(t => TokenFormatters.ContainsKey(t.GetType()))
        //         .Select(t => TokenFormatters[t.GetType()](agentContext, t))
        //         .Select(async s => 
        //             new ChatMessage
        //             {
        //                 Role = ChatRole.User,
        //                 Contents = [ (await s).Content ],
        //             }
        //         )
        //         .Select(c => c.Result)
        //         .ToArray()
        // );
        
        chatMessages.AddRange(
            actionContext.GetTokensOfType<string>()
                .Select(s => 
                    new ChatMessage
                    {
                        Role = ChatRole.User,
                        Contents = [ new TextContent(s)]
                    }
                )
        );
        
        chatMessages.AddRange(actionContext.GetTokensOfType<ChatMessage>());
        
        chatMessages.AddRange(actionContext.GetTokensOfType<AgenticChatInquiry>().Select(t =>
            new ChatMessage
            {
                Role = ChatRole.User,
                Contents = [ new TextContent($"There is the inquiry about the statement: {GetInquiryText(t.Message)}") ]
            }
        ));
        
        chatMessages.AddRange(actionContext.GetTokensOfType<AgenticChatMessage>().Select(t => t.Message));

        if (!chatMessages.Any() && InitialPrompt != null)
        {
            chatMessages.Add(
                new ChatMessage
                {
                    Role = ChatRole.User,
                    AuthorName = "User",
                    Contents = [ new TextContent(InitialPrompt) ]
                }
            );
        }
        
        // chatHistory.AddRange(chatMessages);

        if (chatMessages.Any())
        {
            Debug.WriteLine($">>> user: {chatMessages.First().Contents?.FirstOrDefault()}");
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        var responseStream = agent.RunStreamingAsync(chatMessages, cancellationToken: cancellationToken);

        try
        {
            await foreach (var response in responseStream)
            {
                Debug.WriteLine($">>> agent: '{response.Text}'");
                
                actionContext.Publish(response);
            }
            
            var sessionData = await agent.SerializeSessionAsync(session, cancellationToken: cancellationToken);
            await actionContext.Values.SetAsync(agentThreadKey, sessionData.GetRawText());
        }
        catch (TaskCanceledException)
        {
            Debug.WriteLine("AIAgent execution was cancelled.");
        }
        catch (OperationCanceledException)
        {
            Debug.WriteLine("AIAgent execution was cancelled.");
        }
    }

    private static string GetInquiryText(ChatMessage message)
        => string.IsNullOrWhiteSpace(message.Text)
            ? message.ToString()
            : message.Text;

    protected virtual AIAgentFactoryAsync AIAgentFactoryAsync { get; private set; } = null!;

    AIAgentFactoryAsync IConfigurable<AIAgentFactoryAsync>.Configuration
    {
        set => AIAgentFactoryAsync = value;
    }

    protected AgentBuildAction? AgentBuildAction { get; private set; }

    AgentBuildAction? IConfigurable<AgentBuildAction?>.Configuration
    {
        set => AgentBuildAction = value;
    }

    protected IMetadataBuilder Metadata { get; private set; } = null!;

    IMetadataBuilder IConfigurable<IMetadataBuilder>.Configuration
    {
        set => Metadata = value;
    }
}

internal class AIAgentAction<TAgent>(
    IActionContext actionContext,
    IBehaviorContext behaviorContext,
    IExecutionContext executionContext
) : AIAgentAction(actionContext, behaviorContext, executionContext)
    where TAgent : class, IAIAgent
{
    protected override AIAgentFactoryAsync AIAgentFactoryAsync
        => async serviceProvider =>
        {
            var agentContext = new AIAgentContext(actionContext, actionContext, actionContext);
            var agentBehavior = await StateflowsActivator.CreateModelElementInstanceAsync<TAgent>(actionContext.ServiceProvider);
            if (agentBehavior.InitialPrompt != null && InitialPrompt == null)
            {
                InitialPrompt = agentBehavior.InitialPrompt;
            }
            var agent = await agentBehavior.BuildAgentAsync(agentContext);

            return agent;
        };
}
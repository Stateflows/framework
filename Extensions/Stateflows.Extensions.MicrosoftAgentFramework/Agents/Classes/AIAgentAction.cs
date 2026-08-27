using Stateflows.Common;
using Stateflows.Common.Classes;
using Stateflows.Common.Interfaces;
using Stateflows.Extensions.MicrosoftAgentFramework.AIAgents.Classes;
using Stateflows.MAF.AIAgents.Registration;
using System.Diagnostics;
using System.Text.Json;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Actions;
using Stateflows.Extensions.MicrosoftAgentFramework.Agents.Classes;
using Stateflows.MAF.AIAgents.Events;

namespace Stateflows.MAF.AIAgents.Classes;

internal class AIAgentAction(
    IServiceProvider serviceProvider,
    IActionContext actionContext,
    IBehaviorContext behaviorContext,
    IExecutionContext executionContext) : IAction,
    IConfigurable<AIAgentFactoryAsync>,
    IConfigurable<AIAgentBuildAction?>,
    IConfigurable<IMetadataBuilder>,
    // IEventConsumer<string>,
    // IEventConsumer<ChatMessage>,
    IEventConsumer<AgenticMessage>,
    // IEventConsumer<AgenticChatInquiry>,
    IEventProducer<AgenticMessage>
    // IEventProducer<AgentResponseUpdate>
{
    public Dictionary<Type, ChatMessageConverterHolder> EventFormatters { get; } = new();
    public Dictionary<Type, ChatMessageConverterHolder> TokenFormatters { get; } = new();
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? InitialPrompt { get; set; }

    private IOwnerBehaviorContext? _ownerBehaviorContext = null;
    private bool _ownerBehaviorContextSet = false;
    private IOwnerBehaviorContext? OwnerBehaviorContext
    {
        get
        {
            if (!_ownerBehaviorContextSet)
            {
                try
                {
                    _ownerBehaviorContext ??= serviceProvider.GetService<IOwnerBehaviorContext>();
                }
                catch (Exception) { }
                finally
                {
                    _ownerBehaviorContextSet = true;
                }
            }

            return _ownerBehaviorContext;
        }
    }

    public virtual async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        const string sessionDataKey = "system::aiAgentSession";
        
        var tools = new List<AITool>();

        if (Metadata is not null && Metadata.Metadata.TryGetValue(AIAgentConstants.Transitions, out var agenticTransitionsObj))
        {
            var agenticTransitions = agenticTransitionsObj as List<Dictionary<string, object>> ?? [];
            if (agenticTransitions.Any())
            {
                tools.AddRange(agenticTransitions
                    .Select(agenticTransition =>
                        AIFunctionFactory.Create(
                            () => OwnerBehaviorContext?.Send(new AgenticDecision()
                            {
                                DecisionMarker = agenticTransition.GetValueOrDefault(AIAgentConstants.GuardValue) as string
                            }),
                            // () => actionContext.Values.SetAsync(AIAgentConstants.GuardKey, agenticTransition.GetValueOrDefault(AIAgentConstants.GuardValue) as string),
                            new AIFunctionFactoryOptions()
                            {
                                Name = agenticTransition.GetValueOrDefault(AIAgentConstants.TransitionName) as string,
                                Description = agenticTransition.GetValueOrDefault(AIAgentConstants.TransitionDescription) as string
                            }
                        )
                    )
                );
            }
        }

        if (actionContext.HasTokensOfType<AgenticChatInquiry>())
        {
            var inquiry = actionContext.GetTokensOfType<AgenticChatInquiry>().First();
            var headers = inquiry.GuardTriggerHolder.Headers;
            headers[nameof(TransitionGuardInquiryAcceptance)] = new TransitionGuardInquiryAcceptance();

            tools.Add(AIFunctionFactory.Create(() => OwnerBehaviorContext is not null
                    ? OwnerBehaviorContext!.GetType().GetMethod("Send").MakeGenericMethod(inquiry.GuardTriggerHolder.PayloadType).Invoke(actionContext, [inquiry.GuardTriggerHolder.BoxedPayload, headers])
                    : Task.CompletedTask,
                new AIFunctionFactoryOptions()
                {
                    Name = AIAgentConstants.AgenticInquiryAcceptance,
                    Description = $"Call it if this statement is true: {inquiry.Message.Text}"
                }
            ));
        }
        
        var agent = await AIAgentFactoryAsync(actionContext.ServiceProvider, tools.ToArray());
        var session = await agent.CreateSessionAsync(cancellationToken: cancellationToken);

        var sessionLoaded = false;
        var sessionDataString = string.Empty;
        if (OwnerBehaviorContext is not null)
        {
            (sessionLoaded, sessionDataString) = await OwnerBehaviorContext.TryGetAsync<string>(nameof(IAIAgentSessionEntity.AIAgentSessionData));
        }
        if (!sessionLoaded)
        {
            sessionDataString = await actionContext.Values.GetOrDefaultAsync(sessionDataKey, string.Empty);
        }
        
        if (!string.IsNullOrWhiteSpace(sessionDataString))
        {
            var sessionData = JsonElement.Parse(sessionDataString);
            await agent.DeserializeSessionAsync(sessionData, cancellationToken: cancellationToken);
            
            sessionLoaded = true;
        }
        else
        {
            sessionLoaded = false;
        }

        List<ChatMessage> chatMessages = [];
        
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
        
        // chatMessages.AddRange(actionContext.GetTokensOfType<ChatMessage>());
        
        // chatMessages.AddRange(actionContext.GetTokensOfType<AgenticChatInquiry>().Select(t =>
        //     new ChatMessage
        //     {
        //         Role = ChatRole.User,
        //         Contents = [ new TextContent($"There is the inquiry about the statement: {GetInquiryText(t.Message)}") ]
        //     }
        // ));
        
        chatMessages.AddRange(actionContext
            .GetTokensOfType<AgenticMessage>()
            .Select(t => t.ToChatMessage())
        );

        if (InitialPrompt != null && actionContext.HasTokensOfType<Initialize>())
        {
            chatMessages.Add(
                new ChatMessage
                {
                    Role = ChatRole.System,
                    AuthorName = "System",
                    Contents = [ new TextContent(InitialPrompt) ]
                }
            );
        }

        if (chatMessages.Any())
        {
            foreach (var chatMessage in chatMessages)
            {
                foreach (var chatMessageContent in chatMessage.Contents)
                {
                    Debug.WriteLine($">>> user: {chatMessageContent}");
                }
            }
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        if (!chatMessages.Any())
        {
            return;
        }

        var response = await (
            chatMessages.Any()
                ? agent.RunAsync(chatMessages, session)
                : agent.RunAsync(session)
        );

        try
        {
            Debug.WriteLine($">>> agent: '{response.Text}'");

            if (OwnerBehaviorContext is not null)
            {
                OwnerBehaviorContext.PublishRange(response.Messages
                    .Select(AgenticMessage.FromChatMessage)
                    .ToArray()
                );
                
                // OwnerBehaviorContext.Publish(response);
                // OwnerBehaviorContext.Publish(response.Text);
            }
            else
            {
                actionContext.Publish(response);
            }
            
            var sessionData = await agent.SerializeSessionAsync(session, cancellationToken: cancellationToken);
            var sessionSaved = false;
            if (OwnerBehaviorContext is not null)
            {
                sessionSaved = await OwnerBehaviorContext.TrySetAsync(nameof(IAIAgentSessionEntity.AIAgentSessionData), sessionData.GetRawText());
            }

            if (!sessionSaved)
            {
                await actionContext.Values.SetAsync(sessionDataKey, sessionData.GetRawText());
            }
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

    protected AIAgentBuildAction? AgentBuildAction { get; private set; }

    AIAgentBuildAction? IConfigurable<AIAgentBuildAction?>.Configuration
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
    IServiceProvider serviceProvider,
    IActionContext actionContext,
    IBehaviorContext behaviorContext,
    IExecutionContext executionContext
) : AIAgentAction(serviceProvider, actionContext, behaviorContext, executionContext)
    where TAgent : class, IAIAgent
{
    protected override AIAgentFactoryAsync AIAgentFactoryAsync
        => async (serviceProvider, tools) =>
        {
            var agentContext = new AIAgentContext(actionContext, actionContext, actionContext);
            var agentBehavior = await StateflowsActivator.CreateModelElementInstanceAsync<TAgent>(actionContext.ServiceProvider);
            if (agentBehavior.InitialPrompt != null && InitialPrompt == null)
            {
                InitialPrompt = agentBehavior.InitialPrompt;
            }
            var agent = await agentBehavior.BuildAgentAsync(agentContext, tools);

            return agent;
        };
}
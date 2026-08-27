using Microsoft.Extensions.AI;
using Stateflows.Extensions.MicrosoftAgentFramework.Agents.Enums;
using Stateflows.Extensions.MinimalAPIs.Attributes;

namespace Stateflows.MAF;

public class AgenticContent
{
    public AgenticContentType Type { get; set; }
    public string Payload { get; set; }
}

[NotificationWatch<AgenticMessage>]
public class AgenticMessage
{
    public AgenticRole Role { get; set; }
    public AgenticContent[] Contents { get; set; }
    
    public ChatMessage ToChatMessage()
        => new ChatMessage(
            Role switch
            {
                AgenticRole.System => ChatRole.System,
                AgenticRole.Assistant => ChatRole.Assistant,
                AgenticRole.User => ChatRole.User,
                AgenticRole.Tool => ChatRole.Tool,
            },
            Contents.Select<AgenticContent, AIContent>(c => c.Type switch
            {
                AgenticContentType.Data => new DataContent(c.Payload),
                AgenticContentType.Error => new ErrorContent(c.Payload),
                // AgenticContentType.FunctionCall => new FunctionCallContent(c.Payload),
                // AgenticContentType.FunctionResult => new FunctionResultContent(c.Payload),
                AgenticContentType.HostedFile => new HostedFileContent(c.Payload),
                AgenticContentType.HostedVectorStore => new HostedVectorStoreContent(c.Payload),
                // AgenticContentType.Text => new TextContent(c.Payload),
                AgenticContentType.TextReasoning => new TextReasoningContent(c.Payload),
                AgenticContentType.Uri => new UriContent(c.Payload),
                // AgenticContentType.Usage => new UsageContent(c.Payload),
                AgenticContentType.ToolCall => new ToolCallContent(c.Payload),
                AgenticContentType.ToolResult => new ToolResultContent(c.Payload),
                // AgenticContentType.InputRequest => new InputRequestContent(c.Payload),
                // AgenticContentType.InputResponse => new InputResponseContent(c.Payload),
                // AgenticContentType.ToolApprovalRequest => new ToolApprovalRequestContent(c.Payload),
                // AgenticContentType.ToolApprovalResponse => new ToolApprovalResponseContent(c.Payload),
                // AgenticContentType.McpServerToolCall => new McpServerToolCallContent(c.Payload),
                AgenticContentType.McpServerToolResult => new McpServerToolResultContent(c.Payload),
                AgenticContentType.ImageGenerationToolCall => new ImageGenerationToolCallContent(c.Payload),
                AgenticContentType.ImageGenerationToolResult => new ImageGenerationToolResultContent(c.Payload),
                AgenticContentType.CodeInterpreterToolCall => new CodeInterpreterToolCallContent(c.Payload),
                AgenticContentType.CodeInterpreterToolResult => new CodeInterpreterToolResultContent(c.Payload),
                AgenticContentType.WebSearchToolCall => new WebSearchToolCallContent(c.Payload),
                AgenticContentType.WebSearchToolResult => new WebSearchToolResultContent(c.Payload),
                _ => new TextContent(c.Payload)
            }).ToArray()
        );
    
    public static AgenticMessage FromChatMessage(ChatMessage chatMessage)
        => new AgenticMessage
        {
            Role = chatMessage.Role.Value switch
            {
                "system" => AgenticRole.System,
                "assistant" => AgenticRole.Assistant,
                "user" => AgenticRole.User,
                "tool" => AgenticRole.Tool,
                _ => AgenticRole.Unknown
            },
            Contents = chatMessage.Contents.Select(c => new AgenticContent
            {
                Type = c switch
                {
                    DataContent => AgenticContentType.Data,
                    ErrorContent => AgenticContentType.Error,
                    FunctionCallContent => AgenticContentType.FunctionCall,
                    FunctionResultContent => AgenticContentType.FunctionResult,
                    HostedFileContent => AgenticContentType.HostedFile,
                    HostedVectorStoreContent => AgenticContentType.HostedVectorStore,
                    TextContent => AgenticContentType.Text,
                    TextReasoningContent => AgenticContentType.TextReasoning,
                    UriContent => AgenticContentType.Uri,
                    UsageContent => AgenticContentType.Usage,
                    McpServerToolCallContent => AgenticContentType.McpServerToolCall,
                    McpServerToolResultContent => AgenticContentType.McpServerToolResult,
                    ImageGenerationToolCallContent => AgenticContentType.ImageGenerationToolCall,
                    ImageGenerationToolResultContent => AgenticContentType.ImageGenerationToolResult,
                    CodeInterpreterToolCallContent => AgenticContentType.CodeInterpreterToolCall,
                    CodeInterpreterToolResultContent => AgenticContentType.CodeInterpreterToolResult,
                    WebSearchToolCallContent => AgenticContentType.WebSearchToolCall,
                    WebSearchToolResultContent => AgenticContentType.WebSearchToolResult,
                    ToolCallContent => AgenticContentType.ToolCall,
                    ToolResultContent => AgenticContentType.ToolResult,
                    ToolApprovalRequestContent => AgenticContentType.ToolApprovalRequest,
                    ToolApprovalResponseContent => AgenticContentType.ToolApprovalResponse,
                    InputRequestContent => AgenticContentType.InputRequest,
                    InputResponseContent => AgenticContentType.InputResponse,
                    _ => AgenticContentType.Unknown
                },
                Payload = c switch
                {
                    DataContent t => t.ToString(),
                    ErrorContent t => t.Message,
                    FunctionCallContent t => $"{t.CallId}: {t.Name}",
                    FunctionResultContent t => t.CallId,
                    HostedFileContent t => t.FileId,
                    HostedVectorStoreContent t => t.VectorStoreId,
                    TextContent t => t.Text,
                    TextReasoningContent t => t.Text,
                    UriContent t => t.Uri.ToString(),
                    UsageContent t => t.Details.ToString(),
                    McpServerToolCallContent t => $"{t.CallId}: {t.Name}",
                    McpServerToolResultContent t => t.CallId,
                    ImageGenerationToolCallContent t => t.CallId,
                    ImageGenerationToolResultContent t => t.CallId,
                    CodeInterpreterToolCallContent t => t.CallId,
                    CodeInterpreterToolResultContent t => t.CallId,
                    WebSearchToolCallContent t => t.CallId,
                    WebSearchToolResultContent t => t.CallId,
                    ToolCallContent t => t.CallId,
                    ToolResultContent t => t.CallId,
                    ToolApprovalRequestContent t => t.ToolCall.CallId,
                    ToolApprovalResponseContent t => $"{t.ToolCall.CallId}: {(t.Approved ? "approved" : "denied")}",
                    InputRequestContent t => t.RequestId,
                    InputResponseContent t => t.RequestId,
                    _ => c.ToString()
                }
            }).ToArray()
        };
}
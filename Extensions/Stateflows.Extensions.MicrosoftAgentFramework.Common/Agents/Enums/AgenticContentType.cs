namespace Stateflows.Extensions.MicrosoftAgentFramework.Agents.Enums;

public enum AgenticContentType
{
    Data,
    Error,
    FunctionCall,
    FunctionResult,
    HostedFile,
    HostedVectorStore,
    Text,
    TextReasoning,
    Uri,
    Usage,
    ToolCall,
    ToolResult,
    InputRequest,
    InputResponse,
    ToolApprovalRequest,
    ToolApprovalResponse,
    McpServerToolCall,
    McpServerToolResult,
    ImageGenerationToolCall,
    ImageGenerationToolResult,
    CodeInterpreterToolCall,
    CodeInterpreterToolResult,
    WebSearchToolCall,
    WebSearchToolResult,
    Unknown
}
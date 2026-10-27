namespace Stateflows.Extensions.MicrosoftAgentFramework.Agents.Enums;

/// <summary>Identifies the framework content kind represented by a serialized agentic message payload.</summary>
public enum AgenticContentType
{
    /// <summary>Data content.</summary>
    Data,
    /// <summary>Error content.</summary>
    Error,
    /// <summary>A function call.</summary>
    FunctionCall,
    /// <summary>A function result.</summary>
    FunctionResult,
    /// <summary>A hosted file reference.</summary>
    HostedFile,
    /// <summary>A hosted vector store reference.</summary>
    HostedVectorStore,
    /// <summary>Plain text content.</summary>
    Text,
    /// <summary>Reasoning text content.</summary>
    TextReasoning,
    /// <summary>A URI content value.</summary>
    Uri,
    /// <summary>Token or usage information.</summary>
    Usage,
    /// <summary>A tool call.</summary>
    ToolCall,
    /// <summary>A tool result.</summary>
    ToolResult,
    /// <summary>A request for user input.</summary>
    InputRequest,
    /// <summary>A response to an input request.</summary>
    InputResponse,
    /// <summary>A request for tool approval.</summary>
    ToolApprovalRequest,
    /// <summary>A response to a tool approval request.</summary>
    ToolApprovalResponse,
    /// <summary>A tool call made through an MCP server.</summary>
    McpServerToolCall,
    /// <summary>A result returned by an MCP server tool.</summary>
    McpServerToolResult,
    /// <summary>A call to an image-generation tool.</summary>
    ImageGenerationToolCall,
    /// <summary>A result from an image-generation tool.</summary>
    ImageGenerationToolResult,
    /// <summary>A call to a code-interpreter tool.</summary>
    CodeInterpreterToolCall,
    /// <summary>A result from a code-interpreter tool.</summary>
    CodeInterpreterToolResult,
    /// <summary>A call to a web-search tool.</summary>
    WebSearchToolCall,
    /// <summary>A result from a web-search tool.</summary>
    WebSearchToolResult,
    /// <summary>Content not recognized by the current serializer mapping.</summary>
    Unknown
}
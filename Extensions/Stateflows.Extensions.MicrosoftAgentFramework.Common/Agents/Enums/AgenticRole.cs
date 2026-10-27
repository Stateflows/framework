namespace Stateflows.Extensions.MicrosoftAgentFramework.Agents.Enums;

/// <summary>Identifies a chat message role in the agentic message transport contract.</summary>
public enum AgenticRole
{
    /// <summary>System-provided instruction or context.</summary>
    System,
    /// <summary>Message produced by the assistant or agent.</summary>
    Assistant,
    /// <summary>Message supplied by the user.</summary>
    User,
    /// <summary>Message supplied by a tool.</summary>
    Tool,
    /// <summary>A role not recognized by the current serializer mapping.</summary>
    Unknown
}
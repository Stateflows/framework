using Microsoft.Extensions.AI;

namespace Stateflows.MAF;

public class AgenticChatMessage
{
    public required ChatMessage Message { get; set; }
}
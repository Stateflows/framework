using Microsoft.Extensions.AI;
using Stateflows.Common;

namespace Stateflows.MAF;

public class AgenticChatInquiry
{
    public required ChatMessage Message { get; set; }
    public EventHolder GuardTriggerHolder { get; set; }
}
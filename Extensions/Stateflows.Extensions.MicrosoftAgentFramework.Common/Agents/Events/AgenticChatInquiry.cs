using Microsoft.Extensions.AI;
using Stateflows.Common;

namespace Stateflows.MAF;

/// <summary>Notification carrying an agentic transition inquiry and the event that caused its guard to defer.</summary>
public class AgenticChatInquiry
{
    /// <summary>Gets or sets the system chat message describing the candidate transition.</summary>
    public required ChatMessage Message { get; set; }
    /// <summary>Gets or sets the original event and headers whose transition guard requested agent input.</summary>
    public EventHolder GuardTriggerHolder { get; set; }
}
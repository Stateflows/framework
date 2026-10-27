namespace Stateflows.MAF.AIAgents;

/// <summary>Defines metadata keys and values used to describe agentic workflow transitions.</summary>
public static class AIAgentConstants
{
    /// <summary>The metadata value identifying an agentic workflow decision operation.</summary>
    public const string AgenticWorkflowDecision = "agentic_workflow_decision";
    /// <summary>The metadata namespace used to group agentic extension data.</summary>
    public const string AgenticExtensions = nameof(AgenticExtensions);
    /// <summary>The metadata key for the agentic extension mode.</summary>
    public const string ExtensionMode = nameof(ExtensionMode);
    /// <summary>The metadata value identifying an agentic transition.</summary>
    public const string AgenticTransition = nameof(AgenticTransition);
    /// <summary>The metadata key containing configured agentic transitions.</summary>
    public const string Transitions = $"{AgenticExtensions}::{nameof(Transitions)}";
    /// <summary>The metadata key for an agentic transition's display name.</summary>
    public const string TransitionName = $"{AgenticExtensions}::{nameof(TransitionName)}";
    /// <summary>The metadata key for the prompt or description used to select a transition.</summary>
    public const string TransitionDescription = $"{AgenticExtensions}::{nameof(TransitionDescription)}";
    /// <summary>The metadata key identifying the transition guard value.</summary>
    public const string GuardKey = $"{AgenticExtensions}::{nameof(GuardKey)}";
    /// <summary>The metadata key containing the generated transition guard marker.</summary>
    public const string GuardValue = $"{AgenticExtensions}::{nameof(GuardValue)}";
    
    /// <summary>The metadata key for agentic inquiry tools.</summary>
    public const string AgenticInquiryTools = "agentic_inquiry_tools";
    /// <summary>The event header key indicating acceptance of an agentic inquiry.</summary>
    public const string AgenticInquiryAcceptance = "inquiry_acceptance";
    /// <summary>The metadata key for the event that triggered an agentic transition inquiry.</summary>
    public const string GuardTriggerKey = $"{AgenticExtensions}::{nameof(GuardTriggerKey)}";
}
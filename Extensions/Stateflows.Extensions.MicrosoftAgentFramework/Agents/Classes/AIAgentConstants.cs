namespace Stateflows.MAF.AIAgents;

public static class AIAgentConstants
{
    public const string AgenticWorkflowDecision = "agentic_workflow_decision";
    public const string AgenticExtensions = nameof(AgenticExtensions);
    public const string ExtensionMode = nameof(ExtensionMode);
    public const string AgenticTransition = nameof(AgenticTransition);
    public const string Transitions = $"{AgenticExtensions}::{nameof(Transitions)}";
    public const string TransitionName = $"{AgenticExtensions}::{nameof(TransitionName)}";
    public const string TransitionDescription = $"{AgenticExtensions}::{nameof(TransitionDescription)}";
    public const string GuardKey = $"{AgenticExtensions}::{nameof(GuardKey)}";
    public const string GuardValue = $"{AgenticExtensions}::{nameof(GuardValue)}";
    
    public const string AgenticInquiryTools = "agentic_inquiry_tools";
    public const string AgenticInquiryAcceptance = "inquiry_acceptance";
    public const string GuardTriggerKey = $"{AgenticExtensions}::{nameof(GuardTriggerKey)}";
}
using Stateflows.StateMachines;

namespace Stateflows.Examples.Behaviors.StateMachines.Document.Effects;

public class ReportAutorejection(
    IStateMachineContext stateMachineContext
) : BaseRejectionEffect(stateMachineContext, "Rejected automatically after period of inactivity.");
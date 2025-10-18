using Stateflows.StateMachines;

namespace Stateflows.Examples.Behaviors.StateMachines.Document.Effects;

public class ReportRejection(
    IStateMachineContext stateMachineContext
) : BaseRejectionEffect(stateMachineContext, "Rejected explicitly.");
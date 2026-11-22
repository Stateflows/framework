namespace Stateflows.StateMachines;

/// <summary>A registered action's semantic operation.</summary>
public enum StateMachineOperation
{
    Initialize,
    Finalize,
    Entry,
    Exit,
    Effect
}

/// <summary>The Boolean structure explicitly registered by a guard builder.</summary>
public enum GuardKind
{
    Predicate,
    And,
    Or,
    Not
}

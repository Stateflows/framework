using System;

namespace Stateflows;

/// <summary>Flags selecting the categories of elements included when constructing typed element names.</summary>
[Flags]
public enum TypedElements
{
    /// <summary>No element category.</summary>
    None = 0,
    /// <summary>State-machine behavior types.</summary>
    StateMachines = 1,
    /// <summary>States contained in state-machine models.</summary>
    StateMachineStates = 2,
    /// <summary>Activity behavior types.</summary>
    Activities = 4,
    /// <summary>Nodes contained in activity models.</summary>
    ActivityNodes = 8,
    /// <summary>Action behavior types.</summary>
    Actions = 16,
    /// <summary>Event types.</summary>
    Events = 32,
    /// <summary>Token types.</summary>
    Tokens = 64,
    /// <summary>Exception types.</summary>
    Exceptions = 128,
    /// <summary>Entity types.</summary>
    Entities = 256,
    /// <summary>All defined element categories.</summary>
    All = StateMachines | StateMachineStates | Activities | ActivityNodes | Actions | Events | Tokens | Exceptions | Entities,
    /// <summary>All behavior categories.</summary>
    Behaviors = StateMachines | Activities | Actions | Entities,
    /// <summary>All model-element categories.</summary>
    ModelElements = StateMachineStates | ActivityNodes,
    /// <summary>All deliverable categories.</summary>
    Deliverables = Events | Tokens | Exceptions,
}
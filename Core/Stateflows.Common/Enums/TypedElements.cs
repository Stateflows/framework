using System;

namespace Stateflows;

[Flags]
public enum TypedElements
{
    None = 0,
    StateMachines = 1,
    StateMachineStates = 2,
    Activities = 4,
    ActivityNodes = 8,
    Actions = 16,
    Events = 32,
    Tokens = 64,
    Exceptions = 128,
    Entities = 256,
    All = StateMachines | StateMachineStates | Activities | ActivityNodes | Actions | Events | Tokens | Exceptions | Entities,
    Behaviors = StateMachines | Activities | Actions | Entities,
    ModelElements = StateMachineStates | ActivityNodes,
    Deliverables = Events | Tokens | Exceptions,
}
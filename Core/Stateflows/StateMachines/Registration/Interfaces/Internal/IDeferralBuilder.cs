using System;
using Stateflows.Common.Models;
using Stateflows.StateMachines.Interfaces;
using Stateflows.StateMachines.Models;

namespace Stateflows.StateMachines.Registration.Interfaces.Internal;

internal interface IDeferralBuilder
{
    Type EventType { get; }
    Vertex Vertex { get; }
    Logic<StateMachinePredicateAsync> Guards { get; }
}
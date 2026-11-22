using System.Collections.Generic;
using Stateflows.Common.Models;
using Stateflows.StateMachines.Interfaces;

namespace Stateflows.StateMachines.Models
{
    /// <summary>An initializer declared for a set of payload event names.</summary>
    internal sealed class PayloadInitializerDeclaration(IReadOnlyList<string> names, Logic<StateMachinePredicateAsync> logic)
    {
        public IReadOnlyList<string> Names { get; } = names;
        public Logic<StateMachinePredicateAsync> Logic { get; } = logic;
    }
}

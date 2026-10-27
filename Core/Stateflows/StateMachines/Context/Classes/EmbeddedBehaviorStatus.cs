using System;
using System.Linq;

namespace Stateflows.StateMachines.Context.Classes
{
    /// <summary>Stores the embedded behavior's reported expected events and state stack for machine persistence.</summary>
    public class EmbeddedBehaviorStatus
    {
        /// <summary>Determines whether nonempty expected-event metadata should be serialized.</summary>
        /// <returns>Whether <see cref="ExpectedEvents"/> contains any entries.</returns>
        public bool ShouldSerializeExpectedEvents()
            => ExpectedEvents.Any();

        /// <summary>The event names last reported as accepted by the embedded behavior.</summary>
        public string[] ExpectedEvents { get; set; } = Array.Empty<string>();

        /// <summary>Determines whether a nonempty embedded-state stack should be serialized.</summary>
        /// <returns>Whether <see cref="StatesStack"/> contains any entries.</returns>
        public bool ShouldSerializeStatesStack()
            => StatesStack.Any();

        /// <summary>The state names last reported for the embedded behavior.</summary>
        public string[] StatesStack { get; set; } = Array.Empty<string>();
    }
}

using System;
using System.Linq;

namespace Stateflows.StateMachines.Context.Classes
{
    public class EmbeddedBehaviorStatus
    {
        public bool ShouldSerializeExpectedEvents()
            => ExpectedEvents.Any();

        public string[] ExpectedEvents { get; set; } = Array.Empty<string>();

        public bool ShouldSerializeStatesStack()
            => StatesStack.Any();

        public string[] StatesStack { get; set; } = Array.Empty<string>();
    }
}

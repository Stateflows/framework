using System;
using System.Collections.Generic;
using Stateflows.Payloads;

namespace Stateflows.Activities.Registration.Builders
{
    /// <summary>
    /// Carries the payload names of a name-first flow member (D30) to the typed <c>AddFlow&lt;Payload&gt;</c> /
    /// <c>UseFlow&lt;Payload&gt;</c> implementation that creates or selects the edge. Building is synchronous, so the
    /// names live in a thread-static slot that is consumed by the first edge created or selected inside the scope.
    /// </summary>
    internal static class PayloadFlowScope
    {
        [ThreadStatic]
        private static IReadOnlyList<string> pendingNames;

        public static TResult With<TResult>(IReadOnlyList<string> names, string paramName, Func<TResult> action)
        {
            var validated = PayloadNameSet.Validate(names, paramName);
            var previous = pendingNames;
            pendingNames = validated;
            try
            {
                return action();
            }
            finally
            {
                pendingNames = previous;
            }
        }

        public static void With(IReadOnlyList<string> names, string paramName, System.Action action)
            => With(names, paramName, () => { action(); return true; });

        public static IReadOnlyList<string> Take()
        {
            var names = pendingNames;
            pendingNames = null;
            return names;
        }
    }
}
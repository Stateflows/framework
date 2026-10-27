using System;
using Stateflows.Common.Extensions;

namespace Stateflows.Common
{
    /// <summary>Resolves CLR event type names according to the Stateflows naming policy.</summary>
    public static class EventInfoExtensions
    {
        /// <summary>Gets the readable event name for a CLR type.</summary>
        /// <param name="type">The event payload type.</param>
        /// <returns>The event name using full or short names according to the configured event naming setting.</returns>
        public static string GetEventName(this Type @type)
            => @type.GetReadableName(TypedElements.Events);
    }
}

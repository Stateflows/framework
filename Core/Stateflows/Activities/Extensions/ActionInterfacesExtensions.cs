using System.Linq;
using System.Collections.Generic;

namespace Stateflows.Activities
{
    public static class ActionInterfacesExtensions
    {
        public static IEnumerable<TToken> GetInput<TToken>(this IConsumes<TToken> consumer)
            where TToken : Token, new()
            => (consumer as Action).Context.Input.OfType<TToken>().ToArray();

        public static void Output<TToken>(this IProduces<TToken> producent, TToken outputToken)
            where TToken : Token, new()
            => (producent as Action).Context.Output(outputToken);

        public static void OutputRange<TToken>(this IProduces<TToken> producent, IEnumerable<TToken> outputTokens)
            where TToken : Token, new()
            => (producent as Action).Context.OutputRange(outputTokens);
    }
}

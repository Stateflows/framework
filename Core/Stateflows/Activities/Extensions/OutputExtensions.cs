using System.Linq;
using System.Collections.Generic;
using Stateflows.Common;
using Stateflows.Common.Data;

namespace Stateflows.Activities
{
    public static class OutputExtensions
    {
        public static void Add<TTokenPayload>(this Output<Token<TTokenPayload>> output, TTokenPayload payload)
            => output.Add(payload.ToToken());

        public static void AddRange<TTokenPayload>(this Output<Token<TTokenPayload>> output, IEnumerable<TTokenPayload> payloads)
            => output.AddRange(payloads.Select(payload => payload.ToToken()).ToArray());
    }
}

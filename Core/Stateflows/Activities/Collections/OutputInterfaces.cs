using System.Linq;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using Stateflows.Utils;
using Stateflows.Common;

namespace Stateflows.Activities
{
    interface IOutputTokens<TToken> : ICollection<TToken>
    {
        void AddRange(IEnumerable<TToken> items);
    }
}

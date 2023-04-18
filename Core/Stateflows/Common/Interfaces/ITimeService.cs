using System.Threading.Tasks;
using System.Collections.Generic;
using Stateflows.Common.Classes;

namespace Stateflows.Common.Interfaces
{
    public interface ITimeService
    {
        Task Clear(BehaviorId behaviorId, IEnumerable<string> ids);

        Task Register(TimeToken[] timeTokens);
    }
}

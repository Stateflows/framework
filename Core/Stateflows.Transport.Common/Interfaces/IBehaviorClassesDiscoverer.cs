using System.Threading.Tasks;
using System.Collections.Generic;

namespace Stateflows.Transport.Common.Interfaces
{
    public interface IBehaviorClassesDiscoverer
    {
        Task DiscoverBehaviorClassesAsync(IEnumerable<BehaviorClass> localBehaviorClasses);
    }
}

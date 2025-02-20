using System.Threading.Tasks;

namespace Stateflows.Common
{
    public interface IUnwatcher
    {
        Task UnwatchAsync<TNotification>();
    }
}

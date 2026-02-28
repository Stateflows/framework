using System.Threading.Tasks;

namespace Stateflows.Common.Interfaces
{
    public interface ITypedNotificationHandler
    {
        Task HandleNotificationAsync<T>(EventHolder<T> eventHolder);
    }
}

using System.Threading.Tasks;

namespace Stateflows.Common.Interfaces
{
    public interface INotificationHandler
    {
        Task HandleNotificationAsync(EventHolder eventHolder);
    }
}

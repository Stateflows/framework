using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace Stateflows.Common.Interfaces
{
    public interface IStateflowsExecutor
    {
        Task<EventStatus> DoProcessAsync<TEvent>(EventHolder<TEvent> eventHolder);
    }
}

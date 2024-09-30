using System.Threading.Tasks;

namespace Stateflows.Common.Interfaces
{
    public interface IStateflowsProcessor
    {
        Task<EventStatus> ExecuteBehaviorAsync<TEvent>(EventHolder<TEvent> eventHolder, EventStatus result, IStateflowsExecutor stateflowsExecutor);
    }
}

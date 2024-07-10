using Stateflows.Common;
using System.Threading.Tasks;

namespace Stateflows.StateMachines
{
    public interface IInitializer<TInitializationEvent>
        where TInitializationEvent : Event, new()
    {
        Task<bool> OnInitialize(TInitializationEvent initializationEvent);
    }

    public interface IDefaultInitializer
    {
        Task<bool> OnInitialize();
    }
}

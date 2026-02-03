using System.Threading.Tasks;

namespace Stateflows.Common
{
    public interface IInitializer<in TInitializationEvent>
    {
        Task<bool> OnInitializeAsync(TInitializationEvent initializationEvent);
    }

    public interface IDefaultInitializer
    {
        Task<bool> OnInitializeAsync();
    }
}

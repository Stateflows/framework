using System.Threading.Tasks;

namespace Stateflows.Common
{
    public interface IInitializedBy<in TInitializationRequest>
        where TInitializationRequest : InitializationRequest, new()
    {
        Task<bool> OnInitializeAsync(TInitializationRequest initializationRequest);
    }
}

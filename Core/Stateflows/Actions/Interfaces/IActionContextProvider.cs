using System.Threading.Tasks;

namespace Stateflows.Actions
{
    public interface IActionContextProvider
    {
        Task<(bool Success, IActionContextHolder ContextHolder)> TryProvideAsync(ActionId actionId);
    }
}
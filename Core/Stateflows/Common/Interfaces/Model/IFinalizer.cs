using System.Threading.Tasks;

namespace Stateflows.Common
{
    public interface IFinalizer
    {
        Task OnFinalizeAsync();
    }
}

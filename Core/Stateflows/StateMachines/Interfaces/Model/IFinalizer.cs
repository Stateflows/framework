using System.Threading.Tasks;

namespace Stateflows.StateMachines
{
    public interface IFinalizer
    {
        Task OnFinalize();
    }
}

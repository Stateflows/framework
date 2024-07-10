using System.Threading.Tasks;

namespace Stateflows.Activities
{
    public interface IActionNode
    {
        Task ExecuteAsync();
    }
}

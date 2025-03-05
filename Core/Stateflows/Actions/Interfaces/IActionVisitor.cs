using System.Threading.Tasks;

namespace Stateflows.Actions
{
    public interface IActionVisitor
    {
        Task ActionAddedAsync(string actionName, int actionVersion);
        
        Task ActionTypeAddedAsync<TAction>(string actionName, int actionVersion)
            where TAction : class, IAction;
    }
}

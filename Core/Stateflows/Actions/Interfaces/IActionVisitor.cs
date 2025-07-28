using System.Threading.Tasks;
using Stateflows.Common;

namespace Stateflows.Actions
{
    public interface IActionVisitor
    {
        Task ActionAddedAsync(string actionName, int actionVersion);
        
        Task ActionTypeAddedAsync<TAction>(string actionName, int actionVersion)
            where TAction : class, IAction;
        
        Task CustomEventAddedAsync<TEvent>(string actionName, int actionVersion, BehaviorStatus[] supportedStatuses);
    }
}

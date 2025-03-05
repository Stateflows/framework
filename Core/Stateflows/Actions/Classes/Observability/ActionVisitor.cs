using System.Threading.Tasks;

namespace Stateflows.Actions
{
    public abstract class ActionVisitor : IActionVisitor
    {
        public virtual Task ActionAddedAsync(string actionName, int actionVersion)
            => Task.CompletedTask;

        public virtual Task ActionTypeAddedAsync<TAction>(string actionName, int actionVersion) where TAction : class, IAction
            => Task.CompletedTask;
    }
}
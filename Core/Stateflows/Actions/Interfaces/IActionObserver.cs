using Stateflows.Actions.Context.Interfaces;

namespace Stateflows.Actions
{
    public interface IActionObserver
    {
        void BeforeActionInitialize(IActionDelegateContext context);
        void AfterActionInitialize(IActionDelegateContext context);

        void BeforeActionFinalize(IActionDelegateContext context);
        void AfterActionFinalize(IActionDelegateContext context);
    }
}

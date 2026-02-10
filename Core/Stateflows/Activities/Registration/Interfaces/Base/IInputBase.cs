namespace Stateflows.Activities.Registration.Interfaces.Base
{
    public interface IInputBase<out TReturn>
    {
        TReturn AddInput(InputBuildAction buildAction);
    }
}

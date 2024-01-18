namespace Stateflows.Activities.Registration.Interfaces.Base
{
    public interface IOutput<out TReturn>
    {
        TReturn AddOutput();
    }
}

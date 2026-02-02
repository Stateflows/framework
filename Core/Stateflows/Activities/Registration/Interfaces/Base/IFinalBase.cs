namespace Stateflows.Activities.Registration.Interfaces.Base
{
    public interface IFinalBase<out TReturn>
    {
        TReturn AddFinal();
    }
}

namespace Stateflows.Activities.Context.Interfaces
{
    public interface ITransformContext<out TToken> : IGuardContext<TToken>
        where TToken : Token, new()
    { }
}

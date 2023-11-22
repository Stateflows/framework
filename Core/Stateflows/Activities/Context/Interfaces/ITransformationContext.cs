namespace Stateflows.Activities.Context.Interfaces
{
    public interface ITransformationContext<out TToken> : IGuardContext<TToken>
        where TToken : Token, new()
    { }
}

namespace Stateflows.Activities.Context.Interfaces
{
    public interface ITokenContext<out TToken> : IActivityActionContext
        where TToken : Token, new()
    {
        TToken Token { get; }
    }
}

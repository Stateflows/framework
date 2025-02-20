using Stateflows.Common;

namespace Stateflows.Activities
{
    public class TokensOutputRequest : IRequest<TokensOutput>
    { }
    
    public class TokensOutputRequest<T> : IRequest<TokensOutput<T>>
    { }
}
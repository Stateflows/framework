namespace Stateflows.Common;

public class TokensOutputRequest : IRequest<TokensOutput>;

public class TokensOutputRequest<T> : IRequest<TokensOutput<T>>;
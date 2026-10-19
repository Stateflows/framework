namespace Stateflows.Common;

[NoDeduplication]
public class TokensOutputRequest : IRequest<TokensOutput>;

[NoDeduplication]
public class TokensOutputRequest<T> : IRequest<TokensOutput<T>>;
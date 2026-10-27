namespace Stateflows.Common;

/// <summary>Requests all output tokens from a behavior.</summary>
[NoDeduplication]
public class TokensOutputRequest : IRequest<TokensOutput>;

/// <summary>Requests output tokens assignable to a specified type.</summary>
/// <typeparam name="T">The requested token type.</typeparam>
[NoDeduplication]
public class TokensOutputRequest<T> : IRequest<TokensOutput<T>>;
namespace Stateflows.Activities;

public interface IInputOutputTokens<TToken> : IInputTokens<TToken>, IOutputTokens<TToken>
{
    void PassAllOn();
}

public interface IInputOutputToken<TToken> : IInputToken<TToken>, IOutputTokens<TToken>
{
    void PassOn();
}

public interface IOptionalInputOutputTokens<TToken> : IOptionalInputTokens<TToken>
{
    void PassAllOn();
}

public interface IOptionalInputOutputToken<TToken> : IOptionalInputToken<TToken>
{
    void PassOn();
}
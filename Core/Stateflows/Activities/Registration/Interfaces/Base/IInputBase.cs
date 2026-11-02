namespace Stateflows.Activities.Registration.Interfaces.Base
{
    /// <summary>Adds an input node to an activity graph.</summary>
    /// <typeparam name="TReturn">Fluent builder type returned after this Activity element is configured.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-input-node" />
    public interface IInputBase<out TReturn>
    {
        /// <summary>Adds an input boundary node to a structured Activity.</summary>
        /// <param name="buildAction">Callback configuring the input node's outgoing flows. Its builder is scoped to the
        /// node or flow, not the enclosing Activity.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-input-node" />
        [System.Obsolete("Use AddInputPin<TToken> to declare supported input token types.")]
        TReturn AddInput(InputBuildAction buildAction);

        /// <summary>Adds a typed input pin with object flows of its declared token type.</summary>
        TReturn AddInputPin<TToken>(System.Action<IInputPinBuilder<TToken>> buildAction);
    }
}

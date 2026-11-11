namespace Stateflows.Activities.Registration.Interfaces.Base
{
    /// <summary>Adds typed input pins to an activity graph.</summary>
    /// <typeparam name="TReturn">Fluent builder type returned after this Activity element is configured.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.1.6.-Input-Pin" />
    public interface IInputBase<out TReturn>
    {
        /// <summary>Adds a typed input pin with object flows of its declared token type.</summary>
        /// <remarks>The pin name is generated as the containing scope name followed by
        /// <c>InputPin&lt;TToken&gt;.Name</c>.</remarks>
        TReturn AddInputPin<TToken>(System.Action<IInputPinBuilder<TToken>> buildAction);
    }
}

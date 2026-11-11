namespace Stateflows.Activities.Registration.Interfaces.Base
{
    /// <summary>Adds typed output pins to a nested activity graph.</summary>
    /// <typeparam name="TReturn">Fluent builder type returned after this Activity element is configured.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.1.7.-Output-Pin" />
    public interface IOutputBase<out TReturn>
    {
        /// <summary>Adds a typed output pin, targeted with OutputPin&lt;TToken&gt; by default.</summary>
        /// <param name="name">Optional globally unique node name; defaults to OutputPin&lt;TToken&gt;.Name.</param>
        TReturn AddOutputPin<TToken>(string? name = null);
    }
}

namespace Stateflows.Common
{
    /// <summary>Requests reset of the receiving behavior instance.</summary>
    [NoImplicitInitialization]
    public class Reset
    {
        /// <summary>Gets or sets whether to perform a full or partial reset.</summary>
        public ResetMode Mode { get; set; } = ResetMode.Full;
    }
}

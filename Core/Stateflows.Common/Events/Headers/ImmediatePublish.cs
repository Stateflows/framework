namespace Stateflows.Common
{
    /// <summary>
    /// Publishes this notification during execution instead of buffering it until publisher state commits.
    /// The notification cannot be retracted if execution or persistence later fails.
    /// </summary>
    public sealed class ImmediatePublish : EventHeader
    { }
}
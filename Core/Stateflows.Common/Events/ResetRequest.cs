namespace Stateflows.Common
{
    [NoImplicitInitialization]
    public class ResetRequest : Request<ResetResponse>
    {
        public ResetMode Mode { get; set; } = ResetMode.Full;
    }
}

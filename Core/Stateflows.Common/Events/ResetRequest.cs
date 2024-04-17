namespace Stateflows.Common
{
    public class ResetRequest : Request<ResetResponse>
    {
        public bool KeepVersion { get; set; } = false;
    }
}

namespace Stateflows.Common
{
    public sealed class Reset : Command
    {
        public bool KeepVersion { get; set; } = false;
    }
}

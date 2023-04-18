namespace Stateflows.Common
{
    public sealed class InitializedResponse : Response
    {
        public override string Name => nameof(InitializedResponse);

        public bool Initialized { get; set; }
    }
}

namespace Stateflows.Common
{
    public sealed class InitializedRequest : Request<InitializedResponse>
    {
        public override string Name => nameof(InitializedRequest);
    }
}

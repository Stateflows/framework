namespace Stateflows.Common
{
    public class InitializationResponse : Response
    {
        public override string Name => nameof(InitializationResponse);

        public bool InitializationSuccessful { get; set; }
    }
}

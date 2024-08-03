

namespace Activity.IntegrationTests.Classes.Actions
{
    internal class AcceptSomeEventAction : IAcceptEventActionNode<SomeEvent>
    {
        public Task ExecuteAsync(SomeEvent @event, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}

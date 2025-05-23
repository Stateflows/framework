namespace StateMachine.IntegrationTests.Classes.Events;

public class SomeEvent
{
    public string TheresSomethingHappeningHere { get; set; } = "Boo!";
    public bool InitializationSuccessful { get; set; }
    public int DelaySize { get; set; }
}
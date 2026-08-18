namespace Activity.IntegrationTests.Classes.Events;


public interface IInherited;

public class SomeInheritedEvent : SomeEvent, IInherited;
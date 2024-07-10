using Stateflows.Common;

namespace Examples.Common
{
    public class SomeEvent : Event
    {
        public string TheresSomethingHappeningHere { get; set; } = "What it is ain't exactly clear";
        public int DelaySize { get; set; }
        public bool InitializationSuccessful { get; set; }
    }
}
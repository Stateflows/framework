namespace Stateflows.Common
{
    public abstract class EventHeader
    {
        protected EventHeader(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}

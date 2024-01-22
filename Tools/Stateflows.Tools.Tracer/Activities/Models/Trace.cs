namespace Stateflows.Tools.Tracer.Activities.Models
{
    internal class Trace : Tracer.Classes.Trace
    {
        public Step EntryStep { get; set; } = new Step();
    }
}

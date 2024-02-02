namespace Stateflows
{
    public struct ActivityClassFactory
    {
        public readonly ActivityClass ToClass(string name) => new ActivityClass(name);

        public static implicit operator string(ActivityClassFactory _) => "Activity";
    }
}

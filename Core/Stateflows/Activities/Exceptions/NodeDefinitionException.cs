namespace Stateflows.Activities.Exceptions
{
    public class NodeDefinitionException : ActivityDefinitionException
    {
        public NodeDefinitionException(string nodeName, string message, ActivityClass activityClass) : base(message, activityClass)
        {
            NodeName = nodeName;
        }

        public string NodeName { get; set; }
    }
}

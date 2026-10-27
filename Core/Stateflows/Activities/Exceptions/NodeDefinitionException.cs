namespace Stateflows.Activities.Exceptions
{
    /// <summary>Reports an invalid or inconsistent node in an activity definition.</summary>
    public class NodeDefinitionException : ActivityDefinitionException
    {
        /// <summary>Initializes a node definition exception.</summary>
        /// <param name="nodeName">Name of the node whose definition is invalid.</param>
        /// <param name="message">Description of the node definition error.</param>
        /// <param name="activityClass">The activity class containing the invalid node.</param>
        public NodeDefinitionException(string nodeName, string message, ActivityClass activityClass) : base(message, activityClass)
        {
            NodeName = nodeName;
        }

        /// <summary>Gets or sets the name of the node whose definition is invalid.</summary>
        public string NodeName { get; set; }
    }
}

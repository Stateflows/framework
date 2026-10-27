using Stateflows.Common.Exceptions;

namespace Stateflows.Actions.Exceptions
{
#pragma warning disable S3925 // "ISerializable" should be implemented correctly
    /// <summary>Reports an action-related definition error not associated with a specific action class.</summary>
    public class ActionException : StateflowsDefinitionException
    {
        /// <summary>Initializes an action exception.</summary>
        /// <param name="message">Description of the error.</param>
        public ActionException(string message) : base(message) { }
    }
#pragma warning restore S3925 // "ISerializable" should be implemented correctly
}

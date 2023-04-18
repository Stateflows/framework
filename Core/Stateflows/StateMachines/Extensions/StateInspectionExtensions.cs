using System.Linq;
using Stateflows.StateMachines.Inspection.Classes;

namespace Stateflows.StateMachines.Extensions
{
    internal static class StateInspectionExtensions
    {
        public static void BeginAction(this StateInspection stateInspection, string actionName)
        {
            var initializeAction = stateInspection.Actions.Where(a => a.Name == actionName).Take(1);
            if (initializeAction.Count() == 1)
            {
                (initializeAction.First() as ActionInspection).Active = true;
            }
        }

        public static void EndAction(this StateInspection stateInspection, string actionName)
        {
            var initializeAction = stateInspection.Actions.Where(a => a.Name == actionName).Take(1);
            if (initializeAction.Count() == 1)
            {
                (initializeAction.First() as ActionInspection).Active = false;
            }
        }
    }
}

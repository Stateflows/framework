using System.Linq;
using System.Text;
using System.Collections.Generic;
using Stateflows.StateMachines.Inspection.Interfaces;

namespace Stateflows.Extensions.PlantUml.Classes
{
    internal static class PlantUmlGenerator
    {
        private static void GetPlantUml(int indentCount, IEnumerable<IStateInspection> states, StringBuilder builder)
        {
            string indent = new string(' ', indentCount);
            foreach (var state in states)
            {
                if (state.IsInitial)
                {
                    builder.AppendLine($"{indent}[*] --> {state.Name}");
                }
                GetPlantUml(indentCount, state, builder);
            }
        }

        private static void GetPlantUml(int indentCount, IStateInspection state, StringBuilder builder)
        {
            string indent = new string(' ', indentCount);

            if (state.Active)
            {
                builder.AppendLine($"{indent}state {state.Name} #line.bold {{");
                builder.AppendLine($"{indent}  skinparam {state.Name} {{");
                builder.AppendLine($"{indent}    FontStyle bold");
                builder.AppendLine($"{indent}  }}");
            }
            else
            {
                builder.AppendLine($"{indent}state {state.Name} {{");
            }

            if (state.States.Count() > 0)
            {
                GetPlantUml(indentCount + 2, state.States, builder);
            }

            builder.AppendLine($"{indent}}}");

            foreach (var action in state.Actions)
            {
                if (action.Name == Constants.Entry)
                {
                    builder.AppendLine($"{indent}{state.Name} : entry / onEntry()");
                }

                if (action.Name == Constants.Exit)
                {
                    builder.AppendLine($"{indent}{state.Name} : exit / onExit()");
                }
            }

            foreach (var transition in state.Transitions)
            {
                if (transition.Target == null)
                {
                    builder.AppendLine($"{indent}{state.Name} : {transition.Trigger} / on{transition.Trigger}()");
                }
                else
                {
                    if (transition.Trigger == Constants.CompletionEvent)
                    {
                        builder.AppendLine($"{indent}{state.Name} --> {transition.Target.Name}");
                    }
                    else
                    {
                        builder.AppendLine($"{indent}{state.Name} --> {transition.Target.Name} : {transition.Trigger}");
                    }
                }
            }
        }

        public static string GetPlantUml(this IStateMachineInspection stateMachine)
        {
            var builder = new StringBuilder();

            builder.AppendLine("@startuml");

            GetPlantUml(0, stateMachine.States, builder);

            builder.AppendLine("@enduml");

            return builder.ToString();
        }

        public static string GetPlantUmlUrl(this IStateMachineInspection stateMachine)
        {
            return "http://www.plantuml.com/plantuml/png/" + PlantUmlTextEncoder.Encode(stateMachine.GetPlantUml());
        }
    }
}

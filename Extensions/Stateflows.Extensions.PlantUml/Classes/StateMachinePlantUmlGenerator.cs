using System.Linq;
using System.Text;
using System.Collections.Generic;
using Stateflows.StateMachines.Inspection.Interfaces;

namespace Stateflows.Extensions.PlantUml.Classes
{
    internal static class StateMachinePlantUmlGenerator
    {
        private static void GetPlantUml(int indentCount, IEnumerable<IStateInspection> states, StringBuilder builder)
        {
            string indent = new string(' ', indentCount);

            foreach (var state in states)
            {
                var stateName = state.Name.Split('.').Last();

                if (state.IsInitial)
                {
                    builder.AppendLine($"{indent}[*] --> {stateName}");
                }
                GetPlantUml(indentCount, state, stateName, builder);
            }
        }

        private static void GetPlantUml(int indentCount, IStateInspection state, string stateName, StringBuilder builder)
        {
            string indent = new string(' ', indentCount);

            if (state.IsFinal)
            {
                return;
            }

            if (state.Active)
            {
                builder.AppendLine($"{indent}state {stateName} #line.bold {{");
                builder.AppendLine($"{indent}  skinparam {stateName} {{");
                builder.AppendLine($"{indent}    FontStyle bold");
                builder.AppendLine($"{indent}  }}");
            }
            else
            {
                builder.AppendLine($"{indent}state {stateName} {{");
            }

            if (state.States.Any())
            {
                GetPlantUml(indentCount + 2, state.States, builder);
            }

            builder.AppendLine($"{indent}}}");

            foreach (var action in state.Actions)
            {
                if (action.Name == Constants.Entry)
                {
                    builder.AppendLine($"{indent}{stateName} : entry / onEntry()");
                }

                if (action.Name == Constants.Exit)
                {
                    builder.AppendLine($"{indent}{stateName} : exit / onExit()");
                }
            }

            foreach (var transition in state.Transitions)
            {
                var triggers = transition.Triggers.Select(trigger => trigger.Contains('<')
                    ? $"{trigger.Split('<').First().Split('.').Last()}<{trigger.Split('<').Last().Split('.').Last()}"
                    : trigger.Split('.').Last()
                );

                foreach (var trigger in triggers)
                {
                    if (transition.Target == null)
                    {
                        builder.AppendLine($"{indent}{stateName} : {trigger} / on{trigger}()");
                    }
                    else
                    {
                        var target = !transition.Target.IsFinal
                            ? transition.Target.Name.Split('.').Last()
                            : "[*]";

                        if (trigger == Constants.CompletionEvent)
                        {
                            builder.AppendLine($"{indent}{stateName} --> {target}");
                        }
                        else
                        {
                            builder.AppendLine($"{indent}{stateName} --> {target} : {trigger}");
                        }
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

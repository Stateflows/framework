using System.Linq;
using System.Text;
using System.Collections.Generic;
using Stateflows.StateMachines;
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

            if (state.IsChoice)
            {
                builder.AppendLine($"{indent}state {stateName} <<choice>>");
            }
            else
            if (state.IsJunction)
            {
                builder.AppendLine($"{indent}state \" \" as {stateName}_invisibleState #white;line:white {{");
                builder.AppendLine($"{indent}    state \" \" as {stateName} <<entryPoint>>");
                builder.AppendLine($"{indent}}}");
            }
            else
            if (state.IsFork)
            {
                builder.AppendLine($"{indent}state {stateName} <<fork>>");
            }
            else
            if (state.IsJoin)
            {
                builder.AppendLine($"{indent}state {stateName} <<join>>");
            }
            else
            {
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
            }

            if (state.Regions.Count() == 1)
            {
                GetPlantUml(indentCount + 2, state.Regions.First().States, builder);
            }

            if (state.Regions.Count() > 1)
            {
                var i = 1;
                foreach (var region in state.Regions)
                {
                    builder.AppendLine($"{indent}  state Region{i} #white ##[dashed]black {{");
                    GetPlantUml(indentCount + 4, region.States, builder);
                    builder.AppendLine($"{indent}  }}");

                    i++;
                }
            }

            if (!state.IsJoin && !state.IsFork && !state.IsChoice && !state.IsJunction)
            {
                builder.AppendLine($"{indent}}}");
            }

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
                            builder.AppendLine($"{indent}{stateName} --> {target} {(transition.IsElse ? ": [else]" : "")}");
                        }
                        else
                        {
                            builder.AppendLine($"{indent}{stateName} --> {target} : {trigger} {(transition.IsElse ? "[else]" : "")}");
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

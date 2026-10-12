using System.Linq;
using System.Text;
using System.Collections.Generic;
using Stateflows.Common;
using Stateflows.StateMachines;
using Stateflows.StateMachines.Inspection.Interfaces;

namespace Stateflows.Extensions.PlantUml.Classes
{
    internal static class StateMachinePlantUmlGenerator
    {
        private static void GetPlantUml(int indentCount, IEnumerable<IStateInspection> states, StringBuilder builder)
        {
            var indent = new string(' ', indentCount);

            foreach (var state in states)
            {
                var stateName = state.Name.ToShortName();
                var stateIdentifier = state.Name.ToIdentifier();

                builder.AppendLine($"state \"{stateName}\" as {stateIdentifier}");
                
                if (state.IsInitial)
                {
                    builder.AppendLine($"{indent}[*] --> {stateIdentifier}");
                }
                GetPlantUml(indentCount, state, stateIdentifier, builder);
            }
        }

        private static void GetPlantUml(int indentCount, IStateInspection state, string stateIdentifier, StringBuilder builder)
        {
            string indent = new string(' ', indentCount);

            if (state.IsFinal)
            {
                return;
            }

            if (state.IsChoice)
            {
                builder.AppendLine($"{indent}state {stateIdentifier} <<choice>>");
            }
            else
            if (state.IsJunction)
            {
                builder.AppendLine($"{indent}state \" \" as {stateIdentifier}_invisibleState #white;line:white {{");
                builder.AppendLine($"{indent}    state \" \" as {stateIdentifier} <<entryPoint>>");
                builder.AppendLine($"{indent}}}");
            }
            else
            if (state.IsFork)
            {
                builder.AppendLine($"{indent}state {stateIdentifier} <<fork>>");
            }
            else
            if (state.IsJoin)
            {
                builder.AppendLine($"{indent}state {stateIdentifier} <<join>>");
            }
            else
            {
                if (state.Active)
                {
                    builder.AppendLine($"{indent}state {stateIdentifier} #line.bold {{");
                    builder.AppendLine($"{indent}  skinparam {stateIdentifier} {{");
                    builder.AppendLine($"{indent}    FontStyle bold");
                    builder.AppendLine($"{indent}  }}");
                }
                else
                {
                    builder.AppendLine($"{indent}state {stateIdentifier} {{");
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
                    builder.AppendLine($"{indent}{stateIdentifier} : entry / onEntry()");
                }

                if (action.Name == Constants.Exit)
                {
                    builder.AppendLine($"{indent}{stateIdentifier} : exit / onExit()");
                }
            }

            foreach (var transition in state.Transitions)
            {
                var triggers = transition.Triggers.Select(trigger => trigger.ToShortName());

                foreach (var trigger in triggers)
                {
                    if (transition.Target == null)
                    {
                        builder.AppendLine($"{indent}{stateIdentifier} : {trigger} / on{trigger}()");
                    }
                    else
                    {
                        var target = !transition.Target.IsFinal
                            ? transition.Target.Name.ToIdentifier()
                            : "[*]";

                        if (trigger == Constants.CompletionEvent)
                        {
                            builder.AppendLine($"{indent}{stateIdentifier} --> {target} {(transition.IsElse ? ": [else]" : "")}");
                        }
                        else
                        {
                            builder.AppendLine($"{indent}{stateIdentifier} --> {target} : {trigger} {(transition.IsElse ? "[else]" : "")}");
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

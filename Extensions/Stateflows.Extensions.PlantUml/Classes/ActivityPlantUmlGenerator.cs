using System.Linq;
using System.Text;
using System.Collections.Generic;
using Stateflows.Common;
using Stateflows.Activities;
using Stateflows.Activities.Inspection.Interfaces;

namespace Stateflows.Extensions.PlantUml.Classes
{
    internal static class ActivityPlantUmlGenerator
    {
        private static string GetNodeName(INodeInspection node, string parentName)
        {
            var nodeName = node.Name.Split('.').Last();

            if (node.Type == NodeType.AcceptEventAction || node.Type == NodeType.TimeEventAction)
            {
                var parts = node.Name.Split('<');
                var eventPart = parts.Last().Split('.').Last();
                nodeName = string.Join("<", parts.First(), eventPart);
            }

            if (nodeName == "ExceptionHandler")
            {
                nodeName = string.Join("<", node.Name.Split('.').Reverse().Take(2)) + ">";
            }

            if (node.Type != NodeType.Final && node.Type != NodeType.Initial)
            {
                return (node.Type == NodeType.Input || node.Type == NodeType.Output) && parentName != null
                    ? $"\"{parentName}.{nodeName}\""
                    : $"\"{nodeName}\"";
            }
            else
            {
                return parentName != null
                    ? $"\"{parentName}.{nodeName}\""
                    : "(*)";
            }
        }
        private static void GetPlantUml(int indentCount, IEnumerable<INodeInspection> nodes, StringBuilder builder, string parentName = null)
        {
            foreach (var node in nodes)
            {
                GetPlantUml(indentCount, node, builder, parentName);
            }
        }

        private static void GetPlantUml(int indentCount, INodeInspection node, StringBuilder builder, string parentName = null)
        {
            string indent = new string(' ', indentCount);

            if (node.Type == NodeType.Final)
            {
                return;
            }

            if (node.Nodes.Any())
            {
                var nodeName = node.Name.Split('.').Last();
                builder.AppendLine($"{indent}partition \"{node.Type} {nodeName}\" " + "{");
                GetPlantUml(indentCount + 2, node.Nodes, builder, nodeName);
                builder.AppendLine($"{indent}" + "}");
            }

            foreach (var transition in node.Flows)
            {
                var source = GetNodeName(transition.Source, parentName);
                var target = GetNodeName(transition.Target, parentName);

                if (transition.TokenName != "NodeReferenceToken")
                {
                    if (transition.TokenName == typeof(ControlToken).GetTokenName())
                    {
                        builder.AppendLine($"{indent}{source} --> {target}");
                    }
                    else
                    {
                        builder.AppendLine($"{indent}{source} -->[{transition.TokenName}] {target}");
                    }
                }
            }
        }

        public static string GetPlantUml(this IActivityInspection activity)
        {
            var builder = new StringBuilder();

            builder.AppendLine("@startuml");

            GetPlantUml(0, activity.Nodes, builder);

            builder.AppendLine("@enduml");

            return builder.ToString();
        }

        public static string GetPlantUmlUrl(this IActivityInspection activity)
        {
            return "http://www.plantuml.com/plantuml/png/" + PlantUmlTextEncoder.Encode(activity.GetPlantUml());
        }
    }
}

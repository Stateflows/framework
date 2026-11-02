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
        private static string GetNodeName(INodeInspection node, string parentName,
            IReadOnlyDictionary<INodeInspection, string> pins)
        {
            if (pins.TryGetValue(node, out var pin))
                return pin;

            var nodeName = node.Name.ToShortName();

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
        private static void GetPlantUml(int indentCount, IEnumerable<INodeInspection> nodes, StringBuilder builder,
            IReadOnlyDictionary<INodeInspection, string> pins, string parentName = null)
        {
            foreach (var node in nodes.Where(pins.ContainsKey))
            {
                var direction = node.Type == NodeType.Input ? "Input" : "Output";
                var tokenName = node.PinTokenType.GetTokenName().ToShortName();
                var pin = pins[node];
                // Legacy Activity syntax declares nodes through links; a hidden self-link also covers isolated pins.
                builder.AppendLine($"{new string(' ', indentCount)}\"{direction} pin\\n{tokenName}\" as {pin} -[hidden]-> {pin}");
            }

            foreach (var node in nodes)
            {
                GetPlantUml(indentCount, node, builder, pins, parentName);
            }
        }

        private static void GetPlantUml(int indentCount, INodeInspection node, StringBuilder builder,
            IReadOnlyDictionary<INodeInspection, string> pins, string parentName = null)
        {
            string indent = new string(' ', indentCount);

            if (node.Type == NodeType.Final)
            {
                return;
            }

            if (node.Nodes.Any())
            {
                var nodeName = node.Name.ToShortName();
                builder.AppendLine($"{indent}partition \"{node.Type} {nodeName}\" " + "{");
                GetPlantUml(indentCount + 2, node.Nodes, builder, pins, nodeName);
                builder.AppendLine($"{indent}" + "}");
            }

            foreach (var flow in node.Flows)
            {
                var source = GetNodeName(flow.Source, parentName, pins);
                var target = GetNodeName(flow.Target, parentName, pins);

                switch (flow.Status)
                {
                    case FlowStatus.Activated:
                        builder.AppendLine("skinparam {");
                        builder.AppendLine("ArrowColor green");
                        builder.AppendLine("ArrowFontColor green");
                        builder.AppendLine("}");
                        break;
                    
                    case FlowStatus.NotActivated:
                        builder.AppendLine("skinparam {");
                        builder.AppendLine("ArrowColor orange");
                        builder.AppendLine("ArrowFontColor orange");
                        builder.AppendLine("}");
                        break;
                }

                var stereotype = flow.Target.Status switch
                {
                    NodeStatus.Executed => "<<executed>>",
                    NodeStatus.Active => "<<active>>",
                    NodeStatus.Omitted => "<<omitted>>",
                    NodeStatus.Failed => "<<failed>>",
                    NodeStatus.NotUsed => "",
                    _ => ""
                };
                
                if (flow.TokenName != "NodeReferenceToken")
                {
                    if (flow.TokenName == typeof(ControlToken).GetTokenName())
                    {
                        builder.AppendLine($"{indent}{source} --> {target} {stereotype}");
                    }
                    else
                    {
                        var tokenCountInfo = flow.Status == FlowStatus.NotUsed
                            ? flow.Weight > 1
                                ? $" (expected {flow.Weight})"
                                : ""
                            : flow.Weight > 1
                                ? $" (passed {flow.TokenCount} of expected {flow.Weight})"
                                : flow.Weight == 0
                                    ? $" (optional, passed {flow.TokenCount})"
                                    : $" (passed {flow.TokenCount})";
                        
                        builder.AppendLine($"{indent}{source} -->[{flow.TokenName.ToShortName()}{tokenCountInfo}] {target} {stereotype}");
                    }
                }
                
                builder.AppendLine("skinparam {");
                builder.AppendLine("ArrowColor black");
                builder.AppendLine("ArrowFontColor black");
                builder.AppendLine("}");
            }
        }

        public static string GetPlantUml(this IActivityInspection activity)
        {
            var builder = new StringBuilder();
            var pins = new Dictionary<INodeInspection, string>();
            var nodes = FlattenNodes(activity.Nodes).ToArray();
            var names = nodes.Select(node => node.Name.ToShortName()).ToHashSet();
            var index = 0;
            foreach (var node in nodes.Where(node => node.PinTokenType != null &&
                (node.Type == NodeType.Input || node.Type == NodeType.Output)))
            {
                string alias;
                do
                {
                    alias = $"pin{index++}";
                } while (!names.Add(alias));
                pins.Add(node, alias);
            }

            builder.AppendLine("@startuml");
            
            
            builder.AppendLine("<style>");
            builder.AppendLine("    .executed {");
            builder.AppendLine("        LineColor green");
            builder.AppendLine("        FontColor green");
            builder.AppendLine("    }");
            builder.AppendLine("    .omitted {");
            builder.AppendLine("        LineColor orange");
            builder.AppendLine("        FontColor orange");
            builder.AppendLine("    }");
            builder.AppendLine("    .active {");
            builder.AppendLine("        LineColor blue");
            builder.AppendLine("        FontColor blue");
            builder.AppendLine("    }");
            builder.AppendLine("    .failed {");
            builder.AppendLine("        LineColor red");
            builder.AppendLine("        FontColor red");
            builder.AppendLine("    }");
            builder.AppendLine("</style>");

            GetPlantUml(0, activity.Nodes, builder, pins);

            builder.AppendLine("@enduml");

            return builder.ToString();
        }

        private static IEnumerable<INodeInspection> FlattenNodes(IEnumerable<INodeInspection> nodes)
        {
            foreach (var node in nodes)
            {
                yield return node;
                foreach (var child in FlattenNodes(node.Nodes))
                    yield return child;
            }
        }

        public static string GetPlantUmlUrl(this IActivityInspection activity)
        {
            return "http://www.plantuml.com/plantuml/png/" + PlantUmlTextEncoder.Encode(activity.GetPlantUml());
        }
    }
}

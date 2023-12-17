using System;

namespace Stateflows.Activities.Models
{
    internal class Edge : Element
    {
        public override string Identifier => TokenType != TargetTokenType
            ? $"{Source.Identifier}-{TokenType}|{TargetTokenType}->{Target.Identifier}"
            : $"{Source.Identifier}-{TokenType}->{Target.Identifier}";

        public Graph Graph { get; set; }
        public Type TokenType { get; set; }
        public Type TargetTokenType { get; set; }
        private Pipeline<PipelineActionAsync> pipeline = null;
        public Pipeline<PipelineActionAsync> Pipeline => pipeline ??= new Pipeline<PipelineActionAsync>() { Graph = Graph };
        public string SourceName { get; set; }
        public Node Source { get; set; }
        public string TargetName { get; set; }
        public Node Target { get; set; }
        public int Weight { get; set; } = 1;
    }
}

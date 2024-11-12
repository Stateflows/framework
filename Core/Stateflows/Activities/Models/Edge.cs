using System;

namespace Stateflows.Activities.Models
{
    internal class Edge : Element
    {
        public override string Identifier { get; set; }

        public Graph Graph { get; set; }
        public Type TokenType { get; set; }
        public Type TargetTokenType { get; set; }

        public Pipeline<TokenPipelineActionAsync> TokenPipeline { get; } = new Pipeline<TokenPipelineActionAsync>();

        public string SourceName { get; set; }
        public Node Source { get; set; }
        public string TargetName { get; set; }
        public Node Target { get; set; }
        public int Weight { get; set; } = 1;
        public bool IsElse { get; set; }
    }
}

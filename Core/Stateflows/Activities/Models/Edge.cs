using System;

namespace Stateflows.Activities.Models
{
    internal class Edge : Element
    {
        private string identifier = null;
        public override string Identifier
            => identifier ??= TokenType != TargetTokenType
                ? $"{Source.Identifier}-{TokenType}|{TargetTokenType}->{Target.Identifier}"
                : $"{Source.Identifier}-{TokenType}->{Target.Identifier}";

        public Graph Graph { get; set; }
        public Type TokenType { get; set; }
        public Type TargetTokenType { get; set; }

        private Pipeline<TokenPipelineActionAsync> tokenPipeline = null;
        public Pipeline<TokenPipelineActionAsync> TokenPipeline
            => tokenPipeline ??= new Pipeline<TokenPipelineActionAsync>() { Graph = Graph };

        public string SourceName { get; set; }
        public Node Source { get; set; }
        public string TargetName { get; set; }
        public Node Target { get; set; }
        public int Weight { get; set; } = 1;
        public bool IsElse { get; set; }
    }
}

using System;
using Stateflows.Common;

namespace Stateflows.Activities.Models
{
    internal class Edge : Element
    {
        private string identifier = null;
        public override string Identifier
            => identifier ??= TokenType != TargetTokenType
                ? $"{Source.Identifier}-{TokenTypeDescriptor}|{TargetTokenTypeDescriptor}->{Target.Identifier}"
                : $"{Source.Identifier}-{TargetTokenTypeDescriptor}->{Target.Identifier}";

        public Graph Graph { get; set; }
        public Type TokenType { get; set; }
        private string TokenTypeDescriptor
            => TokenType.GetTokenName();

        public Type TargetTokenType { get; set; }
        private string targetTokenTypeDescriptor = null;
        private string TargetTokenTypeDescriptor
            => targetTokenTypeDescriptor ??= IsElse
                ? $"{TargetTokenType.GetTokenName()}|else"
                : TargetTokenType.GetTokenName();

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

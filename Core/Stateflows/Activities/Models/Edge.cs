using System;

namespace Stateflows.Activities.Models
{
    internal class Edge : Element
    {
        public override string Identifier { get; set; }
        internal string DefinitionIdentifier { get; set; }

        public Graph Graph { get; set; }
        public Type TokenType { get; set; }
        public Type TargetTokenType { get; set; }

        /// <summary>Original payload names of a payload flow (D30), or <c>null</c> for a typed flow.</summary>
        public System.Collections.Generic.IReadOnlyList<string> TokenNames { get; set; }

        /// <summary>Transformed payload names (D30, R16), or <c>null</c> when the transformed token is typed.</summary>
        public System.Collections.Generic.IReadOnlyList<string> TargetTokenNames { get; set; }

        /// <summary>Selects the source tokens carried by this flow.</summary>
        public TokenKey SourceKey => TokenKey.Of(TokenType, TokenNames);

        /// <summary>Describes the tokens delivered to the target.</summary>
        public TokenKey TargetKey => TokenKey.Of(TargetTokenType, TargetTokenNames);

        public Pipeline<TokenPipelineActionAsync> TokenPipeline { get; } = new Pipeline<TokenPipelineActionAsync>();

        public string SourceName { get; set; }
        public Node Source { get; set; }
        public string TargetName { get; set; }
        public Node Target { get; set; }
        public int Weight { get; set; } = 1;
        public bool IsElse { get; set; }
        public bool IsCrossScope { get; set; }
        public Node Owner { get; set; }
        internal Type ImplementationType { get; set; }
        internal System.Collections.Generic.List<FlowOperationRegistration> Operations { get; } = [];
    }
}

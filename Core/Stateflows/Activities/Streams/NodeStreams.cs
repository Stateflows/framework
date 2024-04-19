using System.Collections.Generic;
using Newtonsoft.Json;
using Stateflows.Activities.Models;

namespace Stateflows.Activities.Streams
{
    //internal class NodeStreams
    //{
    //    public string NodeIdentifier { get; set; }

    //    [JsonIgnore]
    //    public Graph Graph { get; private set; }

    //    [JsonIgnore]
    //    private Node node = null;

    //    [JsonIgnore]
    //    public Node Node
    //    {
    //        get
    //        {
    //            if (node is null && Graph.AllNodes.TryGetValue(NodeIdentifier, out Node e))
    //            {
    //                node = e;
    //            }

    //            return node;
    //        }
    //    }

    //    public Dictionary<Edge, Stream> Inputs { get; set; } = new Dictionary<Edge, Stream>();

    //    [JsonIgnore]
    //    public Dictionary<Edge, Stream> Outputs { get; set; } = new Dictionary<Edge, Stream>();

    //    //public void Output<TToken>(TToken token)
    //    //    // where TToken : Token, new();
    //    //{
    //    //    var controlTokenName = TokenInfo<ControlToken>.Name;
    //    //    var tokenName = token.Name;

    //    //    foreach (var edgeOutput in Outputs)
    //    //    {
    //    //        var edgeTokenName = edgeOutput.Key.TokenType.GetTokenName();

    //    //        if (
    //    //            (edgeTokenName == TokenInfo<TToken>.Name) ||
    //    //            (edgeTokenName != controlTokenName && tokenName == controlTokenName)
    //    //        )
    //    //        {
    //    //            edgeOutput.Value.Consume(token);
    //    //        }
    //    //    }
    //    //}
    //}
}

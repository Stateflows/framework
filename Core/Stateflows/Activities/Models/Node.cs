using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Stateflows.Common.Models;
using Stateflows.Activities.Registration;
using Stateflows.Activities.Context.Classes;

namespace Stateflows.Activities.Models
{
    internal class Node : Element
    {
        public override string Identifier => !(Parent is null)
            ? $"{Type}:{Parent.Name}:{Name}"
            : $"{Type}:{Name}";

        public Graph Graph { get; set; }
        public Node Parent { get; set; }
        public string Name { get; set; }
        public NodeType Type { get; set; }
        public NodeOptions Options { get; set; } = NodeOptions.Default;
        public Type ExceptionType { get; set; }
        public Type EventType { get; set; }

        private Logic<ActivityActionAsync> action = null;
        public Logic<ActivityActionAsync> Action
            => action ??= new Logic<ActivityActionAsync>()
                {
                    Name = Constants.Action
                };

        public List<Edge> Edges { get; set; } = new List<Edge>();
        public List<Edge> IncomingEdges { get; set; } = new List<Edge>();
        public Dictionary<string, Node> Nodes { get; set; } = new Dictionary<string, Node>();
        public Dictionary<string, Node> NamedNodes { get; set; } = new Dictionary<string, Node>();

        //public bool DeclaredTypesSet { get; set; } = false;

        //public List<Type> ConsumedTypes { get; set; } = new List<Type>();

        //public List<Type> ProducedTypes { get; set; } = new List<Type>();

        //public void ScanForDeclaredTypes(Type nodeType)
        //{
        //    DeclaredTypesSet = true;

        //    var interfaces = nodeType.GetInterfaces().Where(t => 
        //        typeof(INodeInterface).IsAssignableFrom(t) &&
        //        (
        //            t.FullName.StartsWith("Stateflows.Activities.IProduces") ||
        //            t.FullName.StartsWith("Stateflows.Activities.IConsumes")
        //        ) &&
        //        t.GetGenericArguments().Length == 1
        //    );

        //    foreach (var type in interfaces)
        //    {
        //        var tokenType = type.GetGenericArguments().First();

        //        if (type.FullName.StartsWith("Stateflows.Activities.IProduces"))
        //        {
        //            ProducedTypes.Add(tokenType);
        //        }

        //        if (type.FullName.StartsWith("Stateflows.Activities.IConsumes"))
        //        {
        //            ConsumedTypes.Add(tokenType);
        //        }
        //    }
        //}

        private Logic<ActivityEventActionAsync> initialize = null;
        public Logic<ActivityEventActionAsync> Initialize
            => initialize ??= new Logic<ActivityEventActionAsync>()
            {
                Name = Constants.Initialize
            };

        private Logic<ActivityEventActionAsync> finalize = null;
        public Logic<ActivityEventActionAsync> Finalize
            => finalize ??= new Logic<ActivityEventActionAsync>()
            {
                Name = Constants.Finalize
            };

        private IEnumerable<Node> initialNodes = null;
        public IEnumerable<Node> InitialNodes
            => initialNodes ??= Nodes.Values
                .Where(n => n.Type == NodeType.Initial);

        private Node inputNode = null;
        private bool inputNodeSet = false;
        public Node InputNode
        {
            get
            {
                if (!inputNodeSet)
                {
                    inputNode = Nodes.Values.FirstOrDefault(n => n.Type == NodeType.Input);
                    inputNodeSet = true;
                }

                return inputNode;
            }
        }

        private Node outputNode = null;
        private bool outputNodeSet = false;
        public Node OutputNode
        {
            get
            {
                if (!outputNodeSet)
                {
                    outputNode = Nodes.Values.FirstOrDefault(n => n.Type == NodeType.Output);
                    outputNodeSet = true;
                }

                return outputNode;
            }
        }

        private IEnumerable<Node> exceptionHandlers = null;
        public IEnumerable<Node> ExceptionHandlers
            => exceptionHandlers ??= Edges
                .Select(e => e.Target)
                .Where(n => n.Type == NodeType.ExceptionHandler);

        public async Task<IEnumerable<Token>> HandleExceptionAsync(Exception exception, BaseContext context)
        {
            Node handler = null;
            var currentNode = this;
            while (handler == null && currentNode != null)
            {
                handler = currentNode.ExceptionHandlers.FirstOrDefault(n => exception.GetType().IsAssignableFrom(n.ExceptionType));

                currentNode = currentNode.Parent;
            }

            if (handler != null)
            {
                var exceptionContext = new ActionContext(
                    context.Context,
                    context.NodeScope,
                    this,
                    new Token[] { new ExceptionToken<Exception>() { Exception = exception } }
                );

                await handler.Action.WhenAll(exceptionContext);

                return exceptionContext.OutputTokens;
            }

            return new Token[0];
        }
    }
}

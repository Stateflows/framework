using System;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Engine;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Context;
using Stateflows.StateMachines.Context.Classes;

namespace Stateflows.StateMachines.Engine
{
    internal class Executor : IDisposable
    {
        public Graph Graph { get; }

        public StateMachinesRegister Register { get; }

        public IServiceProvider ServiceProvider { get; private set; }

        private IServiceScope Scope { get; }

        public Executor(StateMachinesRegister register, Graph graph, IServiceProvider serviceProvider)
        {
            Register = register;
            Graph = graph;

            Scope = serviceProvider.CreateScope();
            ServiceProvider = Scope.ServiceProvider;
        }

        public RootContext Context { get; private set; }

        private Inspector observer;

        public Inspector Observer
            => observer ?? (observer = new Inspector(this));

        private List<Vertex> verticesStack = null;

        public async Task<List<Vertex>> GetVerticesStackAsync(bool forceRebuild)
        {
            if (verticesStack == null || forceRebuild)
            {
                Debug.Assert(Context != null, $"Context is not available. Is state machine '{Graph.Name}' hydrated?");

                verticesStack = BuildVerticesStack();

                if (verticesStack.Count == 0)
                {
                    await DoInitializeAsync(new InitializationRequest());
                }
                else
                {
                    if (verticesStack.Last().Vertices.Count > 0)
                    {
                        await DoInitializeCascadeAsync(verticesStack.Last().InitialVertex);
                    }
                }

                verticesStack = BuildVerticesStack();
            }

            return verticesStack;
        }

        private List<Vertex> BuildVerticesStack()
        {
            var result = new List<Vertex>();
            var vertices = Graph.Vertices;
            for (int i = 0; i < Context.StatesStack.Count(); i++)
            {
                var vertexName = Context.StatesStack[i];
                if (vertices.TryGetValue(vertexName, out var vertex))
                {
                    result.Add(vertex);
                    vertices = vertex.Vertices;
                }
                else
                {
                    while (Context.StatesStack.Count() > i)
                    {
                        Context.StatesStack.RemoveAt(i);
                    }

                    break;
                }
            }

            return result;
        }

        public async Task<StateDescriptor> GetCurrentStateAsync()
        {
            var currentStack = await GetVerticesStackAsync(false);
            currentStack.Reverse();
            StateDescriptor stateDescriptor = null;
            foreach (var vertex in currentStack)
            {
                stateDescriptor = new StateDescriptor()
                {
                    Name = vertex.Name,
                    InnerState = stateDescriptor
                };
            }

            return stateDescriptor;
        }

        public async Task<bool> Hydrate(RootContext context)
        {
            Context = context;
            Context.Executor = this;

            await Observer.AfterHydrateAsync(new StateMachineActionContext(Context));

            return true;
        }

        public async Task<RootContext> Dehydrate()
        {
            Debug.Assert(Context != null, $"Context is unavailable. Is state machine '{Graph.Name}' already dehydrated?");

            await Observer.BeforeDehydrateAsync(new StateMachineActionContext(Context));

            var result = Context;
            result.Executor = null;
            Context = null;

            return result;
        }

        public bool Initialized
        {
            get
            {
                Debug.Assert(Context != null, $"Context is unavailable. Is state machine '{Graph.Name}' hydrated?");

                return Context.StatesStack.Count > 0;
            }
        }

        public async Task<bool> Initialize(InitializationRequest @event)
        {
            Debug.Assert(Context != null, $"Context is unavailable. Is state machine '{Graph.Name}' hydrated?");

            if (!Initialized)
            {
                await DoInitializeAsync(@event);

                await DoInitializeCascadeAsync(Graph.InitialVertex);
                    
                return true;
            }

            return false;
        }

        private async Task DoInitializeCascadeAsync(Vertex vertex)
        {
            while (vertex != null)
            {
                if (vertex.Parent != null)
                {
                    await DoInitializeAsync(vertex.Parent);
                }

                await DoEntryAsync(vertex);
                Context.StatesStack.Add(vertex.Name);

                vertex = vertex.InitialVertex;
            }
        }

        public async Task<IEnumerable<Type>> GetExpectedEventsAsync()
        {
            var currentStack = await GetVerticesStackAsync(false);
            currentStack.Reverse();

            return currentStack.SelectMany(vertex => vertex.Edges).Select(edge => edge.TriggerType).Distinct();
        }

        private List<Vertex> GetNestedVertices(Vertex vertex)
        {
            var result = new List<Vertex>() { vertex };
            foreach (var childVertex in vertex.Vertices.Values)
            {
                result.AddRange(GetNestedVertices(childVertex));
            }

            return result;
        }

        public async Task<bool> ProcessAsync<TEvent>(TEvent @event)
            where TEvent : Event, new()
        {
            var result = false;

            if (Initialized)
            {
                result = await DoProcessAsync(@event);
            }

            ServiceProvider = null;

            return result;
        }

        private async Task<bool> DoProcessAsync<TEvent>(TEvent @event)
            where TEvent : Event, new()
        {
            Debug.Assert(Context != null, $"Context is not available. Is state machine '{Graph.Name}' hydrated?");

            var currentStack = await GetVerticesStackAsync(true);
            currentStack.Reverse();

            foreach (var vertex in currentStack)
            {
                foreach (var edge in vertex.Edges)
                {
                    if (edge.Trigger == @event.Name)
                    {
                        if (await DoGuardAsync(edge, @event))
                        {
                            await DoConsumeAsync<TEvent>(vertex, edge);

                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private async Task<bool> DoGuardAsync<TEvent>(Edge edge, TEvent @event)
            where TEvent : Event, new()
        {
            Context.Context.Values[Constants.SourceState] = edge.SourceName;
            Context.Context.Values[Constants.TargetState] = edge.TargetName;
            Context.Context.Values[Constants.Event] = @event;

            var context = new GuardContext<TEvent>(Context, edge);
            await Observer.BeforeTransitionGuardAsync(context);

            var result = await edge.Guards.WhenAll(Context);

            await Observer.AfterGuardAsync(context, result);

            return result;
        }

        private async Task DoEffectAsync<TEvent>(Edge edge)
            where TEvent : Event, new()
        {
            var context = new TransitionContext<TEvent>(Context, edge);
            await Observer.BeforeEffectAsync(context);

            await edge.Effects.WhenAll(Context);

            await Observer.AfterEffectAsync(context);
        }

        public async Task DoInitializeAsync(InitializationRequest @event)
        {
            var context = new StateMachineActionContext(Context);
            await Observer.BeforeStateMachineInitializeAsync(context);

            if (@event.Values != null)
            {
                foreach (var key in @event.Values.Keys)
                {
                    context.StateMachine.GlobalValues.Set(key, @event.Values[key]);
                }
            }

            await Graph.Initialize.WhenAll(Context);

            await Observer.AfterStateMachineInitializeAsync(context);
        }

        public async Task DoInitializeAsync(Vertex vertex)
        {
            Context.Context.Values[Constants.State] = vertex.Name;
            var context = new StateActionContext(Context, vertex, Constants.Entry);
            await Observer.BeforeStateInitializeAsync(context);

            await vertex.Initialize.WhenAll(Context);

            await Observer.AfterStateInitializeAsync(context);
        }

        public async Task DoEntryAsync(Vertex vertex)
        {
            Context.Context.Values[Constants.State] = vertex.Name;
            var context = new StateActionContext(Context, vertex, Constants.Entry);
            await Observer.BeforeStateEntryAsync(context);

            await vertex.Entry.WhenAll(Context);

            await Observer.AfterStateEntryAsync(context);
        }

        private async Task DoExitAsync(Vertex vertex)
        {
            Context.Context.Values[Constants.State] = vertex.Name;
            var context = new StateActionContext(Context, vertex, Constants.Exit);
            await Observer.BeforeStateExitAsync(context);

            await vertex.Exit.WhenAll(Context);

            await Observer.AfterStateExitAsync(context);
        }

        private async Task DoConsumeAsync<TEvent>(Vertex vertex, Edge edge)
            where TEvent : Event, new()
        {
            var nextVertex = edge.Target;
            if (nextVertex != null)
            {
                Context.StatesStack.Reverse();
                foreach (var state in Context.StatesStack)
                {
                    if (Graph.AllVertices.TryGetValue(state, out vertex))
                    {
                        if (vertex == nextVertex.Parent)
                        {
                            break;
                        }

                        await DoExitAsync(vertex);
                    }
                }
            }

            await DoEffectAsync<TEvent>(edge);

            if (nextVertex != null)
            {
                var previousNextVertex = nextVertex;

                Context.StatesStack.Clear();

                while (nextVertex != null)
                {
                    if (nextVertex.Parent != null && nextVertex.Parent.InitialVertex == nextVertex)
                    {
                        await DoInitializeAsync(nextVertex.Parent);
                    }
                    await DoEntryAsync(nextVertex);

                    previousNextVertex = nextVertex;
                    nextVertex = nextVertex.InitialVertex;
                }

                nextVertex = previousNextVertex;
                while (nextVertex != null)
                {
                    Context.StatesStack.Add(nextVertex.Name);
                    nextVertex = nextVertex.Parent;
                }
                Context.StatesStack.Reverse();
            }

            await DoProcessAsync(new Completion());
        }

        public void Dispose()
        {
            Scope.Dispose();
        }
    }
}

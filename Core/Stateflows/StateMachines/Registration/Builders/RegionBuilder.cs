using System;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Registration;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Exceptions;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Base;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Registration.Builders
{
    internal class RegionBuilder :
        IRegionBuilder,
        IInitializedRegionBuilder,
        IFinalizedRegionBuilder,
        IFinalizedOverridenRegionBuilder,
        IOverridenRegionBuilder,
        IInternal,
        IBehaviorBuilder
    {
        public Region Region { get; }

        public IServiceCollection Services { get; }

        BehaviorClass IBehaviorBuilder.BehaviorClass => new BehaviorClass(Constants.StateMachine, Region.Graph.Name);

        int IBehaviorBuilder.BehaviorVersion => Region.Graph.Version;

        public RegionBuilder(Region region, IServiceCollection services)
        {
            Region = region;
            Services = services;
            Builder = new StateBuilder(Region.ParentVertex, Services);
        }

        private StateBuilder Builder { get; set; }

        [DebuggerHidden]
        private IInitializedRegionBuilder AddVertex(string stateName, VertexType type, Action<Vertex> vertexBuildAction = null)
        {
            if (string.IsNullOrEmpty(stateName))
            {
                throw new StateDefinitionException(stateName, $"State name cannot be empty", Region.Graph.Class);
            }

            if (Region.Vertices.ContainsKey(stateName))
            {
                throw new StateDefinitionException(stateName, $"State '{stateName}' is already registered", Region.Graph.Class);
            }

            var vertex = new Vertex()
            {
                Name = stateName,
                Type = type,
                ParentRegion = Region,
                Graph = Region.Graph,
            };

            vertexBuildAction?.Invoke(vertex);

            Region.Vertices.Add(vertex.Name, vertex);
            Region.Graph.AllVertices.Add(vertex.Identifier, vertex);

            return this;
        }

        #region AddState
        [DebuggerHidden]
        public IInitializedRegionBuilder AddState(string stateName, StateBuildAction stateBuildAction = null)
            => AddVertex(stateName, VertexType.State, vertex => stateBuildAction?.Invoke(new StateBuilder(vertex, Services)));

        [DebuggerHidden]
        public IInitializedRegionBuilder AddJunction(string junctionName, JunctionBuildAction junctionBuildAction)
            => AddVertex(junctionName, VertexType.Junction, vertex => junctionBuildAction?.Invoke(new StateBuilder(vertex, Services)));

        [DebuggerHidden]
        public IInitializedRegionBuilder AddChoice(string choiceName, ChoiceBuildAction choiceBuildAction)
            => AddVertex(choiceName, VertexType.Choice, vertex => choiceBuildAction?.Invoke(new StateBuilder(vertex, Services)));

        [DebuggerHidden]
        IOverridenRegionBuilder IStateMachine<IOverridenRegionBuilder>.AddCompositeState(string compositeStateName,
            CompositeStateBuildAction compositeStateBuildAction)
            => AddCompositeState(compositeStateName, compositeStateBuildAction) as IOverridenRegionBuilder;

        [DebuggerHidden]
        IOverridenRegionBuilder IStateMachine<IOverridenRegionBuilder>.AddOrthogonalState(string orthogonalStateName,
            OrthogonalStateBuildAction orthogonalStateBuildAction)
            => AddOrthogonalState(orthogonalStateName, orthogonalStateBuildAction) as IOverridenRegionBuilder;

        [DebuggerHidden]
        IOverridenRegionBuilder IStateMachine<IOverridenRegionBuilder>.AddJunction(string junctionName, JunctionBuildAction junctionBuildAction)
            => AddJunction(junctionName, junctionBuildAction) as IOverridenRegionBuilder;

        [DebuggerHidden]
        IOverridenRegionBuilder IStateMachine<IOverridenRegionBuilder>.AddChoice(string choiceName, ChoiceBuildAction choiceBuildAction)
            => AddChoice(choiceName, choiceBuildAction) as IOverridenRegionBuilder;

        #endregion

        #region AddFinalState
        [DebuggerHidden]
        public IFinalizedRegionBuilder AddFinalState(string finalStateName = FinalState.Name)
            => AddVertex(finalStateName, VertexType.FinalState) as IFinalizedRegionBuilder;
        #endregion

        #region AddCompositeState
        [DebuggerHidden]
        IOverridenRegionBuilder IStateMachine<IOverridenRegionBuilder>.AddState(string stateName, StateBuildAction stateBuildAction)
            => AddState(stateName, stateBuildAction) as IOverridenRegionBuilder;

        [DebuggerHidden]
        public IInitializedRegionBuilder AddCompositeState(string compositeStateName, CompositeStateBuildAction compositeStateBuildAction)
            => AddVertex(compositeStateName, VertexType.CompositeState, vertex => compositeStateBuildAction?.Invoke(new CompositeStateBuilder(vertex.DefaultRegion, Services)));

        [DebuggerHidden]
        public IInitializedRegionBuilder AddOrthogonalState(string orthogonalStateName, OrthogonalStateBuildAction orthogonalStateBuildAction)
            => AddVertex(orthogonalStateName, VertexType.OrthogonalState, vertex => orthogonalStateBuildAction?.Invoke(new OrthogonalStateBuilder(vertex, Services)));

        [DebuggerHidden]
        public IInitializedRegionBuilder AddInitialState(string stateName, StateBuildAction stateBuildAction = null)
        {
            Region.InitialVertexName = stateName;
            return AddVertex(stateName, VertexType.InitialState, vertex => stateBuildAction?.Invoke(new StateBuilder(vertex, Services)));
        }

        [DebuggerHidden]
        public IInitializedRegionBuilder AddInitialCompositeState(string compositeStateName, CompositeStateBuildAction compositeStateBuildAction)
        {
            Region.InitialVertexName = compositeStateName;
            return AddVertex(compositeStateName, VertexType.InitialCompositeState, vertex => compositeStateBuildAction?.Invoke(new CompositeStateBuilder(vertex.DefaultRegion, Services)));
        }

        [DebuggerHidden]
        public IInitializedRegionBuilder AddInitialOrthogonalState(string orthogonalStateName, OrthogonalStateBuildAction orthogonalStateBuildAction)
        {
            Region.InitialVertexName = orthogonalStateName;
            return AddVertex(orthogonalStateName, VertexType.InitialOrthogonalState, vertex => orthogonalStateBuildAction?.Invoke(new OrthogonalStateBuilder(vertex, Services)));
        }
        #endregion

        public IOverridenRegionBuilder UseState(string stateName, OverridenStateBuildAction stateBuildAction)
        {
            if (
                !Region.Vertices.TryGetValue(stateName, out var vertex) ||
                (
                    vertex.Type != VertexType.State && 
                    vertex.Type != VertexType.InitialState
                ) || 
                vertex.OriginStateMachineName == null
            )
            {
                throw new StateMachineOverrideException($"State '{stateName}' not found in overriden composite state '{Region.ParentVertex.Name}'", Region.Graph.Class);
            }
            
            stateBuildAction?.Invoke(new StateBuilder(vertex, Services));

            return this;
        }

        public IOverridenRegionBuilder UseCompositeState(string compositeStateName,
            OverridenCompositeStateBuildAction compositeStateBuildAction)
        {
            if (
                !Region.Vertices.TryGetValue(compositeStateName, out var vertex) ||
                (
                    vertex.Type != VertexType.CompositeState &&
                    vertex.Type != VertexType.InitialCompositeState
                ) ||
                vertex.OriginStateMachineName == null
            )
            {
                throw new StateMachineOverrideException($"Composite state '{compositeStateName}' not found in overriden composite state '{Region.ParentVertex.Name}'", Region.Graph.Class);
            }

            compositeStateBuildAction?.Invoke(new CompositeStateBuilder(vertex.DefaultRegion, Services));

            return this;
        }

        public IOverridenRegionBuilder UseOrthogonalState(string orthogonalStateName,
            OverridenOrthogonalStateBuildAction orthogonalStateBuildAction)
        {
            if (
                !Region.Vertices.TryGetValue(orthogonalStateName, out var vertex) ||
                (
                    vertex.Type != VertexType.OrthogonalState &&
                    vertex.Type != VertexType.InitialOrthogonalState
                ) ||
                vertex.OriginStateMachineName == null
            )
            {
                throw new StateMachineOverrideException($"Orthogonal state '{orthogonalStateName}' not found in overriden composite state '{Region.ParentVertex.Name}'", Region.Graph.Class);
            }

            orthogonalStateBuildAction?.Invoke(new OrthogonalStateBuilder(vertex, Services));

            return this;
        }

        public IOverridenRegionBuilder UseJunction(string junctionName, OverridenJunctionBuildAction junctionBuildAction)
        {
            if (!Region.Vertices.TryGetValue(junctionName, out var vertex) || vertex.Type != VertexType.Junction || vertex.OriginStateMachineName == null)
            {
                throw new StateMachineOverrideException($"Junction '{junctionName}' not found in overriden composite state '{Region.ParentVertex.Name}'", Region.Graph.Class);
            }
            
            junctionBuildAction?.Invoke(new StateBuilder(vertex, Services));

            return this;
        }

        public IOverridenRegionBuilder UseChoice(string choiceName, OverridenChoiceBuildAction choiceBuildAction)
        {
            if (!Region.Vertices.TryGetValue(choiceName, out var vertex) || vertex.Type != VertexType.Choice || vertex.OriginStateMachineName == null)
            {
                throw new StateMachineOverrideException($"Choice '{choiceName}' not found in overriden composite state '{Region.ParentVertex.Name}'", Region.Graph.Class);
            }
            
            choiceBuildAction?.Invoke(new StateBuilder(vertex, Services));

            return this;
        }

        IFinalizedOverridenRegionBuilder IStateMachineFinal<IFinalizedOverridenRegionBuilder>.AddFinalState(string finalStateName)
            => AddFinalState(finalStateName) as IFinalizedOverridenRegionBuilder;

        IOverridenRegionBuilder IStateMachineOverrides<IOverridenRegionBuilder>.UseOrthogonalState(string orthogonalStateName, OverridenOrthogonalStateBuildAction orthogonalStateBuildAction)
            => UseOrthogonalState(orthogonalStateName, orthogonalStateBuildAction) as IOverridenRegionBuilder;

        IFinalizedOverridenRegionBuilder IStateMachineOverrides<IFinalizedOverridenRegionBuilder>.UseState(string stateName, OverridenStateBuildAction stateBuildAction)
        {
            throw new NotImplementedException();
        }

        IFinalizedOverridenRegionBuilder IStateMachineOverrides<IFinalizedOverridenRegionBuilder>.UseCompositeState(string compositeStateName, OverridenCompositeStateBuildAction compositeStateBuildAction)
        {
            throw new NotImplementedException();
        }

        IFinalizedOverridenRegionBuilder IStateMachineOverrides<IFinalizedOverridenRegionBuilder>.UseOrthogonalState(string orthogonalStateName, OverridenOrthogonalStateBuildAction orthogonalStateBuildAction)
        {
            throw new NotImplementedException();
        }

        IFinalizedOverridenRegionBuilder IStateMachineOverrides<IFinalizedOverridenRegionBuilder>.UseJunction(string junctionName, OverridenJunctionBuildAction junctionBuildAction)
        {
            throw new NotImplementedException();
        }

        IFinalizedOverridenRegionBuilder IStateMachineOverrides<IFinalizedOverridenRegionBuilder>.UseChoice(string choiceName, OverridenChoiceBuildAction choiceBuildAction)
        {
            throw new NotImplementedException();
        }
    }
}

using System;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Extensions;
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
        IBehaviorBuilder
    {
        public Region Region { get; }

        BehaviorClass IBehaviorBuilder.BehaviorClass => new BehaviorClass(Constants.StateMachine, Region.Graph.Name);

        int IBehaviorBuilder.BehaviorVersion => Region.Graph.Version;

        public RegionBuilder(Region region)
        {
            Region = region;
            Builder = new StateBuilder(Region.ParentVertex);
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
            
            Region.Graph.VisitingTasks.Add(visitor => visitor.VertexAddedAsync(Region.Graph.Name, Region.Graph.Version, vertex.Name, vertex.Type));

            return this;
        }

        #region AddState
        [DebuggerHidden]
        public IInitializedRegionBuilder AddState(string stateName, StateBuildAction stateBuildAction = null)
            => AddVertex(stateName, VertexType.State, vertex => stateBuildAction?.Invoke(new StateBuilder(vertex)));

        [DebuggerHidden]
        public IInitializedRegionBuilder AddJunction(string junctionName, JunctionBuildAction junctionBuildAction)
            => AddVertex(junctionName, VertexType.Junction, vertex => junctionBuildAction?.Invoke(new StateBuilder(vertex)));

        [DebuggerHidden]
        public IInitializedRegionBuilder AddChoice(string choiceName, ChoiceBuildAction choiceBuildAction)
            => AddVertex(choiceName, VertexType.Choice, vertex => choiceBuildAction?.Invoke(new StateBuilder(vertex)));

        IOverridenRegionBuilder IStateMachine<IOverridenRegionBuilder>.AddFork(string forkName, ForkBuildAction forkBuildAction)
            => AddFork(forkName, forkBuildAction) as IOverridenRegionBuilder;

        IOverridenRegionBuilder IStateMachine<IOverridenRegionBuilder>.AddJoin(string joinName, JoinBuildAction joinBuildAction)
            => AddJoin(joinName, joinBuildAction) as IOverridenRegionBuilder;

        public IInitializedRegionBuilder AddFork(string forkName, ForkBuildAction forkBuildAction)
            => AddVertex(forkName, VertexType.Fork, vertex => forkBuildAction?.Invoke(new StateBuilder(vertex)));

        public IInitializedRegionBuilder AddJoin(string joinName, JoinBuildAction joinBuildAction)
            => AddVertex(joinName, VertexType.Join, vertex => joinBuildAction?.Invoke(new StateBuilder(vertex)));

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
            => AddVertex(compositeStateName, VertexType.CompositeState, vertex => compositeStateBuildAction?.Invoke(new CompositeStateBuilder(vertex.DefaultRegion)));

        [DebuggerHidden]
        public IInitializedRegionBuilder AddOrthogonalState(string orthogonalStateName, OrthogonalStateBuildAction orthogonalStateBuildAction)
            => AddVertex(orthogonalStateName, VertexType.OrthogonalState, vertex => orthogonalStateBuildAction?.Invoke(new OrthogonalStateBuilder(vertex)));

        [DebuggerHidden]
        public IInitializedRegionBuilder AddInitialState(string stateName, StateBuildAction stateBuildAction = null)
        {
            Region.InitialVertexName = stateName;
            return AddVertex(stateName, VertexType.InitialState, vertex => stateBuildAction?.Invoke(new StateBuilder(vertex)));
        }

        [DebuggerHidden]
        public IInitializedRegionBuilder AddInitialCompositeState(string compositeStateName, CompositeStateBuildAction compositeStateBuildAction)
        {
            Region.InitialVertexName = compositeStateName;
            return AddVertex(compositeStateName, VertexType.InitialCompositeState, vertex => compositeStateBuildAction?.Invoke(new CompositeStateBuilder(vertex.DefaultRegion)));
        }

        [DebuggerHidden]
        public IInitializedRegionBuilder AddInitialOrthogonalState(string orthogonalStateName, OrthogonalStateBuildAction orthogonalStateBuildAction)
        {
            Region.InitialVertexName = orthogonalStateName;
            return AddVertex(orthogonalStateName, VertexType.InitialOrthogonalState, vertex => orthogonalStateBuildAction?.Invoke(new OrthogonalStateBuilder(vertex)));
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
            
            stateBuildAction?.Invoke(new StateBuilder(vertex));

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

            compositeStateBuildAction?.Invoke(new CompositeStateBuilder(vertex.DefaultRegion));

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

            orthogonalStateBuildAction?.Invoke(new OrthogonalStateBuilder(vertex));

            return this;
        }

        public IOverridenRegionBuilder UseJunction(string junctionName, OverridenJunctionBuildAction junctionBuildAction)
        {
            if (!Region.Vertices.TryGetValue(junctionName, out var vertex) || vertex.Type != VertexType.Junction || vertex.OriginStateMachineName == null)
            {
                throw new StateMachineOverrideException($"Junction '{junctionName}' not found in overriden composite state '{Region.ParentVertex.Name}'", Region.Graph.Class);
            }
            
            junctionBuildAction?.Invoke(new StateBuilder(vertex));

            return this;
        }

        public IOverridenRegionBuilder UseChoice(string choiceName, OverridenChoiceBuildAction choiceBuildAction)
        {
            if (!Region.Vertices.TryGetValue(choiceName, out var vertex) || vertex.Type != VertexType.Choice || vertex.OriginStateMachineName == null)
            {
                throw new StateMachineOverrideException($"Choice '{choiceName}' not found in overriden composite state '{Region.ParentVertex.Name}'", Region.Graph.Class);
            }
            
            choiceBuildAction?.Invoke(new StateBuilder(vertex));

            return this;
        }

        public IOverridenRegionBuilder UseFork(string forkName, OverridenForkBuildAction forkBuildAction)
        {
            if (!Region.Vertices.TryGetValue(forkName, out var vertex) || vertex.Type != VertexType.Fork || vertex.OriginStateMachineName == null)
            {
                throw new StateMachineOverrideException($"Fork '{forkName}' not found in overriden composite state '{Region.ParentVertex.Name}'", Region.Graph.Class);
            }
            
            forkBuildAction?.Invoke(new StateBuilder(vertex));

            return this;
        }

        public IOverridenRegionBuilder UseJoin(string joinName, OverridenJoinBuildAction joinBuildAction)
        {
            if (!Region.Vertices.TryGetValue(joinName, out var vertex) || vertex.Type != VertexType.Join || vertex.OriginStateMachineName == null)
            {
                throw new StateMachineOverrideException($"Join '{joinName}' not found in overriden composite state '{Region.ParentVertex.Name}'", Region.Graph.Class);
            }
            
            joinBuildAction?.Invoke(new StateBuilder(vertex));

            return this;
        }

        IFinalizedOverridenRegionBuilder IStateMachineOverrides<IFinalizedOverridenRegionBuilder>.UseFork(string forkName, OverridenForkBuildAction forkBuildAction)
            => UseFork(forkName, forkBuildAction) as IFinalizedOverridenRegionBuilder;

        IFinalizedOverridenRegionBuilder IStateMachineOverrides<IFinalizedOverridenRegionBuilder>.UseJoin(string joinName, OverridenJoinBuildAction joinBuildAction)
            => UseJoin(joinName, joinBuildAction) as IFinalizedOverridenRegionBuilder;

        IFinalizedOverridenRegionBuilder IStateMachineFinal<IFinalizedOverridenRegionBuilder>.AddFinalState(string finalStateName)
            => AddFinalState(finalStateName) as IFinalizedOverridenRegionBuilder;

        IOverridenRegionBuilder IStateMachineOverrides<IOverridenRegionBuilder>.UseOrthogonalState(string orthogonalStateName, OverridenOrthogonalStateBuildAction orthogonalStateBuildAction)
            => UseOrthogonalState(orthogonalStateName, orthogonalStateBuildAction);

        IFinalizedOverridenRegionBuilder IStateMachineOverrides<IFinalizedOverridenRegionBuilder>.UseState(string stateName, OverridenStateBuildAction stateBuildAction)
            => UseState(stateName, stateBuildAction) as IFinalizedOverridenRegionBuilder;

        IFinalizedOverridenRegionBuilder IStateMachineOverrides<IFinalizedOverridenRegionBuilder>.UseCompositeState(string compositeStateName, OverridenCompositeStateBuildAction compositeStateBuildAction)
            => UseCompositeState(compositeStateName, compositeStateBuildAction) as IFinalizedOverridenRegionBuilder;

        IFinalizedOverridenRegionBuilder IStateMachineOverrides<IFinalizedOverridenRegionBuilder>.UseOrthogonalState(string orthogonalStateName, OverridenOrthogonalStateBuildAction orthogonalStateBuildAction)
            => UseOrthogonalState(orthogonalStateName, orthogonalStateBuildAction) as IFinalizedOverridenRegionBuilder;

        IFinalizedOverridenRegionBuilder IStateMachineOverrides<IFinalizedOverridenRegionBuilder>.UseJunction(string junctionName, OverridenJunctionBuildAction junctionBuildAction)
            => UseJunction(junctionName, junctionBuildAction) as IFinalizedOverridenRegionBuilder;

        IFinalizedOverridenRegionBuilder IStateMachineOverrides<IFinalizedOverridenRegionBuilder>.UseChoice(string choiceName, OverridenChoiceBuildAction choiceBuildAction)
            => UseChoice(choiceName, choiceBuildAction) as IFinalizedOverridenRegionBuilder;
    }
}

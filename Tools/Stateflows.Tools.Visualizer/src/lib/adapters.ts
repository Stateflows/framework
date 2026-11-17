import type { DiagramDocument, DiagramOperation } from './model.js';
import { operationId, stateMachineView, transitionParts, transitionText } from './stateMachine.js';

export interface StateMachineDefinition {
  name: string;
  version: number;
  initialStateId: string | null;
  states: {
    id: string; name: string; kind: string; regionId: string | null;
    initializeActions: number; finalizeActions: number; entryActions: number; exitActions: number;
    deferredEvents: string[]; behaviorType: string | null; behaviorName: string | null;
    stateType?: string | null; entryNames?: string[]; exitNames?: string[]; behaviorImplementation?: string | null;
    initializeNames?: string[]; finalizeNames?: string[];
    deferrals?: { trigger: string; guards: string[] }[];
  }[];
  regions: { id: string; stateId: string; index: number; initialStateId: string | null; historyStateId: string | null }[];
  transitions: {
    id: string; sourceId: string; targetId: string | null; triggers: string[];
    isElse: boolean; isLocal: boolean; guards: number; effects: number;
    guardNames?: string[]; effectNames?: string[];
  }[];
}

export interface ActivityDefinition {
  name: string; version: number;
  nodes: {
    id: string; name: string; kind: string; parentId: string | null;
    pinTokenType: string | null; eventName: string | null; options: number | string;
  }[];
  flows: {
    id: string; sourceId: string; targetId: string; tokenType: string; targetTokenType: string;
    weight: number; isElse: boolean; isCrossScope: boolean;
  }[];
}

export interface ActionDefinition {
  name: string;
  version: number;
  implementation?: string | null;
}

export function fromAction(definition: ActionDefinition): DiagramDocument {
  return {
    id: documentId('action', definition.name, definition.version),
    name: definition.name, version: definition.version, family: 'action',
    nodes: [{ id: definition.name, label: definition.implementation ?? definition.name, kind: 'Action',
      details: ['C# action implementation'] }], edges: []
  };
}

export function documentId(family: DiagramDocument['family'], name: string, version: number): string {
  return JSON.stringify([family, name, version]);
}

export function fromStateMachine(definition: StateMachineDefinition): DiagramDocument {
  const initial = new Set([definition.initialStateId, ...definition.regions.map(region => region.initialStateId)]);
  return stateMachineView({
    id: documentId('stateMachine', definition.name, definition.version),
    name: definition.name, version: definition.version, family: 'stateMachine',
    nodes: [
      ...definition.states.map(state => {
        const embeddedBehavior = state.behaviorType && state.behaviorName
          ? { type: state.behaviorType, name: state.behaviorName } : undefined;
        const operations: DiagramOperation[] = [];
        for (const [operation, count, names, runtimeOperation] of [
          ['initialization', state.initializeActions, state.initializeNames, 'state.initialize'],
          ['entry', state.entryActions, state.entryNames, 'state.entry'],
          ['exit', state.exitActions, state.exitNames, 'state.exit'],
          ['finalization', state.finalizeActions, state.finalizeNames, 'state.finalize']
        ] as const) {
          operationText(operation, count, names, state.stateType).forEach((label, index) => operations.push({
            id: JSON.stringify([state.id, runtimeOperation, index]), selectionId: operationId(state.id, runtimeOperation),
            label, target: { elementId: state.id, operation: runtimeOperation }
          }));
        }
        for (const trigger of state.deferredEvents) {
          const target = { elementId: state.id, operation: `state.defer:${trigger}` };
          operations.push({
            id: operationId(target.elementId, target.operation), selectionId: operationId(target.elementId, target.operation),
            target, label: transitionText({ triggers: [trigger], isElse: false, guards: 0, effects: 1,
              effectNames: ['defer'], guardNames: state.deferrals?.find(item => item.trigger === trigger)?.guards })
          });
        }
        if (embeddedBehavior) operations.push({
          id: operationId(state.id, 'state.do'), selectionId: operationId(state.id, 'state.do'),
          target: { elementId: state.id, operation: 'state.do' },
          label: `do / ${state.behaviorImplementation ?? state.behaviorName}`, embeddedBehavior
        });
        return {
        id: state.id, label: state.name, kind: state.kind, parentId: state.regionId ?? undefined,
        embeddedBehavior, operations, entryBreakpoint: true,
        initial: initial.has(state.id),
        details: operations.map(operation => operation.label)
      }; }),
      ...definition.regions.map(region => ({
        id: region.id, label: `Region ${region.index + 1}`, kind: 'Region', parentId: region.stateId
      }))
    ],
    edges: definition.transitions.map(edge => ({
      triggers: edge.triggers,
      id: edge.id, source: edge.sourceId, target: edge.targetId ?? undefined,
      label: transitionText(edge),
      parts: transitionParts(edge),
      kind: 'transition', isElse: edge.isElse, isLocal: edge.isLocal
    }))
  });
}

function operationText(operation: string, count: number, names: readonly string[] = [], stateType?: string | null): string[] {
  const handlers = [...names];
  while (handlers.length < count) handlers.push('action');
  return handlers.map(name => `${operation} / ${name === stateType
    ? `on${operation[0].toUpperCase()}${operation.slice(1)}()` : name}`);
}

export function fromActivity(definition: ActivityDefinition): DiagramDocument {
  return {
    id: documentId('activity', definition.name, definition.version),
    name: definition.name, version: definition.version, family: 'activity',
    nodes: definition.nodes.map(node => ({
      id: node.id, label: node.name || node.kind, kind: node.kind, parentId: node.parentId ?? undefined,
      details: [node.pinTokenType, node.eventName].filter((value): value is string => value !== null)
    })),
    edges: definition.flows.map(flow => {
      const control = flow.tokenType.endsWith('ControlToken');
      const transform = flow.tokenType !== flow.targetTokenType;
      return {
        id: flow.id, source: flow.sourceId, target: flow.targetId,
        label: `${control ? '' : flow.tokenType}${transform ? ` => ${flow.targetTokenType}` : ''}${flow.weight !== 1 ? ` [weight ${flow.weight}]` : ''}${flow.isElse ? ' [else]' : ''}`,
        kind: control ? 'control' : transform ? 'transformation' : 'object',
        weight: flow.weight, isElse: flow.isElse
      };
    })
  };
}

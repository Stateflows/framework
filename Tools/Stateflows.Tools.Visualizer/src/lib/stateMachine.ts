import type { DiagramDocument, DiagramEdge, DiagramNode, DiagramOperation, LabelParts } from './model.js';

export function triggerNames(edge: DiagramEdge): readonly string[] {
  const names = edge.triggers ?? (edge.parts ? [edge.parts.trigger] : []);
  return [...new Set(names.map(name => ['', 'Completion', 'Stateflows.Common.Completion', 'Stateflows.StateMachines.Completion'].includes(name)
    ? 'default' : name))];
}

export function searchElements(document: DiagramDocument, query: string): string[] {
  const text = query.trim().toLowerCase();
  if (!text) return [];
  return [...new Set([
    ...document.nodes.filter(node => node.selectable !== false && node.label.toLowerCase().includes(text)).map(node => node.id),
    ...document.edges.filter(edge => edge.selectable !== false &&
      !(document.family === 'stateMachine' && edge.target === undefined) && edge.label.toLowerCase().includes(text)).map(edge => edge.id),
    ...document.nodes.flatMap(node => operationRows(node).filter(row => row.label.toLowerCase().includes(text)).map(row => row.selectionId))
  ])];
}

export function operationId(elementId: string, operation: string): string {
  return JSON.stringify(['operation', elementId, operation]);
}

export function operationRows(node: DiagramNode): readonly DiagramOperation[] {
  if (node.operations) return node.operations;
  const rows: DiagramOperation[] = (node.details ?? []).map((label, index) => ({
    id: operationId(node.id, `detail:${index}`), selectionId: operationId(node.id, `detail:${index}`),
    label, target: { elementId: node.id, operation: `detail:${index}` }, breakpointable: false
  }));
  if (node.embeddedBehavior) {
    const existing = rows.findIndex(row => row.label.startsWith('do /'));
    const row: DiagramOperation = {
      id: operationId(node.id, 'state.do'), selectionId: operationId(node.id, 'state.do'),
      label: existing < 0 ? `do / ${node.embeddedBehavior.name}` : rows[existing].label,
      target: { elementId: node.id, operation: 'state.do' }, embeddedBehavior: node.embeddedBehavior
    };
    if (existing < 0) rows.push(row); else rows[existing] = row;
  }
  return rows;
}

export function transitionParts(edge: Parameters<typeof transitionText>[0]): LabelParts {
  const trigger = edge.triggers.filter(name => !['', 'Completion', 'Stateflows.Common.Completion', 'Stateflows.StateMachines.Completion'].includes(name)).join(', ');
  const guards = [...(edge.guardNames ?? [])];
  while (guards.length < edge.guards) guards.push('guard');
  if (edge.isElse) guards.unshift('else');
  const effects = [...(edge.effectNames ?? [])];
  while (effects.length < edge.effects) effects.push('effect');
  return { trigger, guard: guards.length ? guards.join(' && ') : undefined,
    effect: effects.length ? effects.join('; ') : undefined };
}

export function transitionText(edge: {
  triggers: readonly string[]; isElse: boolean; guards: number; effects: number;
  guardNames?: readonly string[]; effectNames?: readonly string[];
}): string {
  const { trigger, guard, effect } = transitionParts(edge);
  return `${trigger}${guard ? ` [${guard}]` : ''}${effect ? ` / ${effect}` : ''}`.trim();
}

/** Presentation-only UML expansion; registered identities and internal transitions remain intact. */
export function stateMachineView(document: DiagramDocument): DiagramDocument {
  if (document.family !== 'stateMachine') return document;
  const regions = new Map<string, DiagramNode[]>();
  for (const node of document.nodes)
    if (node.kind === 'Region' && node.parentId)
      regions.set(node.parentId, [...(regions.get(node.parentId) ?? []), node]);
  const flattened = new Map([...regions].filter(([, items]) => items.length === 1).map(([parent, items]) => [items[0].id, parent]));
  const nodes: DiagramNode[] = document.nodes.filter(node => !flattened.has(node.id)).map(node => ({
    ...node, initial: undefined, notation: 'stateMachine',
    parentId: node.parentId ? flattened.get(node.parentId) ?? node.parentId : undefined,
    details: [
      ...operationRows(node).map(operation => operation.label),
      ...document.edges.filter(edge => edge.source === node.id && edge.target === undefined).map(edge => edge.label)
    ],
    operations: [
      ...operationRows(node),
      ...document.edges.filter(edge => edge.source === node.id && edge.target === undefined).map(edge => ({
        id: operationId(edge.id, 'transition.effect'), selectionId: operationId(edge.id, 'transition.effect'),
        label: edge.label, target: { elementId: edge.id, operation: 'transition.effect' }, parts: edge.parts
      }))
    ]
  }));
  const edges: DiagramEdge[] = [...document.edges];
  const ids = new Set([...nodes, ...edges].map(element => element.id));
  for (const initial of document.nodes.filter(node => node.initial)) {
    let id = JSON.stringify(['uml-initial', initial.id]);
    while (ids.has(id) || ids.has(`${id}:edge`)) id += ':';
    ids.add(id); ids.add(`${id}:edge`);
    const target = nodes.find(node => node.id === initial.id)!;
    nodes.push({ id, label: '', kind: 'Initial', notation: 'stateMachine', parentId: target.parentId, selectable: false });
    edges.push({ id: `${id}:edge`, source: id, target: target.id, label: '', kind: 'transition', selectable: false });
  }
  return { ...document, nodes, edges };
}

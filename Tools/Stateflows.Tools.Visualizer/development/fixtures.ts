import type { DiagramDocument, DiagramEdge, DiagramNode } from '../src/lib/model.js';
import { stateMachineView } from '../src/lib/stateMachine.js';

export interface SyntheticFixture {
  document: DiagramDocument;
  description: string;
}

type Family = DiagramDocument['family'];
type Size = 'small' | 'medium' | 'large';
const moduleCounts: Record<Size, number> = { small: 1, medium: 8, large: 32 };

function machineModule(prefix: string, nodes: DiagramNode[], edges: DiagramEdge[]) {
  const node = (id: string, kind: string, parent?: string, initial = false) =>
    nodes.push({ id: `${prefix}/${id}`, label: id, kind, parentId: parent ? `${prefix}/${parent}` : undefined, initial });
  const edge = (id: string, source: string, target?: string, extra: Partial<DiagramEdge> = {}) =>
    edges.push({ id: `${prefix}/edge/${id}`, source: `${prefix}/${source}`, target: target ? `${prefix}/${target}` : undefined,
      label: id, kind: 'transition', ...extra });

  node('Order', 'CompositeState');
  node('Review', 'State', 'Order', true);
  node('Work', 'OrthogonalState', 'Order');
  node('Payment region', 'Region', 'Work');
  node('Stock region', 'Region', 'Work');
  node('Pending payment', 'State', 'Payment region', true);
  node('Paid', 'State', 'Payment region');
  node('Reservation', 'CompositeState', 'Stock region', true);
  node('Checking', 'State', 'Reservation', true);
  node('Reserved', 'State', 'Reservation');
  node('History', 'History', 'Stock region');
  node('Choice', 'Choice', 'Order');
  node('Junction', 'Junction', 'Order');
  node('Done', 'FinalState', 'Order');
  node('Cancelled', 'State');
  edge('Approve [allowed] / audit', 'Review', 'Work');
  edge('Approve [else]', 'Review', 'Cancelled', { isElse: true });
  edge('Approve [manual review]', 'Review', 'Work');
  edge('Payment received', 'Pending payment', 'Paid');
  edge('Stock found', 'Checking', 'Reserved');
  edge('Retry', 'Checking', 'Checking');
  edge('Refresh / update values', 'Checking');
  edge('Cancel from nested state', 'Checking', 'Cancelled', { isLocal: false });
  edge('Reset to ancestor (external)', 'Reserved', 'Order', { isLocal: false });
  edge('Resume descendant (local)', 'Order', 'Checking', { isLocal: true });
  edge('Finish', 'Work', 'Choice');
  edge('[accepted]', 'Choice', 'Junction');
  edge('[else]', 'Choice', 'Review', { isElse: true });
  edge('Complete', 'Junction', 'Done');
  edge('Restore', 'Review', 'History');
}

function activityModule(prefix: string, nodes: DiagramNode[], edges: DiagramEdge[]) {
  const node = (id: string, kind: string, parent?: string, details?: string[]) =>
    nodes.push({ id: `${prefix}/${id}`, label: id, kind, parentId: parent ? `${prefix}/${parent}` : undefined, details });
  const edge = (id: string, source: string, target: string, kind: DiagramEdge['kind'] = 'control', extra: Partial<DiagramEdge> = {}) =>
    edges.push({ id: `${prefix}/edge/${source}/${target}/${id}`, source: `${prefix}/${source}`, target: `${prefix}/${target}`, label: id, kind, ...extra });

  node('Start', 'Initial');
  node('Input', 'InputPin', undefined, ['Order']);
  node('Validate', 'Action', undefined, ['Validate order and emit accepted items']);
  node('Choose', 'Decision');
  node('Process', 'StructuredActivity');
  node('Input pin', 'InputPin', 'Process', ['Item']);
  node('Fork', 'Fork', 'Process');
  node('Parallel', 'ParallelActivity', 'Process', ['chunkSize=4', 'maxConcurrency=3']);
  node('Worker input', 'InputPin', 'Parallel', ['Item']);
  node('Transform', 'Action', 'Parallel');
  node('Worker output', 'OutputPin', 'Parallel', ['Result']);
  node('Iterate', 'IterativeActivity', 'Process');
  node('Iteration input', 'InputPin', 'Iterate', ['Result']);
  node('Enrich', 'Action', 'Iterate');
  node('Iteration output', 'OutputPin', 'Iterate', ['Result']);
  node('Store', 'DataStore', 'Process', ['Retained Result tokens']);
  node('Join', 'Join', 'Process');
  node('Output pin', 'OutputPin', 'Process', ['Result']);
  node('Rejected', 'Action');
  node('Output', 'OutputPin', undefined, ['Summary']);
  node('End', 'Final');
  edge('Start processing', 'Start', 'Validate');
  edge('Order', 'Input', 'Validate', 'object');
  edge('Item', 'Validate', 'Choose', 'object');
  edge('[valid] Item', 'Choose', 'Input pin', 'object');
  edge('[else] Item', 'Choose', 'Rejected', 'object', { isElse: true });
  edge('Activate scope', 'Validate', 'Process');
  edge('Fork', 'Input pin', 'Fork', 'object');
  edge('Dispatch', 'Fork', 'Parallel');
  edge('Inspect', 'Fork', 'Store', 'object', { weight: 0 });
  edge('Partition', 'Parallel', 'Worker input', 'object');
  edge('Item', 'Worker input', 'Transform', 'object');
  edge('Item -> Result', 'Transform', 'Worker output', 'transformation');
  edge('Batch Result (weight 4)', 'Worker output', 'Iteration input', 'object', { weight: 4 });
  edge('Iterate', 'Parallel', 'Iterate');
  edge('Result', 'Iteration input', 'Enrich', 'object');
  edge('Enriched Result', 'Enrich', 'Iteration output', 'object');
  edge('Retain Result', 'Iteration output', 'Store', 'object');
  edge('Result (weight 2)', 'Store', 'Join', 'object', { weight: 2 });
  edge('Joined Result', 'Join', 'Output pin', 'object');
  edge('Result -> Summary', 'Output pin', 'Output', 'transformation');
  edge('Rejected summary', 'Rejected', 'Output', 'object');
  edge('Complete', 'Process', 'End');
}

export function syntheticModel(family: Family, size: Size): SyntheticFixture {
  const nodes: DiagramNode[] = [];
  const edges: DiagramEdge[] = [];
  for (let index = 0; index < moduleCounts[size]; index++) {
    const prefix = `module-${index}`;
    if (family === 'stateMachine') machineModule(prefix, nodes, edges);
    else activityModule(prefix, nodes, edges);
    if (index) edges.push({
      id: `link-${index}`, source: `module-${index - 1}/${family === 'stateMachine' ? 'Done' : 'Output pin'}`,
      target: `${prefix}/${family === 'stateMachine' ? 'Review' : 'Input pin'}`,
      label: 'Cross module', kind: family === 'stateMachine' ? 'transition' : 'object'
    });
  }
  return {
    document: stateMachineView({ id: `synthetic:${family}:${size}`, name: `Synthetic ${family} / ${size}`,
      family, version: 1, nodes, edges }),
    description: family === 'stateMachine'
      ? 'Orthogonal regions, five-level nesting, ancestor/descendant transitions, internal transitions, self-loops, parallel edges, history and choices.'
      : 'Structured, parallel and iterative scopes, typed pins, transformations, weighted/else flows, fork/join and cross-scope connections.'
  };
}

export function deepModel(family: Family): SyntheticFixture {
  const nodes: DiagramNode[] = [];
  const edges: DiagramEdge[] = [];
  for (let depth = 0; depth < 12; depth++) {
    nodes.push({ id: `scope-${depth}`, label: `Depth ${depth + 1}`,
      kind: family === 'stateMachine' ? 'CompositeState' : 'StructuredActivity',
      parentId: depth ? `scope-${depth - 1}` : undefined });
    for (let branch = 0; branch < 2; branch++) {
      nodes.push({ id: `leaf-${depth}-${branch}`, label: `Repeated label ${branch}`,
        kind: family === 'stateMachine' ? 'State' : 'Action', parentId: `scope-${depth}` });
    }
    edges.push({ id: `step-${depth}`, source: `leaf-${depth}-0`, target: `leaf-${depth}-1`,
      label: 'An intentionally long connection label to exercise routing and label clearance',
      kind: family === 'stateMachine' ? 'transition' : 'object' });
    if (depth) edges.push({ id: `cross-${depth}`, source: `leaf-${depth - 1}-1`, target: `leaf-${depth}-0`,
      label: 'Enter deeper scope', kind: family === 'stateMachine' ? 'transition' : 'object' });
  }
  edges.push({ id: 'escape', source: 'leaf-11-1', target: 'leaf-0-0', label: 'Cross eleven scope boundaries',
    kind: family === 'stateMachine' ? 'transition' : 'object' });
  return {
    document: stateMachineView({ id: `synthetic:${family}:deep`, name: `Synthetic ${family} / deep`, family, version: 1, nodes, edges }),
    description: 'Twelve nested scopes, repeated display labels, long edge labels and connections spanning eleven boundaries.'
  };
}

export const syntheticFixtures: readonly SyntheticFixture[] = (['stateMachine', 'activity'] as const).flatMap(family => [
  ...(['small', 'medium', 'large'] as const).map(size => syntheticModel(family, size)),
  deepModel(family)
]);

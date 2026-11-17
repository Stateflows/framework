import { describe, expect, test } from 'vitest';
import { fromAction, fromStateMachine, type StateMachineDefinition } from '../src/lib/adapters.js';
import { dimensions, layoutDiagram } from '../src/lib/layout.js';
import { stateMachineView, transitionText, searchElements, triggerNames } from '../src/lib/stateMachine.js';
import type { DiagramDocument } from '../src/lib/model.js';
import { routedPath } from '../src/lib/edgePath.js';

describe('UML state machine notation', () => {
  test('normalizes exact trigger names and focuses all search matches independently of selection', () => {
    expect(triggerNames({ id: 'edge', source: 'a', label: '', kind: 'transition',
      triggers: ['Approve', 'Completion', 'Stateflows.Common.Completion'] })).toEqual(['Approve', 'default']);
    const document = stateMachineView({
      id: 'focus', name: 'Focus', family: 'stateMachine', version: 1,
      nodes: [{ id: 'a', label: 'Ready A', kind: 'State', initial: true },
        { id: 'b', label: 'Ready B', kind: 'State' }],
      edges: [{ id: 'ab', source: 'a', target: 'b', label: 'Ready', kind: 'transition' },
        { id: 'internal', source: 'a', label: 'Ready / Audit', kind: 'transition' }]
    });
    expect(searchElements(document, ' ready ')).toHaveLength(4);
    expect(searchElements(document, ' ready ')).toEqual(expect.arrayContaining(['a', 'b', 'ab']));
    expect(searchElements(document, ' ')).toEqual([]);
    expect(searchElements(document, 'missing')).toEqual([]);
  });
  test.each(['RIGHT', 'DOWN'] as const)('preserves definition order within a layer: %s', async direction => {
    const layout = await layoutDiagram(stateMachineView({
      id: 'ordering', name: 'Ordering', family: 'stateMachine', version: 1,
      nodes: [{ id: 'start', label: 'Start', kind: 'State' },
        ...['third', 'first', 'second'].map(id => ({ id, label: id, kind: 'State' }))],
      edges: ['second', 'first', 'third'].map(id => ({
        id: `to-${id}`, source: 'start', target: id, label: '', kind: 'transition' as const
      }))
    }), { direction, stateOrdering: 'PRESERVE_DEFINITION' });
    const axis = direction === 'RIGHT' ? 'y' : 'x';
    const ordered = layout.nodes.filter(node => node.id !== 'start')
      .sort((a, b) => a.position[axis] - b.position[axis]).map(node => node.id);
    expect(ordered).toEqual(['third', 'first', 'second']);
  });
  test.each(['ORTHOGONAL', 'POLYLINE', 'SPLINES'] as const)('layouts render valid %s routes', async routing => {
    const document = stateMachineView({
      id: 'routes', name: 'Routes', family: 'stateMachine', version: 1,
      nodes: [{ id: 'a', label: 'A', kind: 'State', initial: true },
        { id: 'b', label: 'B', kind: 'State' }, { id: 'c', label: 'C', kind: 'State' }],
      edges: [{ id: 'ab', source: 'a', target: 'b', label: 'Go', kind: 'transition' },
        { id: 'ac', source: 'a', target: 'c', label: 'Skip', kind: 'transition' },
        { id: 'bc', source: 'b', target: 'c', label: 'Go', kind: 'transition' }]
    });
    const layout = await layoutDiagram(document, { routing });
    for (const edge of layout.edges) for (const points of edge.paths) {
      const path = routedPath(points, edge.routing);
      expect(path).not.toMatch(/NaN|undefined/);
      expect(path).toContain(`M ${points[0].x} ${points[0].y}`);
    }
  });
  test('shows an action as an opaque implementation rather than an invented graph', async () => {
    const document = fromAction({ name: 'Machine.Done.do', version: 1, implementation: 'CompletionAudit' });
    expect(document.family).toBe('action');
    expect(document.nodes).toEqual([{ id: 'Machine.Done.do', label: 'CompletionAudit', kind: 'Action',
      details: ['C# action implementation'] }]);
    expect(document.edges).toEqual([]);
    const layout = await layoutDiagram(document);
    expect(layout.nodes).toHaveLength(1);
    expect(layout.edges).toEqual([]);
  });

  test.each(['RIGHT', 'DOWN'] as const)('junction routes meet the circle rather than an invisible margin: %s', async direction => {
    const layout = await layoutDiagram(stateMachineView({
      id: 'junction', name: 'Junction', family: 'stateMachine', version: 1,
      nodes: [{ id: 'before', label: 'Before', kind: 'State' },
        { id: 'junction', label: 'Junction', kind: 'Junction' },
        { id: 'after', label: 'After', kind: 'State' }],
      edges: [{ id: 'in', source: 'before', target: 'junction', label: '', kind: 'transition' },
        { id: 'out', source: 'junction', target: 'after', label: '', kind: 'transition' }]
    }), { direction });
    const junction = layout.nodes.find(node => node.kind === 'Junction')!;
    expect(junction.width).toBe(30);
    expect(junction.height).toBe(30);
    for (const point of [layout.edges[0].paths.at(-1)!.at(-1)!, layout.edges[1].paths[0][0]])
      expect(Math.hypot(point.x - junction.position.x - 15, point.y - junction.position.y - 15)).toBeCloseTo(15);
  });

  test.each(['RIGHT', 'DOWN'] as const)('initial arrows meet the visible circle: %s', async direction => {
    const document = stateMachineView({
      id: 'initial', name: 'Initial', family: 'stateMachine', version: 1,
      nodes: [{ id: 'state', label: 'State', kind: 'State', initial: true }], edges: []
    });
    const layout = await layoutDiagram(document, { direction });
    const initial = layout.nodes.find(node => node.kind === 'Initial')!;
    expect(initial.width).toBe(30);
    expect(initial.height).toBe(30);
    const start = layout.edges[0].paths[0][0];
    expect(Math.hypot(start.x - initial.position.x - initial.width / 2,
      start.y - initial.position.y - initial.height / 2)).toBeCloseTo(15);
  });

  test('uses trigger [guard] / effect and preserves Boolean expression grouping', () => {
    expect(transitionText({ triggers: ['Approve'], guards: 2, effects: 2, isElse: false,
      guardNames: ['Allowed', '(Ready || !Blocked)'], effectNames: ['Audit', 'Notify'] }))
      .toBe('Approve [Allowed && (Ready || !Blocked)] / Audit; Notify');
    expect(transitionText({ triggers: ['Completion'], guards: 0, effects: 0, isElse: false })).toBe('');
    expect(transitionText({ triggers: ['Completion'], guards: 0, effects: 1, isElse: true, effectNames: ['Audit'] }))
      .toBe('[else] / Audit');
    expect(transitionText({ triggers: ['Refresh'], guards: 1, effects: 1, isElse: false }))
      .toBe('Refresh [guard] / effect');
  });

  test('flattens a sole region and creates initial pseudostates at every nesting level', () => {
    const definition: StateMachineDefinition = {
      name: 'UML', version: 1, initialStateId: 'Outer',
      states: [
        { id: 'Outer', name: 'Outer', kind: 'CompositeState', regionId: null,
          initializeActions: 2, finalizeActions: 1, entryActions: 2, exitActions: 1,
          initializeNames: ['OuterState', 'InitializeWork'], finalizeNames: ['OuterState'],
          stateType: 'OuterState', entryNames: ['OuterState', 'Audit'], exitNames: ['Cleanup'],
          deferredEvents: ['Pause', 'Finish', 'Cancel'], behaviorType: 'Activity', behaviorName: 'UML.Outer.do', behaviorImplementation: 'Work',
          deferrals: [{ trigger: 'Pause', guards: [] }, { trigger: 'Finish', guards: ['Deny'] },
            { trigger: 'Cancel', guards: ['Allowed', '(!Blocked || guard)'] }] },
        { id: 'Inner', name: 'Inner', kind: 'State', regionId: 'region',
          initializeActions: 0, finalizeActions: 0, entryActions: 0, exitActions: 0,
          deferredEvents: [], behaviorType: null, behaviorName: null }
      ],
      regions: [{ id: 'region', stateId: 'Outer', index: 0, initialStateId: 'Inner', historyStateId: null }],
      transitions: [{ id: 'internal', sourceId: 'Outer', targetId: null,
        triggers: ['Refresh'], isElse: false, isLocal: true, guards: 0, effects: 1, effectNames: ['Update'] }]
    };
    const document = fromStateMachine(definition);
    expect(document.nodes.filter(node => node.entryBreakpoint).map(node => node.id)).toEqual(['Outer', 'Inner']);
    expect(document.nodes.filter(node => node.kind === 'Initial').every(node => !node.entryBreakpoint)).toBe(true);
    expect(document.nodes.some(node => node.kind === 'Region')).toBe(false);
    expect(document.nodes.find(node => node.id === 'Inner')?.parentId).toBe('Outer');
    expect(document.nodes.find(node => node.id === 'Outer')?.details)
      .toEqual(['initialization / onInitialization()', 'initialization / InitializeWork', 'entry / onEntry()',
        'entry / Audit', 'exit / Cleanup', 'finalization / onFinalization()', 'Pause / defer', 'Finish [Deny] / defer',
        'Cancel [Allowed && (!Blocked || guard)] / defer', 'do / Work', 'Refresh / Update']);
    expect(document.nodes.find(node => node.id === 'Outer')?.embeddedBehavior)
      .toEqual({ type: 'Activity', name: 'UML.Outer.do' });
    const operations = document.nodes.find(node => node.id === 'Outer')!.operations!;
    const entries = operations.filter(operation => operation.target.operation === 'state.entry');
    expect(entries).toHaveLength(2);
    expect(new Set(entries.map(operation => operation.selectionId)).size).toBe(1);
    expect(new Set(entries.map(operation => operation.id)).size).toBe(2);
    expect(operations.find(operation => operation.target.operation === 'state.do')?.embeddedBehavior)
      .toEqual({ type: 'Activity', name: 'UML.Outer.do' });
    expect(operations.at(-1)?.target).toEqual({ elementId: 'internal', operation: 'transition.effect' });
    const initials = document.nodes.filter(node => node.kind === 'Initial');
    expect(initials).toHaveLength(2);
    expect(initials.map(node => node.parentId)).toEqual([undefined, 'Outer']);
    expect(initials.every(node => node.label === '')).toBe(true);
    expect(initials.every(node => node.selectable === false)).toBe(true);
    expect(document.edges.filter(edge => edge.label === '').every(edge => edge.selectable === false)).toBe(true);
    expect(document.nodes.every(node => !node.initial)).toBe(true);
    expect(document.edges.filter(edge => edge.label === '').map(edge => edge.target)).toEqual(['Outer', 'Inner']);
    expect(document.edges.find(edge => edge.id === 'internal')?.target).toBeUndefined();
  });

  test('operation-only documents preserve rows and legacy detail rows do not invent breakpoints', async () => {
    const layout = await layoutDiagram({
      id: 'operations', name: 'Operations', family: 'stateMachine', version: 1,
      nodes: [{ id: 'state', label: 'State', kind: 'State', notation: 'stateMachine',
        operations: [{ id: 'entry-0', selectionId: 'entry', label: 'entry / Work', target: { elementId: 'state', operation: 'state.entry' } }] },
        { id: 'legacy', label: 'Legacy', kind: 'State', notation: 'stateMachine', details: ['entry / Legacy'] }],
      edges: []
    });
    expect(layout.nodes.find(node => node.id === 'state')?.details).toEqual(['entry / Work']);
    expect(layout.nodes.find(node => node.id === 'legacy')?.operations?.[0].breakpointable).toBe(false);
  });

  test.each(['Action', 'Activity', 'StateMachine'])('retains embedded %s identity without a bottom icon gutter', async type => {
    const embeddedBehavior = { type, name: `Machine.Work.${type}`, version: 2 };
    const document = stateMachineView({
      id: 'embedded', name: 'Embedded', family: 'stateMachine', version: 1,
      nodes: [{ id: 'state', label: 'State', kind: 'State', details: ['do / Work'], embeddedBehavior }], edges: []
    });
    for (const presentation of [{}, { collapsedIds: ['state'] }, { collapsedOperationIds: ['state'] }]) {
      const layout = await layoutDiagram(document, presentation);
      expect(layout.nodes[0].embeddedBehavior).toEqual(embeddedBehavior);
      expect(layout.nodes[0].height).toBe('collapsedIds' in presentation ? 42 : 80);
    }
  });

  test('orthogonal states no longer reserve a bottom icon gutter', async () => {
    const document = stateMachineView({
      id: 'embedded-parallel', name: 'Embedded parallel', family: 'stateMachine', version: 1,
      nodes: [
        { id: 'parent', label: 'Parent', kind: 'OrthogonalState', embeddedBehavior: { type: 'Activity', name: 'Work' } },
        ...['left', 'right'].flatMap(id => [
          { id, label: id, kind: 'Region', parentId: 'parent' },
          { id: `${id}-state`, label: 'State', kind: 'State', parentId: id }
        ])
      ], edges: []
    });
    for (const collapsedIds of [[], ['left'], ['left', 'right']]) {
      const layout = await layoutDiagram(document, { collapsedIds });
      const parent = layout.nodes.find(node => node.id === 'parent')!;
      for (const region of layout.nodes.filter(node => node.kind === 'Region')) {
        expect(region.position.y + region.height).toBeLessThanOrEqual(parent.height - 1.5);
        expect(region.height).toBeGreaterThanOrEqual(38);
        if (collapsedIds.includes(region.id)) expect(region.height).toBe(38);
      }
    }
  });

  test('retains multiple orthogonal regions and sizes all operation lines above children', async () => {
    const document = stateMachineView({
      id: 'orthogonal', name: 'Orthogonal', version: 1, family: 'stateMachine',
      nodes: [
        { id: 'state', label: 'Parallel', kind: 'OrthogonalState', details: Array.from({ length: 8 }, (_, i) => `entry / Action${i}`) },
        ...['left', 'right'].flatMap(id => [
          { id, label: id, kind: 'Region', parentId: 'state' },
          { id: `${id}-state`, label: 'Pending', kind: 'State', parentId: id, initial: true }
        ])
      ], edges: []
    });
    expect(document.nodes.filter(node => node.kind === 'Region')).toHaveLength(2);
    const layout = await layoutDiagram(document);
    const state = document.nodes.find(node => node.id === 'state')!;
    expect(dimensions(state).height).toBeGreaterThan(8 * 18 + 38);
    for (const region of layout.nodes.filter(node => node.kind === 'Region'))
      expect(region.position.y).toBeGreaterThanOrEqual(dimensions(state).height);
    expect(layout.edges).toHaveLength(2);
    const parent = layout.nodes.find(node => node.id === 'state')!;
    const regions = layout.nodes.filter(node => node.kind === 'Region').sort((a, b) => a.position.y - b.position.y);
    for (const region of regions) {
      expect(region.position.x).toBe(1.5);
      expect(region.width).toBe(parent.width - 3);
    }
    expect(regions[0].regionSeparator).toBe(false);
    expect(regions[1].regionSeparator).toBe(true);
    expect(regions[0].position.y + regions[0].height).toBe(regions[1].position.y);
    expect(regions[1].position.y + regions[1].height).toBe(parent.height - 1.5);

    const collapsed = await layoutDiagram(document, { collapsedIds: ['left'] });
    expect(collapsed.nodes.some(node => node.id === 'left-state')).toBe(false);
    expect(collapsed.nodes.some(node => node.id === 'right-state')).toBe(true);
    expect(collapsed.nodes.find(node => node.id === 'left')?.hiddenCount).toBe(2);
    expect(collapsed.nodes.find(node => node.id === 'left')!.height).toBeLessThan(regions[0].height);
    expect((await layoutDiagram(document)).nodes).toEqual(layout.nodes);
    const collapsedWork = await layoutDiagram(document, { collapsedOperationIds: ['state'] });
    const workState = collapsedWork.nodes.find(node => node.id === 'state')!;
    expect(workState.operationsCollapsed).toBe(true);
    expect(workState.details).toEqual(state.details);
    expect(workState.height).toBeLessThan(parent.height);
    expect(collapsedWork.nodes.some(node => node.id === 'left-state')).toBe(true);
    expect(collapsedWork.nodes.filter(node => node.kind === 'Region')[0].position.y).toBe(80);
    const collapsedSubstates = await layoutDiagram(document, { collapsedSubstateIds: ['state'] });
    expect(collapsedSubstates.nodes).toEqual(layout.nodes);
    expect(collapsedSubstates.nodes[0].hasSubstatesCompartment).toBe(false);
    for (const collapsedIds of [['left'], ['right'], ['left', 'right']]) {
      const compact = await layoutDiagram(document, { collapsedIds });
      for (const id of collapsedIds)
        expect(compact.nodes.find(node => node.id === id)!.height).toBeCloseTo(38);
    }
    const collapsedState = await layoutDiagram(document, { collapsedIds: ['state'] });
    expect(collapsedState.nodes.map(node => node.id)).toEqual(['state']);
    expect(collapsedState.nodes[0].height).toBe(42);
  });

  test.each([
    { direction: 'RIGHT', nested: false }, { direction: 'DOWN', nested: false },
    { direction: 'RIGHT', nested: true }, { direction: 'DOWN', nested: true }
  ] as const)('distinguishes local and external ancestor transitions: $direction, nested=$nested', async ({ direction, nested }) => {
    for (const isLocal of [true, false]) {
      for (const fromParent of [true, false]) {
        const document = stateMachineView({
          id: 'ancestor', name: 'Ancestor', family: 'stateMachine', version: 1,
          nodes: [
            ...(nested ? [{ id: 'outer', label: 'Outer', kind: 'CompositeState' }] : []),
            { id: 'parent', label: 'Parent', kind: 'CompositeState', parentId: nested ? 'outer' : undefined },
            { id: 'child', label: 'Child', kind: 'State', parentId: 'parent' }
          ],
          edges: [{ id: 'edge', source: fromParent ? 'parent' : 'child', target: fromParent ? 'child' : 'parent',
            label: 'Trigger', kind: 'transition', isLocal }]
        });
        const layout = await layoutDiagram(document, { direction });
        const parent = layout.nodes.find(node => node.id === 'parent')!;
        const outer = layout.nodes.find(node => node.id === 'outer');
        const origin = { x: parent.position.x + (outer?.position.x ?? 0),
          y: parent.position.y + (outer?.position.y ?? 0) };
        const points = layout.edges[0].paths.flat();
        const outside = points.some(point => point.x < origin.x - 0.1 ||
          point.x > origin.x + parent.width + 0.1 || point.y < origin.y - 0.1 ||
          point.y > origin.y + parent.height + 0.1);
        expect(outside).toBe(!isLocal);
        const child = layout.nodes.find(node => node.id === 'child')!;
        const bounds = {
          parent: { ...origin, width: parent.width, height: parent.height },
          child: { x: origin.x + child.position.x, y: origin.y + child.position.y,
            width: child.width, height: child.height }
        };
        for (const [point, box] of [
          [points[0], fromParent ? bounds.parent : bounds.child],
          [points.at(-1)!, fromParent ? bounds.child : bounds.parent]
        ] as const) {
          expect(point.x).toBeGreaterThanOrEqual(box.x - 0.1);
          expect(point.y).toBeGreaterThanOrEqual(box.y - 0.1);
          expect(point.x).toBeLessThanOrEqual(box.x + box.width + 0.1);
          expect(point.y).toBeLessThanOrEqual(box.y + box.height + 0.1);
          expect(Math.min(Math.abs(point.x - box.x), Math.abs(point.x - box.x - box.width),
            Math.abs(point.y - box.y), Math.abs(point.y - box.y - box.height))).toBeLessThan(0.1);
        }
        for (let index = 1; index < points.length; index++)
          expect(points[index].x === points[index - 1].x || points[index].y === points[index - 1].y).toBe(true);
        expect(layout.edges[0].id).toBe('edge');
        expect(layout.edges[0].labelPosition).toBeDefined();
        expect(layout.nodes.map(node => node.id)).toEqual(nested ? ['outer', 'parent', 'child'] : ['parent', 'child']);
        if (outer) {
          for (const point of points) {
            expect(point.x).toBeGreaterThanOrEqual(outer.position.x);
            expect(point.y).toBeGreaterThanOrEqual(outer.position.y);
            expect(point.x).toBeLessThanOrEqual(outer.position.x + outer.width);
            expect(point.y).toBeLessThanOrEqual(outer.position.y + outer.height);
          }
        }
        const collapsed = await layoutDiagram(document, { direction, collapsedIds: ['parent'] });
        expect(collapsed.nodes.map(node => node.id)).toEqual(nested ? ['outer', 'parent'] : ['parent']);
        expect(collapsed.edges).toEqual([]);
      }
    }
  });

  test('single substate compartments and event-only states collapse without changing their definitions', async () => {
    const document = stateMachineView({
      id: 'compartments', name: 'Compartments', family: 'stateMachine', version: 1,
      nodes: [
        { id: 'parent', label: 'Parent', kind: 'CompositeState', details: ['entry / ParentAction'] },
        { id: 'child', label: 'Child', kind: 'State', parentId: 'parent', details: ['entry / ChildAction'] }
      ], edges: []
    });
    const layout = await layoutDiagram(document, { collapsedSubstateIds: ['parent'] });
    expect(layout.nodes.map(node => node.id)).toEqual(['parent']);
    expect(layout.nodes[0].substatesCollapsed).toBe(true);
    expect(layout.nodes[0].hasSubstatesCompartment).toBe(true);
    expect(layout.nodes[0].details).toEqual(['entry / ParentAction']);
    const collapsed = await layoutDiagram(document, { collapsedIds: ['child'] });
    expect(collapsed.nodes.find(node => node.id === 'child')!.height).toBe(42);
    expect(collapsed.nodes.find(node => node.id === 'child')!.details).toEqual(['entry / ChildAction']);
  });

  test.each([true, false])('ancestor transition locality survives a hidden descendant: local=%s', async isLocal => {
    const document = stateMachineView({
      id: 'hidden', name: 'Hidden descendant', family: 'stateMachine', version: 1,
      nodes: [
        { id: 'parent', label: 'Parent', kind: 'CompositeState' },
        { id: 'branch', label: 'Branch', kind: 'CompositeState', parentId: 'parent' },
        { id: 'leaf', label: 'Leaf', kind: 'State', parentId: 'branch' }
      ],
      edges: [{ id: 'edge', source: 'parent', target: 'leaf', kind: 'transition', label: 'Trigger', isLocal }]
    });
    const layout = await layoutDiagram(document, { collapsedIds: ['branch'] });
    const parent = layout.nodes.find(node => node.id === 'parent')!;
    const edge = layout.edges[0];
    expect(edge.source).toBe('parent');
    expect(edge.target).toBe('leaf');
    expect(edge.visibleTarget).toBe('branch');
    expect(edge.hiddenEndpoints).toBe(true);
    expect(edge.paths.flat().some(point => point.x < parent.position.x ||
      point.x > parent.position.x + parent.width || point.y < parent.position.y ||
      point.y > parent.position.y + parent.height)).toBe(!isLocal);
  });

  test.each(['RIGHT', 'DOWN'] as const)('fork and join bars are perpendicular to flow: %s', async direction => {
    const document: DiagramDocument = {
      id: 'bars', name: 'Bars', family: 'stateMachine', version: 1,
      nodes: ['Fork', 'Join'].map(kind => ({ id: kind, label: kind, kind })),
      edges: [{ id: 'flow', source: 'Fork', target: 'Join', kind: 'transition', label: '' }]
    };
    const layout = await layoutDiagram(document, { direction });
    for (const node of layout.nodes) {
      expect(node.width).toBe(direction === 'RIGHT' ? 18 : 100);
      expect(node.height).toBe(direction === 'RIGHT' ? 100 : 18);
    }
    const path = layout.edges[0].paths[0];
    if (direction === 'RIGHT') expect(path.at(-1)!.x).toBeGreaterThan(path[0].x);
    else expect(path.at(-1)!.y).toBeGreaterThan(path[0].y);
  });

  test('does not alter activities or collide with registered identities', () => {
    const document: DiagramDocument = {
      id: 'ids', name: 'IDs', version: 1, family: 'stateMachine',
      nodes: [{ id: 'first', label: 'First', kind: 'State', initial: true },
        { id: '["uml-initial","first"]', label: 'Existing', kind: 'State' }], edges: []
    };
    const view = stateMachineView(document);
    expect(new Set([...view.nodes, ...view.edges].map(item => item.id)).size).toBe(view.nodes.length + view.edges.length);
    const activity = { ...document, family: 'activity' as const };
    expect(stateMachineView(activity)).toBe(activity);
  });

  test.each(['RIGHT', 'DOWN'] as const)('compartments preserve cross-region routing: %s', async direction => {
    const document = stateMachineView({
      id: 'cross-regions', name: 'Cross regions', version: 1, family: 'stateMachine',
      nodes: [
        { id: 'parallel', label: 'Parallel', kind: 'OrthogonalState' },
        { id: 'left', label: 'Left', kind: 'Region', parentId: 'parallel' },
        { id: 'right', label: 'Right', kind: 'Region', parentId: 'parallel' },
        { id: 'left-state', label: 'Left state', kind: 'State', parentId: 'left' },
        { id: 'right-state', label: 'Right state', kind: 'State', parentId: 'right' },
        { id: 'outside', label: 'Outside', kind: 'State' }
      ],
      edges: [
        { id: 'cross', source: 'left-state', target: 'right-state', label: 'Cross', kind: 'transition' },
        { id: 'enter', source: 'outside', target: 'left-state', label: 'Enter', kind: 'transition' },
        { id: 'exit', source: 'right-state', target: 'outside', label: 'Exit', kind: 'transition' }
      ]
    });
    for (const collapsedIds of [[], ['left'], ['left', 'right']]) {
      const layout = await layoutDiagram(document, { direction, collapsedIds });
      const byId = new Map(layout.nodes.map(node => [node.id, node]));
      for (const node of layout.nodes) {
        if (!node.parentId) continue;
        const parent = byId.get(node.parentId)!;
        expect(node.position.x).toBeGreaterThanOrEqual(0);
        expect(node.position.y).toBeGreaterThanOrEqual(0);
        expect(node.position.x + node.width).toBeLessThanOrEqual(parent.width + 0.1);
        expect(node.position.y + node.height).toBeLessThanOrEqual(parent.height + 0.1);
      }
      expect(layout.edges.map(edge => edge.id).sort()).toEqual(['cross', 'enter', 'exit']);
      function absoluteBounds(id: string) {
        const node = byId.get(id)!;
        const bounds = { ...node.position, width: node.width, height: node.height };
        let parentId = node.parentId;
        while (parentId) {
          const parent = byId.get(parentId)!;
          bounds.x += parent.position.x; bounds.y += parent.position.y;
          parentId = parent.parentId;
        }
        return bounds;
      }
      for (const edge of layout.edges) {
        for (const [id, point] of [
          [edge.visibleSource, edge.paths[0][0]],
          [edge.visibleTarget, edge.paths.at(-1)!.at(-1)!]
        ] as const) {
          const bounds = absoluteBounds(id);
          expect(point.x).toBeGreaterThanOrEqual(bounds.x - 0.1);
          expect(point.y).toBeGreaterThanOrEqual(bounds.y - 0.1);
          expect(point.x).toBeLessThanOrEqual(bounds.x + bounds.width + 0.1);
          expect(point.y).toBeLessThanOrEqual(bounds.y + bounds.height + 0.1);
          if (byId.get(edge.visibleSource)?.kind !== 'Region' || byId.get(edge.visibleTarget)?.kind !== 'Region')
            expect(Math.min(Math.abs(point.x - bounds.x), Math.abs(point.x - bounds.x - bounds.width),
              Math.abs(point.y - bounds.y), Math.abs(point.y - bounds.y - bounds.height))).toBeLessThan(0.1);
        }
      }
    }
  });
});

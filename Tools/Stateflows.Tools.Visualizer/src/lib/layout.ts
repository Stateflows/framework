import ELK, { type ElkNode, type ElkExtendedEdge } from 'elkjs/lib/elk.bundled.js';
import type { DiagramDocument, DiagramLayout, DiagramNode, DiagramPresentation, LayoutEdge, LayoutNode, Point } from './model.js';
import { operationRows } from './stateMachine.js';

const elk = new ELK();

export function validateDocument(document: DiagramDocument): void {
  const nodes = new Map(document.nodes.map(node => [node.id, node]));
  const ids = new Set<string>();
  for (const element of [...document.nodes, ...document.edges]) {
    if (!element.id || ids.has(element.id)) throw new Error(`Missing or duplicate element identity: ${element.id}`);
    ids.add(element.id);
  }
  for (const node of document.nodes) {
    const visited = new Set([node.id]);
    let parent = node.parentId;
    while (parent !== undefined) {
      if (!nodes.has(parent)) throw new Error(`Unknown parent ${parent} of ${node.id}`);
      if (visited.has(parent)) throw new Error(`Containment cycle at ${parent}`);
      visited.add(parent);
      parent = nodes.get(parent)?.parentId;
    }
  }
  for (const edge of document.edges) {
    if (!nodes.has(edge.source) || (edge.target !== undefined && !nodes.has(edge.target)))
      throw new Error(`Unresolved endpoint on ${edge.id}`);
  }
}

export function dimensions(node: DiagramNode, direction: 'RIGHT' | 'DOWN' = 'RIGHT'): { width: number; height: number } {
  if (['Initial', 'Junction'].includes(node.kind)) return { width: 30, height: 30 };
  if (['Final', 'FinalState', 'History', 'Choice', 'Decision', 'Merge'].includes(node.kind))
    return { width: 52, height: 52 };
  if (['Fork', 'Join'].includes(node.kind))
    return direction === 'RIGHT' ? { width: 18, height: 100 } : { width: 100, height: 18 };
  if (['InputPin', 'OutputPin'].includes(node.kind)) return { width: 160, height: 62 };
  if (node.notation === 'stateMachine') return {
    width: Math.max(180, ...[node.label, ...(node.details ?? [])].map(text => text.length * 7 + 92)),
    height: 42 + (node.details?.length ? Math.max(38, node.details.length * 18 + 20) : 0)
  };
  return {
    width: Math.min(340, Math.max(180, node.label.length * 7 + 34)),
    height: 78 + Math.min(node.details?.length ?? 0, 3) * 18
  };
}

export async function layoutDiagram(document: DiagramDocument, presentation: DiagramPresentation = {}): Promise<DiagramLayout> {
  document = { ...document, nodes: document.nodes.map(node => node.notation === 'stateMachine' || document.family === 'stateMachine'
    ? { ...node, operations: operationRows(node), details: operationRows(node).map(operation => operation.label) } : node) };
  validateDocument(document);
  const started = performance.now();
  const orderingOptions = !presentation.stateOrdering || presentation.stateOrdering === 'AUTOMATIC' ? {} : {
    'elk.layered.considerModelOrder.crossingCounterNodeInfluence': '0.001',
    ...(presentation.stateOrdering === 'PRESERVE_DEFINITION'
      ? { 'elk.layered.considerModelOrder.components': 'FORCE_MODEL_ORDER',
        'elk.layered.crossingMinimization.strategy': 'INTERACTIVE' } : {})
  };
  const collapsed = new Set(presentation.collapsedIds ?? []);
  const collapsedOperations = new Set(presentation.collapsedOperationIds ?? []);
  const collapsedSubstates = new Set(presentation.collapsedSubstateIds ?? []);
  function hasSubstatesCompartment(node: DiagramNode) {
    return children.has(node.id) && node.kind !== 'Region' &&
      children.get(node.id)!.filter(child => child.kind === 'Region').length <= 1;
  }
  function substatesHidden(id: string) {
    return collapsedSubstates.has(id) && hasSubstatesCompartment(original.get(id)!);
  }
  function nodeDimensions(node: DiagramNode) {
    if (node.notation !== 'stateMachine') return dimensions(node, presentation.direction);
    if (node.kind === 'Region' && collapsed.has(node.id)) return { width: 180, height: 38 };
    const size = dimensions(collapsed.has(node.id) || collapsedOperations.has(node.id)
      ? { ...node, details: [] } : node, presentation.direction);
    if (!collapsed.has(node.id)) {
      if (collapsedOperations.has(node.id) && node.details?.length) size.height += 38;
      if (hasSubstatesCompartment(node)) size.height += 38;
    }
    return size;
  }
  const original = new Map(document.nodes.map(node => [node.id, node]));
  const children = new Map<string, DiagramNode[]>();
  for (const node of document.nodes)
    if (node.parentId) children.set(node.parentId, [...(children.get(node.parentId) ?? []), node]);

  function representative(id: string): string {
    let visible = id;
    let parent = original.get(id)?.parentId;
    while (parent) {
      if (collapsed.has(parent) || substatesHidden(parent)) visible = parent;
      parent = original.get(parent)?.parentId;
    }
    return visible;
  }
  const visible = document.nodes.filter(node => representative(node.id) === node.id);
  const graphNodes = new Map<string, ElkNode>();
  let orderPosition = 0;
  for (const node of visible) {
    const container = children.has(node.id) && !collapsed.has(node.id) && !substatesHidden(node.id);
    const regionContainer = node.notation === 'stateMachine' && children.get(node.id)?.every(child => child.kind === 'Region');
    const size = nodeDimensions(node);
    // Interactive crossing minimization orders siblings by these definition-ordered seed coordinates.
    const orderSeed = presentation.stateOrdering === 'PRESERVE_DEFINITION'
      ? { x: orderPosition, y: orderPosition } : {};
    orderPosition += Math.max(size.width, size.height) + 42;
    graphNodes.set(node.id, {
      id: node.id, ...size, ...orderSeed,
      children: container ? [] : undefined,
      layoutOptions: {
        ...orderingOptions,
        'elk.padding': `[top=${node.notation === 'stateMachine' ? nodeDimensions(node).height + (regionContainer ? 0 : 20) : 54},left=24,bottom=${regionContainer ? 1.5 : 24},right=24]`,
        'elk.nodeSize.minimum': `(${nodeDimensions(node).width},${nodeDimensions(node).height})`,
        ...(regionContainer ? { 'elk.spacing.nodeNode': '0', 'elk.layered.spacing.nodeNodeBetweenLayers': '0' } : {}),
        'elk.direction': node.kind.includes('Orthogonal') ? 'DOWN' : presentation.direction ?? 'RIGHT'
      }
    });
  }
  const root: ElkNode = {
    id: '__root__', children: [], edges: [],
    layoutOptions: {
      ...orderingOptions,
      'elk.algorithm': 'layered', 'elk.hierarchyHandling': 'INCLUDE_CHILDREN',
      'elk.direction': presentation.direction ?? 'RIGHT',
      'elk.edgeRouting': presentation.routing === 'SPLINES' ? 'POLYLINE' : presentation.routing ?? 'ORTHOGONAL',
      'elk.spacing.nodeNode': '42', 'elk.layered.spacing.nodeNodeBetweenLayers': '90',
      'elk.spacing.edgeNode': '24', 'elk.padding': '[top=35,left=35,bottom=35,right=35]'
    }
  };
  for (const node of visible) {
    const item = graphNodes.get(node.id)!;
    if (node.parentId) graphNodes.get(node.parentId)!.children!.push(item);
    else root.children!.push(item);
  }
  const edgeModels = new Map<string, LayoutEdge>();
  const routingNodes = new Set<string>();
  const segments = new Map<string, { edgeId: string; index: number }>();
  const usedIds = new Set([...original.keys(), ...document.edges.map(edge => edge.id)]);
  function routingId(base: string) {
    let id = base;
    while (usedIds.has(id)) id += ':';
    usedIds.add(id);
    return id;
  }
  function isAncestor(ancestor: string, descendant: string) {
    let parent = original.get(descendant)?.parentId;
    while (parent) {
      if (parent === ancestor) return true;
      parent = original.get(parent)?.parentId;
    }
    return false;
  }
  function boundaryPort(nodeId: string, edgeId: string, endpoint: string) {
    const node = graphNodes.get(nodeId)!;
    const id = routingId(`${edgeId}:${endpoint}`);
    (node.ports ??= []).push({
      id, width: 0, height: 0,
      layoutOptions: { 'elk.port.side': presentation.direction === 'DOWN' ? 'NORTH' : 'WEST' }
    });
    node.layoutOptions!['elk.portConstraints'] = 'FIXED_SIDE';
    return id;
  }
  for (const edge of document.edges) {
    if (document.family === 'stateMachine' && edge.target === undefined) continue;
    const source = representative(edge.source);
    const target = representative(edge.target ?? edge.source);
    // Edges wholly hidden by a collapsed scope are represented by its hidden-element count.
    if (source === target && (source !== edge.source || target !== (edge.target ?? edge.source))) continue;
    const model: LayoutEdge = {
      ...edge, routing: presentation.routing ?? 'ORTHOGONAL', visibleSource: source, visibleTarget: target,
      hiddenEndpoints: source !== edge.source || target !== (edge.target ?? edge.source), paths: []
    };
    edgeModels.set(edge.id, model);
    const sourceAncestors: string[] = [];
    let ancestor = original.get(source)?.parentId;
    while (ancestor) { sourceAncestors.push(ancestor); ancestor = original.get(ancestor)?.parentId; }
    let owner = original.get(target)?.parentId;
    while (owner && !sourceAncestors.includes(owner)) owner = original.get(owner)?.parentId;
    const elkEdge: ElkExtendedEdge = {
      id: edge.id, sources: [source], targets: [target],
      labels: edge.label || edge.parts ? [{ text: edge.label || ' ', width: document.family === 'stateMachine'
        ? edge.label.length * 7 + 48 + (edge.parts && !edge.parts.trigger ? 70 : 0) : Math.min(600, edge.label.length * 7 + 12), height: 20 }] : []
    };
    const graph = owner ? graphNodes.get(owner)! : root;
    const ancestorEndpoint = isAncestor(source, target) ? source : isAncestor(target, source) ? target : undefined;
    if (document.family === 'stateMachine' && ancestorEndpoint && !edge.isLocal) {
      // An external ancestor transition must cross the state boundary, not use an interior hierarchy edge.
      const outsideId = routingId(`${edge.id}:outside`);
      const ancestorParent = original.get(ancestorEndpoint)!.parentId;
      const outsideGraph = ancestorParent ? graphNodes.get(ancestorParent)! : root;
      (outsideGraph.children ??= []).push({ id: outsideId, width: 0, height: 0 });
      routingNodes.add(outsideId);
      const sourcePort = boundaryPort(source, edge.id, 'source');
      const targetPort = boundaryPort(target, edge.id, 'target');
      for (const [index, endpoints] of [[sourcePort, outsideId], [outsideId, targetPort]].entries()) {
        const id = routingId(`${edge.id}:segment:${index}`);
        segments.set(id, { edgeId: edge.id, index });
        (outsideGraph.edges ??= []).push({
          ...elkEdge, id, sources: [endpoints[0]], targets: [endpoints[1]],
          labels: index === 0 ? elkEdge.labels : []
        });
      }
    } else {
      (graph.edges ??= []).push(elkEdge);
    }
  }
  const result = await elk.layout(root);
  const nodes: LayoutNode[] = [];
  const graphOffsets = new Map<string, Point>();
  function indexOffsets(graph: ElkNode, parent: Point) {
    const offset = { x: parent.x + (graph.x ?? 0), y: parent.y + (graph.y ?? 0) };
    graphOffsets.set(graph.id, offset);
    for (const child of graph.children ?? []) indexOffsets(child, offset);
  }
  indexOffsets(result, { x: 0, y: 0 });
  const segmentPaths = new Map<string, Point[][][]>();
  type Compartment = { x: number; y: number; width: number; height: number; separator: boolean; columnSeparator: boolean };
  const compartmentBounds = new Map<string, { x: number; y: number; width: number; height: number }>();
  function regionCompartments(graph: ElkNode): Map<string, Compartment> {
    const regions = (graph.children ?? []).filter(child =>
      original.get(child.id)?.notation === 'stateMachine' && original.get(child.id)?.kind === 'Region');
    const bounds = new Map<string, Compartment>();
    if (!regions.length) return bounds;
    const top = nodeDimensions(original.get(graph.id)!).height;
    function partition(items: ElkNode[], area: Compartment): void {
      if (items.length === 1) { bounds.set(items[0].id, area); return; }
      // Prefer horizontal separators; connected regions may require side-by-side compartments.
      for (const axis of ['y', 'x'] as const) {
        const size = axis === 'y' ? 'height' : 'width';
        const sorted = [...items].sort((a, b) => (a[axis] ?? 0) - (b[axis] ?? 0));
        let end = (sorted[0][axis] ?? 0) + sorted[0][size]!;
        for (let index = 1; index < sorted.length; index++) {
          const start = sorted[index][axis] ?? 0;
          if (end <= start) {
            const boundary = (end + start) / 2;
            partition(sorted.slice(0, index), { ...area, [size]: boundary - area[axis] });
            partition(sorted.slice(index), {
              ...area, [axis]: boundary, [size]: area[axis] + area[size] - boundary,
              ...(axis === 'y' ? { separator: true } : { columnSeparator: true })
            });
            return;
          }
          end = Math.max(end, start + sorted[index][size]!);
        }
      }
      throw new Error(`Layout cannot separate overlapping regions in ${graph.id}`);
    }
    partition(regions, { x: 1.5, y: top, width: graph.width! - 3,
      height: graph.height! - top - 1.5,
      separator: false, columnSeparator: false });
    return bounds;
  }
  function collect(graph: ElkNode, absolute: Point, parentDisplay: Point, compartment?: Compartment): void {
    if (routingNodes.has(graph.id)) return;
    const offset = { x: absolute.x + (graph.x ?? 0), y: absolute.y + (graph.y ?? 0) };
    const display = compartment ? { x: absolute.x + compartment.x, y: absolute.y + compartment.y } : offset;
    if (compartment) compartmentBounds.set(graph.id, { ...display, width: compartment.width, height: compartment.height });
    if (graph !== result) {
      const node = original.get(graph.id)!;
      nodes.push({
        ...node, position: { x: display.x - parentDisplay.x, y: display.y - parentDisplay.y },
        operationsCollapsed: collapsedOperations.has(node.id),
        substatesCollapsed: substatesHidden(node.id),
        hasSubstatesCompartment: hasSubstatesCompartment(node),
        width: compartment?.width ?? graph.width!, height: compartment?.height ?? graph.height!,
        regionSeparator: compartment?.separator, regionColumnSeparator: compartment?.columnSeparator,
        container: children.has(node.id),
        hiddenCount: document.nodes.filter(candidate => candidate.id !== node.id && representative(candidate.id) === node.id).length
      });
    }
    for (const edge of graph.edges ?? []) {
      const segment = segments.get(edge.id);
      const model = edgeModels.get(segment?.edgeId ?? edge.id)!;
      const edgeOffset = graphOffsets.get(edge.container ?? graph.id)!;
      const paths = (edge.sections ?? []).map(section =>
        [section.startPoint, ...(section.bendPoints ?? []), section.endPoint].map(point =>
          ({ x: point.x + edgeOffset.x, y: point.y + edgeOffset.y })));
      if (segment) {
        const parts = segmentPaths.get(model.id) ?? [];
        parts[segment.index] = paths;
        segmentPaths.set(model.id, parts);
      } else model.paths = paths;
      const label = edge.labels?.[0];
      if (label) model.labelPosition = {
        x: (label.x ?? 0) + edgeOffset.x + (label.width ?? 0) / 2,
        y: (label.y ?? 0) + edgeOffset.y + (label.height ?? 0) / 2
      };
      if (!paths.length) throw new Error(`Layout produced no route for ${edge.id}`);
    }
    const compartments = regionCompartments(graph);
    for (const child of graph.children ?? []) {
      // Only presentation bounds change: children and routed edges retain their absolute ELK coordinates.
      collect(child, offset, display, compartments.get(child.id));
    }
  }
  collect(result, { x: 0, y: 0 }, { x: 0, y: 0 });
  const absoluteBounds = new Map<string, { x: number; y: number; width: number; height: number }>();
  for (const node of nodes) {
    const parent = node.parentId ? absoluteBounds.get(node.parentId)! : { x: 0, y: 0 };
    absoluteBounds.set(node.id, {
      x: parent.x + node.position.x, y: parent.y + node.position.y, width: node.width, height: node.height
    });
  }
  for (const [id, parts] of segmentPaths) {
    const points = parts.flat(2);
    const model = edgeModels.get(id)!;
    const source = points[0];
    const target = points.at(-1)!;
    const ancestor = isAncestor(model.visibleSource, model.visibleTarget) ? model.visibleSource : model.visibleTarget;
    const bounds = absoluteBounds.get(ancestor)!;
    const gutter = Math.max(24, model.label.length * 7 + 24);
    const vertical = presentation.direction === 'DOWN';
    const lane = vertical ? bounds.y - gutter : bounds.x - gutter;
    const compact = vertical
      ? [source, { x: source.x, y: lane }, { x: target.x, y: lane }, target]
      : [source, { x: lane, y: source.y }, { x: lane, y: target.y }, target];
    const containingId = original.get(ancestor)!.parentId;
    const containingBounds = containingId ? absoluteBounds.get(containingId)! : undefined;
    const withinContainer = !containingBounds || compact.every(point =>
      point.x >= containingBounds.x && point.x <= containingBounds.x + containingBounds.width &&
      point.y >= containingBounds.y && point.y <= containingBounds.y + containingBounds.height);
    const obstructed = nodes.some(node => {
      if (node.id === model.visibleSource || node.id === model.visibleTarget ||
          isAncestor(node.id, model.visibleSource) || isAncestor(node.id, model.visibleTarget)) return false;
      const box = absoluteBounds.get(node.id)!;
      return compact.slice(1).some((end, index) => {
        const start = compact[index];
        return Math.max(start.x, end.x) > box.x && Math.min(start.x, end.x) < box.x + box.width &&
          Math.max(start.y, end.y) > box.y && Math.min(start.y, end.y) < box.y + box.height;
      });
    });
    // Prefer a short boundary loop only when it is clear; otherwise retain ELK's obstacle-avoiding route.
    const distinctPorts = Math.abs(vertical ? source.x - target.x : source.y - target.y) >= 16;
    const useCompact = withinContainer && !obstructed && distinctPorts;
    model.paths = [useCompact ? compact : points];
    if (useCompact && model.label)
      model.labelPosition = vertical
        ? { x: (source.x + target.x) / 2, y: lane - 14 }
        : { x: (lane + bounds.x) / 2, y: Math.min(source.y, target.y) - 14 };
  }
  function trimToCompartment(path: readonly Point[], id: string): readonly Point[] {
    const bounds = compartmentBounds.get(id);
    if (!bounds) return path;
    const right = bounds.x + bounds.width;
    const bottom = bounds.y + bounds.height;
    const outside = path.findIndex(point => point.x < bounds.x || point.x > right || point.y < bounds.y || point.y > bottom);
    if (outside <= 0) return path;
    const point = path[outside];
    return [{ x: Math.max(bounds.x, Math.min(right, point.x)), y: Math.max(bounds.y, Math.min(bottom, point.y)) },
      ...path.slice(outside)];
  }
  for (const edge of edgeModels.values()) {
    // Adjacent collapsed compartments share a divider; keep distinct ports so their connecting arrow remains visible.
    if (compartmentBounds.has(edge.visibleSource) && compartmentBounds.has(edge.visibleTarget)) continue;
    edge.paths = edge.paths.map((path, index) => {
      let points = index === 0 ? trimToCompartment(path, edge.visibleSource) : path;
      if (index === edge.paths.length - 1)
        points = [...trimToCompartment([...points].reverse(), edge.visibleTarget)].reverse();
      return points;
    });
  }
  return { nodes, edges: [...edgeModels.values()], durationMs: performance.now() - started };
}

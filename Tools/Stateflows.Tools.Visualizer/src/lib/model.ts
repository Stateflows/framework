export type DiagramMode = 'definition' | 'live' | 'execution';
export type ElementStatus =
  | 'active' | 'activated' | 'running' | 'entered' | 'exited' | 'executed'
  | 'taken' | 'guard-rejected' | 'waiting' | 'failed';

export interface DiagramNode {
  entryBreakpoint?: boolean;
  embeddedBehavior?: EmbeddedBehavior;
  id: string;
  label: string;
  kind: string;
  parentId?: string;
  initial?: boolean;
  details?: readonly string[];
  operations?: readonly DiagramOperation[];
  notation?: 'stateMachine';
  selectable?: boolean;
}

export interface EmbeddedBehavior {
  type: string;
  name: string;
  version?: number;
}

export interface BreakpointTarget {
  elementId: string;
  operation: string;
}

export interface LabelParts {
  trigger: string;
  guard?: string;
  effect?: string;
}

/** Lines with the same selectionId represent one state operation, not individual C# handlers. */
export interface DiagramOperation {
  id: string;
  selectionId: string;
  label: string;
  target: BreakpointTarget;
  parts?: LabelParts;
  embeddedBehavior?: EmbeddedBehavior;
  breakpointable?: boolean;
}

export interface DiagramEdge {
  triggers?: readonly string[];
  selectable?: boolean;
  id: string;
  source: string;
  target?: string;
  label: string;
  kind: 'transition' | 'control' | 'object' | 'transformation';
  isElse?: boolean;
  isLocal?: boolean;
  weight?: number;
  parts?: LabelParts;
}

/** Semantic identity is independent of the renderer and scoped to this definition/version. */
export interface DiagramDocument {
  id: string;
  name: string;
  version: number;
  family: 'stateMachine' | 'activity' | 'action';
  nodes: readonly DiagramNode[];
  edges: readonly DiagramEdge[];
}

export interface ElementObservation {
  elementId: string;
  status: ElementStatus;
  running?: number;
  tokenCount?: number;
  requiredTokens?: number;
  operation?: string;
}

export interface DiagramOverlay {
  documentId: string;
  instance: string;
  observedAt: string;
  elements: readonly ElementObservation[];
}

export interface DiagramSelection {
  documentId: string;
  elementId: string;
  kind: 'node' | 'edge' | 'operation';
  operation?: string;
  ownerId?: string;
}

export interface DiagramPresentation {
  stateOrdering?: 'AUTOMATIC' | 'PREFER_DEFINITION' | 'PRESERVE_DEFINITION';
  routing?: 'ORTHOGONAL' | 'POLYLINE' | 'SPLINES';
  direction?: 'RIGHT' | 'DOWN';
  collapsedIds?: readonly string[];
  collapsedOperationIds?: readonly string[];
  collapsedSubstateIds?: readonly string[];
}

export interface Point { x: number; y: number }
export interface LayoutNode extends DiagramNode {
  regionSeparator?: boolean;
  operationsCollapsed?: boolean;
  substatesCollapsed?: boolean;
  hasSubstatesCompartment?: boolean;
  regionColumnSeparator?: boolean;
  position: Point;
  width: number;
  height: number;
  container: boolean;
  hiddenCount: number;
}

export interface LayoutEdge extends DiagramEdge {
  routing?: DiagramPresentation['routing'];
  visibleSource: string;
  visibleTarget: string;
  hiddenEndpoints: boolean;
  paths: readonly (readonly Point[])[];
  labelPosition?: Point;
}

export interface DiagramLayout {
  nodes: LayoutNode[];
  edges: LayoutEdge[];
  durationMs: number;
}

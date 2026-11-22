<script lang="ts">
  import { onMount } from 'svelte';
  import type { Snippet } from 'svelte';
  import { SvelteFlow, Background, Controls, MiniMap, MarkerType, Position, type Node, type Edge } from '@xyflow/svelte';
  import '@xyflow/svelte/dist/style.css';
  import DiagramNode, { type NodeData } from './DiagramNode.svelte';
  import DiagramEdge, { type EdgeData } from './DiagramEdge.svelte';
  import type { DiagramDocument, DiagramLayout, DiagramMode, DiagramOverlay, DiagramPresentation, DiagramSelection, ElementObservation, EmbeddedBehavior, BreakpointTarget, DiagramOperation } from './model.js';
  import { operationRows, searchElements, triggerNames } from './stateMachine.js';

  interface Props {
    focusedIds?: readonly string[];
    onfocuschange?: (ids: readonly string[]) => void;
    hiddenTriggers?: readonly string[];
    ontriggerfilterchange?: (triggers: readonly string[]) => void;
    settings?: Snippet;
    document: DiagramDocument;
    mode?: DiagramMode;
    overlay?: DiagramOverlay;
    presentation?: DiagramPresentation;
    selectedId?: string;
    onselect?: (selection: DiagramSelection | undefined) => void;
    onpresentationchange?: (presentation: DiagramPresentation) => void;
    onlayout?: (layout: DiagramLayout) => void;
    onerror?: (error: Error) => void;
    onopenbehavior?: (behavior: EmbeddedBehavior, elementId: string) => void;
    allowBreakpoints?: boolean;
    breakpoints?: readonly BreakpointTarget[];
    onbreakpointchange?: (target: BreakpointTarget, enabled: boolean) => void;
  }
  let { document, mode = 'definition', overlay, presentation = {}, selectedId,
    onselect, onpresentationchange, onlayout, onerror, onopenbehavior,
    allowBreakpoints = false, breakpoints = [], onbreakpointchange, settings,
    focusedIds, onfocuschange, hiddenTriggers, ontriggerfilterchange }: Props = $props();
  const nodeTypes = { stateflows: DiagramNode };
  const edgeTypes = { stateflows: DiagramEdge };
  let worker = $state<Worker>();
  let layout = $state<DiagramLayout>();
  let error = $state('');
  let busy = $state(false);
  let collapsedIds = $state<string[]>([]);
  let localSelection = $state<string>();
  let query = $state('');
  let localFocus = $state<string[]>([]);
  let localHiddenTriggers = $state<string[]>([]);
  let searched = $state(false);
  let focus = $derived(new Set(focusedIds ?? localFocus));
  let excludedTriggers = $derived(hiddenTriggers ?? localHiddenTriggers);
  let triggers = $derived([...new Set(document.edges.filter(edge => edge.selectable !== false).flatMap(triggerNames))].sort());
  let hiddenEdges = $derived(new Set(document.edges.filter(edge => {
    const names = triggerNames(edge);
    return names.length > 0 && names.every(name => excludedTriggers.includes(name));
  }).map(edge => edge.id)));
  let request = 0;
  let currentDocument = '';
  let selection = $derived(selectedId ?? localSelection);
  let overlayMismatch = $derived(mode !== 'definition' && overlay !== undefined && overlay.documentId !== document.id);
  let observations = $derived.by(() => {
    const result = new Map<string, ElementObservation[]>();
    if (mode !== 'definition' && overlay && !overlayMismatch)
      for (const observation of overlay.elements)
        result.set(observation.elementId, [...(result.get(observation.elementId) ?? []), observation]);
    return result;
  });
  let nodes = $derived<Node<NodeData>[]>((layout?.nodes ?? []).map(node => ({
    id: node.id, type: 'stateflows', position: node.position, parentId: node.parentId,
    width: node.width, height: node.height, selected: node.selectable !== false && selection === node.id,
    selectable: node.selectable !== false, focusable: node.selectable !== false,
    zIndex: allowBreakpoints && node.entryBreakpoint && !node.container ? 3 : undefined,
    style: `width:${node.width}px;height:${node.height}px;`,
    data: { model: node, observations: observations.get(node.id) ?? [], collapsed: collapsedIds.includes(node.id),
      focused: focus.has(node.id), focusedIds: focus, hiddenEdges,
      selectedOperation: selection, onselectoperation: selectOperation, allowBreakpoints, breakpoints, ontogglebreakpoint: toggleBreakpoint,
      ontoggle: toggle, ontoggleoperations: toggleOperations, ontogglesubstates: toggleSubstates,
      onopenbehavior: (behavior, elementId) => {
        if (onopenbehavior) onopenbehavior(behavior, elementId);
        else reportError(`No navigation handler is configured for ${behavior.type} '${behavior.name}'.`);
      } }
  })));
  let edges = $derived<Edge<EdgeData>[]>((layout?.edges ?? []).map(edge => ({
    id: edge.id, source: edge.visibleSource, target: edge.visibleTarget, type: 'stateflows',
    sourceHandle: `source-${Position.Right}`, targetHandle: `target-${Position.Left}`,
    selected: edge.selectable !== false && selection === edge.id, zIndex: 2,
    selectable: edge.selectable !== false, focusable: edge.selectable !== false,
    hidden: hiddenEdges.has(edge.id),
    markerEnd: { type: MarkerType.ArrowClosed, color: '#94a3b8' },
    data: { model: edge, focused: focus.has(edge.id), observations: observations.get(edge.id) ?? [], allowBreakpoints, breakpoints, ontogglebreakpoint: toggleBreakpoint }
  })));

  onMount(() => {
    const instance = new Worker(new URL('./layout.worker.js', import.meta.url), { type: 'module' });
    worker = instance;
    instance.onmessage = (event: MessageEvent<{ id: number; layout?: DiagramLayout; error?: string }>) => {
      if (event.data.id !== request) return;
      busy = false;
      if (event.data.error) reportError(event.data.error);
      else if (event.data.layout) {
        layout = event.data.layout;
        error = '';
        onlayout?.(event.data.layout);
      }
    };
    instance.onerror = event => { busy = false; reportError(event.message); };
    return () => instance.terminate();
  });

  function reportError(message: string) { error = message; onerror?.(new Error(message)); }
  $effect(() => {
    if (document.id !== currentDocument) {
      currentDocument = document.id;
      localSelection = undefined;
      localFocus = [];
      localHiddenTriggers = [];
      query = '';
      searched = false;
      layout = undefined;
    }
    collapsedIds = [...(presentation.collapsedIds ?? [])];
  });
  $effect(() => {
    if (!worker) return;
    busy = true;
    worker.postMessage({ id: ++request, document: $state.snapshot(document),
      presentation: { direction: presentation.direction, routing: presentation.routing, stateOrdering: presentation.stateOrdering, collapsedIds: [...collapsedIds],
        collapsedOperationIds: [...(presentation.collapsedOperationIds ?? [])],
        collapsedSubstateIds: [...(presentation.collapsedSubstateIds ?? [])] } });
  });
  $effect(() => {
    if (!selection) return;
    const edge = document.edges.find(edge => edge.id === selection);
    const operationOwner = document.nodes.find(node => operationRows(node).some(item => item.selectionId === selection));
    const targets = edge ? [edge.source, edge.target].filter((id): id is string => id !== undefined) : [operationOwner?.id ?? selection];
    const ancestors = new Set<string>();
    for (const target of targets) {
      let parent = document.nodes.find(node => node.id === target)?.parentId;
      while (parent) { ancestors.add(parent); parent = document.nodes.find(node => node.id === parent)?.parentId; }
    }
    const next = collapsedIds.filter(id => !ancestors.has(id));
    if (next.length !== collapsedIds.length) collapsedIds = next;
    const substateIds = presentation.collapsedSubstateIds ?? [];
    if (substateIds.some(id => ancestors.has(id)))
      presentation = { ...presentation, collapsedSubstateIds: substateIds.filter(id => !ancestors.has(id)) };
  });
  function toggle(id: string) {
    localSelection = undefined;
    onselect?.(undefined);
    collapsedIds = collapsedIds.includes(id) ? collapsedIds.filter(item => item !== id) : [...collapsedIds, id];
    onpresentationchange?.({ ...presentation, collapsedIds });
  }
  function select(elementId: string, kind: 'node' | 'edge') {
    const element = kind === 'node' ? document.nodes.find(node => node.id === elementId) : document.edges.find(edge => edge.id === elementId);
    if (element?.selectable === false) return;
    localSelection = elementId;
    onselect?.({ documentId: document.id, elementId, kind });
  }
  function selectOperation(operation: DiagramOperation) {
    localSelection = operation.selectionId;
    const owner = document.nodes.find(node => operationRows(node).some(item => item.id === operation.id));
    onselect?.({ documentId: document.id, elementId: operation.selectionId, kind: 'operation',
      operation: operation.target.operation, ownerId: owner?.id });
  }
  function toggleBreakpoint(target: BreakpointTarget) {
    if (!allowBreakpoints) return;
    if (!onbreakpointchange) { reportError('No breakpoint handler is configured.'); return; }
    onbreakpointchange(target, !breakpoints.some(item => item.elementId === target.elementId && item.operation === target.operation));
  }
  function toggleOperations(id: string) {
    const ids = presentation.collapsedOperationIds ?? [];
    presentation = { ...presentation, collapsedOperationIds: ids.includes(id) ? ids.filter(item => item !== id) : [...ids, id] };
    onpresentationchange?.(presentation);
  }
  function toggleSubstates(id: string) {
    localSelection = undefined;
    onselect?.(undefined);
    const ids = presentation.collapsedSubstateIds ?? [];
    presentation = { ...presentation, collapsedSubstateIds: ids.includes(id) ? ids.filter(item => item !== id) : [...ids, id] };
    onpresentationchange?.(presentation);
  }
  function search(event: SubmitEvent) {
    event.preventDefault();
    localFocus = searchElements(document, query);
    searched = !!query.trim();
    onfocuschange?.(localFocus);
  }
  function clearFocus() {
    localFocus = []; query = ''; searched = false; onfocuschange?.([]);
  }
  function toggleTrigger(trigger: string) {
    localHiddenTriggers = excludedTriggers.includes(trigger)
      ? excludedTriggers.filter(item => item !== trigger) : [...excludedTriggers, trigger];
    ontriggerfilterchange?.(localHiddenTriggers);
  }
</script>

<div class="visualizer" aria-label={`${document.name} ${mode} diagram`} data-mode={mode}>
  <div class="toolbar">
    <strong>{document.name}</strong><span>v{document.version}</span><span class="mode">{mode}</span>
    <form onsubmit={search}><input aria-label="Find diagram element" placeholder="Find state or node" bind:value={query} /><button>Find</button></form>
    {#if searched}<span role="status">{focus.size} matches</span>{/if}
    {#if focus.size || searched}<button onclick={clearFocus}>Clear focus</button>{/if}
    {#if triggers.length}
      <details class="trigger-filter"><summary>Triggers ({triggers.filter(trigger => !excludedTriggers.includes(trigger)).length}/{triggers.length})</summary>
        <div class="trigger-options">
          <button onclick={() => { localHiddenTriggers = []; ontriggerfilterchange?.([]); }}>Show all triggers</button>
          {#each triggers as trigger}<label><input type="checkbox" checked={!excludedTriggers.includes(trigger)}
            onchange={() => toggleTrigger(trigger)} />{trigger}</label>{/each}
        </div>
      </details>
    {/if}
    {@render settings?.()}
    {#if busy}<span role="status">Laying out…</span>{/if}
  </div>
  {#if error}<div class="error" role="alert">{error}</div>{/if}
  {#if overlayMismatch}<div class="error" role="alert">Overlay belongs to another definition/version; it has not been applied.</div>{/if}
  <div class="canvas" data-layout-ready={!!layout && !busy}>
    {#if layout}
      {#key document.id}
      <SvelteFlow {nodes} {edges} {nodeTypes} {edgeTypes} fitView minZoom={0.001} maxZoom={2.5}
        fitViewOptions={{ minZoom: 0.001 }}
        nodesDraggable={false} nodesConnectable={false} elementsSelectable
        onnodeclick={({ node }) => select(node.id, 'node')}
        onedgeclick={({ edge }) => select(edge.id, 'edge')}>
        <Background patternColor="var(--sfv-grid, #24334c)" />
        <Controls showLock={false} />
        <MiniMap pannable zoomable />
      </SvelteFlow>
      {/key}
    {/if}
  </div>
  <div class="legend">
    <span>Unmarked: no displayed observation</span>
    {#if mode !== 'definition'}
      <span class="active">Active / running</span><span class="success">Activated / executed / taken</span>
      <span class="waiting">Waiting / guard rejected</span><span class="failed">Failed</span>
      <span>{overlay ? `${overlay.instance} · ${overlay.observedAt}` : 'No observation supplied'}</span>
    {/if}
  </div>
</div>

<style>
  .visualizer { height: 100%; min-height: 350px; display: flex; flex-direction: column;
    background: var(--sfv-bg, #0b1424); color: var(--sfv-text, #e2e8f0); font: 12px var(--sfv-font, system-ui, sans-serif); }
  .toolbar { display: flex; flex-wrap: wrap; gap: 12px; align-items: center; padding: 12px 16px; border-bottom: 1px solid var(--sfv-border, #334155); }
  .toolbar strong { font-size: 14px; } .mode { text-transform: uppercase; letter-spacing: .12em; font-size: 10px; }
  form { margin-left: auto; display: flex; gap: 5px; }
  input, button { background: var(--sfv-node-bg, #152137); color: inherit; border: 1px solid var(--sfv-border, #52617b); border-radius: 5px; padding: 5px 8px; }
  button { cursor: pointer; } .canvas { flex: 1; min-height: 250px; }
  .trigger-filter { position: relative; }
  summary { cursor: pointer; }
  .trigger-options { position: absolute; top: 100%; left: 0; z-index: 10; min-width: 200px; max-height: 300px; overflow: auto;
    background: var(--sfv-node-bg, #152137); padding: 10px; border: 1px solid var(--sfv-border, #52617b); }
  .trigger-options label { display: flex; gap: 6px; align-items: center; padding: 4px; white-space: nowrap; }
  .error { padding: 12px; background: #651a2d; color: #fff; }
  .legend { padding: 8px 16px; display: flex; flex-wrap: wrap; gap: 14px; color: var(--sfv-muted, #94a3b8); border-top: 1px solid var(--sfv-border, #334155); font-size: 10px; }
  .active { color: var(--sfv-active, #38bdf8); } .success { color: var(--sfv-success, #34d399); }
  .waiting { color: var(--sfv-waiting, #fbbf24); } .failed { color: var(--sfv-failed, #fb7185); }
  :global(.svelte-flow__controls button) { background: var(--sfv-node-bg, #152137); fill: var(--sfv-text, #e2e8f0); }
  :global(.svelte-flow__minimap) { background: var(--sfv-node-bg, #152137); }
</style>

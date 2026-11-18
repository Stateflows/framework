<script module lang="ts">
  import type { LayoutEdge, ElementObservation, BreakpointTarget } from './model.js';

  export type EdgeData = { model: LayoutEdge; focused: boolean; observations: readonly ElementObservation[];
    allowBreakpoints: boolean; breakpoints: readonly BreakpointTarget[]; ontogglebreakpoint: (target: BreakpointTarget) => void };
</script>
<script lang="ts">
  import { BaseEdge, EdgeLabel, type EdgeProps, type Edge } from '@xyflow/svelte';
  import { routedPath } from './edgePath.js';
  import OperationText from './OperationText.svelte';
  let { id, data, selected, markerEnd }: EdgeProps<Edge<EdgeData>> = $props();
  let model = $derived(data!.model);
  let observation = $derived(data!.observations.at(-1));
  let path = $derived(model.paths.map(points => routedPath(points, model.routing)).join(' '));
  let color = $derived(selected ? 'var(--sfv-selection, #c4b5fd)'
    : data!.focused ? 'var(--sfv-focus, #fbbf24)'
    : observation?.status === 'failed' ? 'var(--sfv-failed, #fb7185)'
    : ['waiting', 'guard-rejected'].includes(observation?.status ?? '') ? 'var(--sfv-waiting, #fbbf24)'
    : ['taken', 'executed'].includes(observation?.status ?? '') ? 'var(--sfv-success, #34d399)'
    : 'var(--sfv-edge, #94a3b8)');
  let tokenNames = $derived(model.tokenNames?.length
    ? ` · ${model.tokenNames.join(', ')}${model.targetTokenNames?.length ? ` → ${model.targetTokenNames.join(', ')}` : ''}` : '');
  let caption = $derived(`${model.label}${tokenNames}${model.hiddenEndpoints ? ' [hidden endpoint]' : ''}${observation ? ` · ${observation.status}` : ''}${observation?.tokenCount !== undefined ? ` ${observation.tokenCount}/${observation.requiredTokens ?? '?'}` : ''}`);
</script>

<BaseEdge {id} {path} {markerEnd} interactionWidth={20}
  style={`stroke: ${color}; stroke-width: ${selected || observation || data!.focused ? 2.5 : 1.5}; ${model.target === undefined || observation?.status === 'guard-rejected' ? 'stroke-dasharray: 6 4;' : ''}`} />
{#if model.labelPosition && (caption || (model.parts && data!.allowBreakpoints))}
  <EdgeLabel x={model.labelPosition.x} y={model.labelPosition.y} transparent style="padding:0">
    <span class="edge-label" class:selected class:empty={!caption && !data!.allowBreakpoints} data-edge-id={id} style={`color:${color}`}
      title={`${model.source} → ${model.target ?? '(internal)'}\n${caption}`}>
      {#if model.parts}
        <OperationText parts={model.parts} elementId={model.id} allowBreakpoints={data!.allowBreakpoints}
          breakpoints={data!.breakpoints} ontoggle={data!.ontogglebreakpoint} />
        {caption.slice(model.label.length)}
      {:else}{caption}{/if}
    </span>
  </EdgeLabel>
{/if}

<style>
  .edge-label { display: block; font: 11px/18px var(--sfv-font, system-ui, sans-serif);
    background: var(--sfv-label-bg, #0b1424ed); padding: 1px 3px; white-space: nowrap; pointer-events: none;
    border: 0; outline: 0; box-shadow: none; }
  .edge-label:has(:global(.breakpoint)) { pointer-events: auto; }
  .selected { font-weight: 650; }
  .empty { background: transparent; }
</style>

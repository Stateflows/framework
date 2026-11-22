<script module lang="ts">
  import type { LayoutNode, ElementObservation, EmbeddedBehavior, BreakpointTarget, DiagramOperation } from './model.js';

  export type NodeData = {
    focused: boolean;
    focusedIds: ReadonlySet<string>;
    hiddenEdges: ReadonlySet<string>;
    model: LayoutNode;
    observations: readonly ElementObservation[];
    collapsed: boolean;
    ontoggle: (id: string) => void;
    ontoggleoperations: (id: string) => void;
    ontogglesubstates: (id: string) => void;
    onopenbehavior: (behavior: EmbeddedBehavior, elementId: string) => void;
    selectedOperation?: string;
    onselectoperation: (operation: DiagramOperation) => void;
    allowBreakpoints: boolean;
    breakpoints: readonly BreakpointTarget[];
    ontogglebreakpoint: (target: BreakpointTarget) => void;
  };
</script>
<script lang="ts">
  import { Handle, Position, type NodeProps, type Node } from '@xyflow/svelte';
  import BehaviorIcon from './BehaviorIcon.svelte';
  import BreakpointButton from './BreakpointButton.svelte';
  import OperationText from './OperationText.svelte';
  let { data, selected }: NodeProps<Node<NodeData>> = $props();
  const markerKinds = ['Initial', 'Final', 'FinalState', 'Choice', 'Junction', 'History', 'Decision', 'Merge', 'Fork', 'Join'];
  let marker = $derived(markerKinds.includes(data.model.kind));
  let status = $derived(data.observations.at(-1)?.status);
  let badge = $derived(data.observations.map(item =>
    `${item.status}${item.running ? ` (${item.running})` : ''}${item.tokenCount !== undefined ? ` ${item.tokenCount}/${item.requiredTokens ?? '?'}` : ''}`).join(' | '));
</script>

<div
  class="shape"
  class:container={data.model.container}
  class:region={data.model.kind === 'Region'}
  class:marker
  class:uml={data.model.notation === 'stateMachine'}
  class:region-separator={data.model.regionSeparator}
  class:region-column-separator={data.model.regionColumnSeparator}
  class:selected
  class:focused={data.focused}
  data-kind={data.model.kind}
  data-status={status}
  data-element-id={data.model.id}
  title={`${data.model.label}\n${data.model.kind}${data.model.pinTokenNames?.length ? `\n${data.model.pinTokenNames.join(', ')}` : ''}${badge ? `\n${badge}` : ''}`}
>
  {#each [Position.Left, Position.Right, Position.Top, Position.Bottom] as position}
    <Handle type="source" {position} id={`source-${position}`} isConnectable={false} />
    <Handle type="target" {position} id={`target-${position}`} isConnectable={false} />
  {/each}
  {#if marker}
    <svg viewBox="0 0 100 100" preserveAspectRatio="none" aria-label={`${data.model.kind}: ${data.model.label}`}>
      {#if data.model.kind === 'Initial'}
        <circle cx="50" cy="50" r="48.5" class="solid" />
      {:else if data.model.kind === 'Junction'}
        <circle cx="50" cy="50" r="48.5" class="solid" />
      {:else if ['Final', 'FinalState'].includes(data.model.kind)}
        <circle cx="50" cy="50" r="40" /><circle cx="50" cy="50" r="27" class="solid" />
      {:else if ['Fork', 'Join'].includes(data.model.kind)}
        <rect x="0" y="0" width="100" height="100" class="solid" stroke="none" />
      {:else if data.model.kind === 'History'}
        <circle cx="50" cy="50" r="42" /><text x="50" y="65" text-anchor="middle">H</text>
      {:else}
        <path d="M50 4 L96 50 L50 96 L4 50 Z" />
      {/if}
    </svg>
    {#if selected}
      {@const width = data.model.width}
      {@const height = data.model.height}
      {@const diamondOffset = Math.SQRT2 * (width * 0.015 + 4.5)}
      <svg class="selection-contour" aria-hidden="true"
        viewBox={`-8 -8 ${width + 16} ${height + 16}`}>
        {#if ['Initial', 'Junction', 'Final', 'FinalState', 'History'].includes(data.model.kind)}
          {@const radius = ['Initial', 'Junction'].includes(data.model.kind) ? 48.5 : data.model.kind === 'History' ? 42 : 40}
          <circle cx={width / 2} cy={height / 2} r={width * (radius + 1.5) / 100 + 4.5} />
        {:else if ['Fork', 'Join'].includes(data.model.kind)}
          <rect x="-4.5" y="-4.5" width={width + 9} height={height + 9} />
        {:else}
          <path d={`M${width / 2} ${height * 0.04 - diamondOffset}
            L${width * 0.96 + diamondOffset} ${height / 2}
            L${width / 2} ${height * 0.96 + diamondOffset}
            L${width * 0.04 - diamondOffset} ${height / 2} Z`} />
        {/if}
      </svg>
    {/if}
    {#if data.model.label}<span class="marker-label breakpoint-site">
      {#if data.model.entryBreakpoint && data.allowBreakpoints}
        <span class="state-breakpoint breakpoint-slot"><BreakpointButton target={{ elementId: data.model.id, operation: 'state.entry' }}
          label={data.model.label} breakpoints={data.breakpoints} ontoggle={data.ontogglebreakpoint} /></span>
      {/if}{data.model.label}</span>{/if}
  {:else}
    <div class="heading" class:breakpoint-site={data.model.entryBreakpoint}>
      <span class="label">{#if data.model.entryBreakpoint && data.allowBreakpoints}<span class="state-breakpoint breakpoint-slot"><BreakpointButton target={{ elementId: data.model.id, operation: 'state.entry' }}
        label={data.model.label} breakpoints={data.breakpoints} ontoggle={data.ontogglebreakpoint} /></span>{/if}{data.model.label}{data.model.kind === 'Region' && data.collapsed ? ` (${data.model.hiddenCount} hidden elements)` : ''}</span>
      {#if data.model.container || (data.model.notation === 'stateMachine' && data.model.details?.length)}
        <button class="collapse-toggle nodrag nopan" onclick={(event) => { event.stopPropagation(); data.ontoggle(data.model.id); }}
          aria-expanded={!data.collapsed}
          aria-label={`${data.collapsed ? 'Expand' : 'Collapse'} ${data.model.label}`}>
          {data.collapsed ? '+' : '-'}
        </button>
      {/if}
    </div>
    {#if data.model.notation === 'stateMachine'}
      {#if !data.collapsed && data.model.details?.length}
        <div class="operations" class:collapsed={data.model.operationsCollapsed}>
          {#if data.model.operationsCollapsed}
          <div class="compartment-heading"><span>Events ({data.model.details.length})</span></div>
          {/if}
          <button class="collapse-toggle nodrag nopan"
            aria-label={`${data.model.operationsCollapsed ? 'Expand' : 'Collapse'} events of ${data.model.label}`}
            aria-expanded={!data.model.operationsCollapsed}
            onclick={(event) => { event.stopPropagation(); data.ontoggleoperations(data.model.id); }}>
            {data.model.operationsCollapsed ? '+' : '-'}
          </button>
          {#if !data.model.operationsCollapsed}
            <div class="operation-details">
              {#each data.model.operations ?? [] as operation (operation.id)}
                <div class="detail operation-row nodrag nopan" class:breakpoint-site={!operation.parts}
                  class:focused={data.focusedIds.has(operation.selectionId) || data.focusedIds.has(operation.target.elementId)}
                  class:filtered={data.hiddenEdges.has(operation.target.elementId)}
                  class:operation-selected={data.selectedOperation === operation.selectionId}
                  role="button" tabindex={data.hiddenEdges.has(operation.target.elementId) ? -1 : 0} aria-pressed={data.selectedOperation === operation.selectionId}
                  aria-label={`Select ${operation.target.operation} of ${data.model.label}`}
                  data-operation={operation.target.operation}
                  onclick={event => { event.stopPropagation(); data.onselectoperation(operation); }}
                  onkeydown={event => {
                    if (event.target === event.currentTarget && ['Enter', ' '].includes(event.key)) {
                      event.preventDefault(); event.stopPropagation(); data.onselectoperation(operation);
                    }
                  }}>
                  {#if operation.parts}
                    <OperationText parts={operation.parts} elementId={operation.target.elementId}
                      allowBreakpoints={data.allowBreakpoints} breakpoints={data.breakpoints} ontoggle={data.ontogglebreakpoint} />
                  {:else}
                    {#if operation.breakpointable !== false && data.allowBreakpoints}
                      <span class="breakpoint-slot"><BreakpointButton target={operation.target} label={data.model.label} breakpoints={data.breakpoints} ontoggle={data.ontogglebreakpoint} /></span>
                    {/if}
                    <span>{operation.label}</span>
                  {/if}
                  {#if operation.embeddedBehavior}
                    <BehaviorIcon behavior={operation.embeddedBehavior} elementId={data.model.id} onopen={data.onopenbehavior} />
                  {/if}
                </div>
              {/each}
            </div>
          {/if}
        </div>
      {/if}
      {#if data.model.hasSubstatesCompartment && !data.collapsed}
        <div class="substates">
          <div class="compartment-heading">
            {#if data.model.substatesCollapsed}<span>Substates ({data.model.hiddenCount} hidden elements)</span>{/if}
            <button class="collapse-toggle nodrag nopan"
              aria-label={`${data.model.substatesCollapsed ? 'Expand' : 'Collapse'} substates of ${data.model.label}`}
              aria-expanded={!data.model.substatesCollapsed}
              onclick={(event) => { event.stopPropagation(); data.ontogglesubstates(data.model.id); }}>
              {data.model.substatesCollapsed ? '+' : '-'}
            </button>
          </div>
        </div>
      {/if}
    {:else}
    <div class="kind">{data.model.kind}</div>
    {#if !data.model.container || data.collapsed}
      {#each (data.model.details ?? []).slice(0, 3) as detail}<div class="detail">{detail}</div>{/each}
    {/if}
    {/if}
    {#if data.model.hiddenCount && data.model.notation !== 'stateMachine'}<div class="detail">{data.model.hiddenCount} hidden elements</div>{/if}
  {/if}
  {#if badge}<div class="badge">{badge}</div>{/if}
</div>

<style>
  .shape {
    box-sizing: border-box; width: 100%; height: 100%; border: 1.5px solid var(--sfv-border, #52617b);
    border-radius: var(--sfv-radius, 10px); background: var(--sfv-node-bg, #152137);
    color: var(--sfv-text, #e2e8f0); font-family: var(--sfv-font, system-ui, sans-serif);
    font-size: 13px; position: relative; box-shadow: 0 3px 12px #0002;
  }
  .container { background: var(--sfv-container-bg, #101b2c); border-width: 2px; }
  .region { border-style: dashed; background: var(--sfv-region-bg, #142236); }
  .heading { display: flex; align-items: center; gap: 7px; padding: 10px 12px 3px; }
  .label { font-weight: 650; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; flex: 1; }
  .kind { color: var(--sfv-muted, #94a3b8); font-size: 10px; padding: 0 12px 5px; letter-spacing: .05em; }
  .detail { padding: 1px 12px; font-size: 11px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
  button { border: 1px solid var(--sfv-border, #52617b); border-radius: 4px; background: transparent; color: inherit; cursor: pointer; }
  .uml .heading, .compartment-heading { box-sizing: border-box; height: 38px; padding: 8px 12px; position: relative; }
  .uml .heading:has(.collapse-toggle) { padding-inline: 36px; }
  .compartment-heading, .uml.region .heading:has(.collapse-toggle) { padding-left: 12px; padding-right: 36px; }
  .uml .label { text-align: center; }
  .uml .operations { position: relative; min-height: 38px; box-sizing: border-box; border-top: 1px solid var(--sfv-border, #52617b); }
  .compartment-heading { font-size: 11px; line-height: 18px; display: flex; align-items: center; }
  .operation-details { padding: 10px 36px 10px 0; }
  .collapse-toggle { box-sizing: border-box; width: 22px; height: 22px; padding: 0; line-height: 20px; font-size: 13px; }
  .uml .collapse-toggle { position: absolute; right: 6px; top: 8px; }
  .uml .detail { line-height: 18px; padding: 0 12px; overflow: visible; text-overflow: clip; }
  .operation-row { cursor: pointer; min-height: 18px; }
  .breakpoint-slot { display: inline-block; width: 18px; height: 0; position: relative; }
  .state-breakpoint { z-index: 5; }
  .operation-selected { background: var(--sfv-operation-selection, #c4b5fd26); border-radius: 3px; }
  .operation-row:focus-visible { outline: 1px solid var(--sfv-selection, #c4b5fd); }
  .shape.focused { box-shadow: 0 0 0 2px var(--sfv-focus, #fbbf24), 0 0 14px var(--sfv-focus, #fbbf24); }
  .marker.focused { box-shadow: none; }
  .marker.focused > svg:first-of-type { filter: drop-shadow(0 0 4px var(--sfv-focus, #fbbf24)); }
  .operation-row.focused { background: var(--sfv-focus-bg, #fbbf2433); }
  .operation-row.filtered { visibility: hidden; pointer-events: none; }
  .uml .substates { border-top: 1px solid var(--sfv-border, #52617b); }
  .uml.container { border-width: 1.5px; }
  .uml.region { border: 0; border-top: 1px solid var(--sfv-border, #52617b); box-shadow: none; border-radius: 0; background: transparent; }
  .uml.region.region-separator { border-top: 1px dashed var(--sfv-border, #52617b); }
  .uml.region.region-column-separator { border-left: 1px dashed var(--sfv-border, #52617b); }
  .uml.region .label { text-align: left; font-weight: 400; font-size: 11px; }
  .uml.region .heading { height: 38px; }
  .selected { outline: 3px solid var(--sfv-selection, #c4b5fd); outline-offset: 3px; }
  .marker.selected { outline: none; }
  .marker.selected .selection-contour { position: absolute; left: -8px; top: -8px; width: calc(100% + 16px); height: calc(100% + 16px);
    fill: none; stroke: var(--sfv-selection, #c4b5fd); stroke-width: 3; pointer-events: none; }
  [data-status='active'], [data-status='running'], [data-status='entered'] { border-color: var(--sfv-active, #38bdf8); }
  [data-status='activated'], [data-status='executed'], [data-status='taken'] { border-color: var(--sfv-success, #34d399); }
  [data-status='waiting'], [data-status='guard-rejected'] { border-color: var(--sfv-waiting, #fbbf24); }
  [data-status='failed'] { border-color: var(--sfv-failed, #fb7185); }
  [data-status='exited'] { border-color: var(--sfv-exited, #a78bfa); border-style: dashed; }
  .badge { position: absolute; bottom: -12px; left: 8px; z-index: 4; border: 1px solid currentColor; border-radius: 5px;
    background: var(--sfv-node-bg, #152137); font-size: 10px; padding: 2px 6px; white-space: nowrap; }
  .marker { background: transparent; border: 0; border-radius: 0; box-shadow: none; }
  svg { width: 100%; height: 100%; overflow: visible; fill: var(--sfv-node-bg, #152137); stroke: var(--sfv-border, #94a3b8); stroke-width: 3; }
  .solid { fill: var(--sfv-text, #e2e8f0); }
  text { fill: var(--sfv-text, #e2e8f0); stroke: none; font-size: 46px; }
  .marker-label { position: absolute; top: 100%; left: 50%; transform: translateX(-50%); white-space: nowrap; font-size: 10px; }
  .marker[data-status] svg { stroke: var(--sfv-active, #38bdf8); }
  :global(.svelte-flow__handle) { opacity: 0; pointer-events: none; }
</style>

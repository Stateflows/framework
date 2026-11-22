<script lang="ts">
  import BreakpointButton from './BreakpointButton.svelte';
  import type { BreakpointTarget, LabelParts } from './model.js';
  let { parts, elementId, allowBreakpoints, breakpoints, ontoggle }: {
    parts: LabelParts; elementId: string; allowBreakpoints: boolean;
    breakpoints: readonly BreakpointTarget[]; ontoggle: (target: BreakpointTarget) => void;
  } = $props();
</script>

{#snippet trigger()}
  {#if parts.trigger}<span>{parts.trigger}</span>{:else if allowBreakpoints}<i>default</i>{/if}
{/snippet}

<span class="breakpoint-site transition-site">{#if allowBreakpoints}<span class="breakpoint-slot"><BreakpointButton target={{ elementId, operation: 'transition.guard' }} {breakpoints} {ontoggle} /></span>{/if}{@render trigger()}</span>{#if parts.guard}<span class="breakpoint-site guard-part">{parts.trigger || allowBreakpoints ? ' [' : '['}{#if allowBreakpoints}<span class="breakpoint-slot"><BreakpointButton target={{ elementId, operation: 'transition.guard' }} {breakpoints} {ontoggle} /></span>{/if}{parts.guard}]</span>{/if}{#if parts.effect}<span class="breakpoint-site effect-part">{parts.trigger || parts.guard || allowBreakpoints ? ' / ' : '/ '}{#if allowBreakpoints}<span class="breakpoint-slot"><BreakpointButton target={{ elementId, operation: 'transition.effect' }} {breakpoints} {ontoggle} /></span>{/if}{parts.effect}</span>{/if}

<style>
  .breakpoint-site { white-space: nowrap; }
  .breakpoint-slot { display: inline-block; width: 18px; height: 0; position: relative; }
</style>

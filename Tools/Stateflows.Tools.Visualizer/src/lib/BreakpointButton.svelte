<script lang="ts">
  import type { BreakpointTarget } from './model.js';
  let { target, label = target.elementId, breakpoints, ontoggle }: {
    target: BreakpointTarget;
    label?: string;
    breakpoints: readonly BreakpointTarget[];
    ontoggle: (target: BreakpointTarget) => void;
  } = $props();
  let enabled = $derived(breakpoints.some(item => item.elementId === target.elementId && item.operation === target.operation));
</script>

<button class="breakpoint nodrag nopan" class:enabled aria-pressed={enabled}
  aria-label={`${enabled ? 'Remove' : 'Add'} breakpoint ${label} ${target.operation}`}
  title={`${enabled ? 'Remove' : 'Add'} breakpoint: ${target.operation}`}
  onclick={event => { event.stopPropagation(); ontoggle(target); }}><span></span></button>

<style>
  .breakpoint { position: absolute; left: 0; top: -12px; display: inline-flex; align-items: center; justify-content: center;
    width: 18px; height: 18px; padding: 0; border: 0; background: transparent; cursor: pointer; flex: none; }
  span { width: 10px; height: 10px; background: var(--sfv-breakpoint, #ef4444); border-radius: 50%; opacity: 0; }
  .enabled span { opacity: 1; }
  .breakpoint:hover span, .breakpoint:focus-visible span, :global(.breakpoint-site:hover) .breakpoint:not(.enabled) span { opacity: .45; }
  .enabled:hover span { opacity: 1; }
</style>

<script lang="ts">
  import type { EmbeddedBehavior } from './model.js';
  let { behavior, elementId, onopen }: {
    behavior: EmbeddedBehavior; elementId: string; onopen: (behavior: EmbeddedBehavior, elementId: string) => void;
  } = $props();
</script>

<button class="embedded-behavior nodrag nopan"
  aria-label={`Open ${behavior.type} ${behavior.name}`} title={`Open ${behavior.type}: ${behavior.name}`}
  onclick={event => { event.stopPropagation(); onopen(behavior, elementId); }}>
  <svg viewBox="0 0 24 24" aria-hidden="true">
    {#if behavior.type === 'Activity'}
      <circle cx="4" cy="12" r="2" /><path d="M6 12 H9 M15 12 H20 M18 10 L20 12 L18 14" />
      <rect x="9" y="7" width="6" height="10" rx="2" />
    {:else if behavior.type === 'StateMachine'}
      <rect x="2" y="3" width="20" height="18" rx="3" /><path d="M5 8 H19 M8 16 H16" />
      <rect x="5" y="13" width="4" height="5" rx="1" /><rect x="15" y="13" width="4" height="5" rx="1" />
    {:else}
      <path d="M9 5 L18 12 L9 19 Z" />
    {/if}
  </svg>
</button>

<style>
  button { position: relative; z-index: 1; display: inline-flex; vertical-align: middle; align-items: center; justify-content: center;
    width: 18px; height: 18px; padding: 0; margin-left: 4px; border: 0; background: transparent; color: inherit; cursor: pointer; }
  svg { width: 16px; height: 16px; fill: none; stroke: currentColor; stroke-width: 1.5; }
</style>

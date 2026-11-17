<script lang="ts">
  import { onMount } from 'svelte';
  import { Visualizer, fromStateMachine, fromActivity, fromAction, type DiagramDocument, type DiagramMode,
    type DiagramOverlay, type DiagramPresentation, type StateMachineDefinition, type ActivityDefinition, type ActionDefinition, type EmbeddedBehavior, type BreakpointTarget } from '../src/lib/index.js';
  import { syntheticFixtures } from './fixtures.js';
  import { frame, matches, type Observation } from './capture.js';

  let documents = $state<DiagramDocument[]>(syntheticFixtures.map(fixture => fixture.document));
  let selectedDocument = $state(syntheticFixtures[0].document.id);
  let mode = $state<DiagramMode>('definition');
  let records = $state<Observation[]>([]);
  let cursor = $state<number>();
  let selectedId = $state<string>();
  let instance = $state(`demo-${Date.now()}`);
  let failure = $state('');
  let connection = $state('Connecting');
  let commandStatus = $state('');
  let commandBusy = $state(false);
  let debugEnabled = $state(false);
  let breakpoints = $state<BreakpointTarget[]>([]);
  let pauses = $state<{ id: string; target: BreakpointTarget; at: string; eventId: string | null }[]>([]);
  let debugConnection = $state('');
  let resumeBusy = $state(false);
  let modelChanged = false;
  let layoutCount = $state(0);
  let layoutTime = $state(0);
  let activeIds = $state<string[]>([]);
  let presentation = $state<DiagramPresentation>({ stateOrdering: 'PREFER_DEFINITION' });
  let document = $derived(documents.find(model => model.id === selectedDocument));
  let fixture = $derived(syntheticFixtures.find(item => item.document.id === selectedDocument));
  let relevant = $derived(document ? records.filter(record => matches(record, document!, instance)) : []);
  let visibleRecords = $derived(cursor === undefined ? relevant : relevant.filter(record => record.sequence <= cursor!));
  let overlay = $derived.by((): DiagramOverlay | undefined => {
    if (!document || fixture) return;
    const result = frame(document, instance, visibleRecords);
    if (mode === 'live' && document.family === 'stateMachine') {
      const nodes = new Set(document.nodes.map(node => node.id));
      result.elements = [
        ...result.elements.filter(item => !nodes.has(item.elementId) || item.status === 'running' || item.status === 'failed'),
        ...activeIds.map(elementId => ({ elementId, status: 'active' as const }))
      ];
    }
    return result;
  });
  let selectedOperation = $derived(document?.nodes.flatMap(node => node.operations ?? []).find(operation => operation.selectionId === selectedId));
  let selectedElement = $derived(selectedOperation
    ? document?.nodes.flatMap(node => node.operations ?? []).filter(operation => operation.selectionId === selectedId)
    : document?.nodes.find(node => node.id === selectedId) ?? document?.edges.find(edge => edge.id === selectedId));
  let elementRecords = $derived(selectedOperation
    ? relevant.filter(record => record.elementId === selectedOperation.target.elementId && record.operation === selectedOperation.target.operation)
    : selectedId ? relevant.filter(record => record.elementId === selectedId) : relevant);

  $effect(() => {
    breakpoints = []; pauses = [];
    if (!document || fixture || document.family !== 'stateMachine') { debugConnection = ''; return; }
    const params = new URLSearchParams({ family: document.family, name: document.name, version: String(document.version), instance });
    const source = new EventSource(`/api/debug/stream?${params}`);
    source.onopen = () => { debugConnection = 'Debugger connected'; };
    source.onerror = () => { debugConnection = 'Debugger disconnected - reconnecting'; };
    source.onmessage = event => {
      try {
        const snapshot: { breakpoints: BreakpointTarget[]; pauses: typeof pauses } = JSON.parse(event.data);
        if (!Array.isArray(snapshot.breakpoints) || !Array.isArray(snapshot.pauses)) throw new Error('Invalid debugger snapshot.');
        breakpoints = snapshot.breakpoints;
        pauses = snapshot.pauses;
      } catch (error) { failure = `Debugger decoding failed: ${error instanceof Error ? error.message : String(error)}`; }
    };
    return () => source.close();
  });

  async function changeBreakpoint(target: BreakpointTarget, enabled: boolean) {
    if (!document) return;
    const scope = { family: document.family, name: document.name, version: document.version, instance };
    try {
      const response = await fetch('/api/debug/breakpoints', { method: 'POST', headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ scope, target, enabled }) });
      if (!response.ok) throw new Error(`Breakpoint update failed: HTTP ${response.status}: ${await response.text()}`);
      const snapshot: { breakpoints: BreakpointTarget[] } = await response.json();
      if (document.name === scope.name && document.version === scope.version && instance === scope.instance)
        breakpoints = snapshot.breakpoints;
    } catch (error) { failure = error instanceof Error ? error.message : String(error); }
  }
  async function resume(pauseId: string) {
    resumeBusy = true;
    try {
      const response = await fetch(`/api/debug/resume/${pauseId}`, { method: 'POST' });
      if (!response.ok) throw new Error(`Resume failed: HTTP ${response.status}: ${await response.text()}`);
      pauses = pauses.filter(pause => pause.id !== pauseId);
    } catch (error) { failure = error instanceof Error ? error.message : String(error); }
    finally { resumeBusy = false; }
  }

  onMount(() => {
    let disposed = false;
    let source: EventSource | undefined;
    async function load() {
      try {
        const response = await fetch('/api/definitions');
        if (!response.ok) throw new Error(`Definition request failed: HTTP ${response.status}`);
        const definitions: { machines: StateMachineDefinition[]; activities: ActivityDefinition[]; actions?: ActionDefinition[] } = await response.json();
        if (disposed) return;
        documents = [
          ...definitions.machines.map(fromStateMachine), ...definitions.activities.map(fromActivity),
          ...(definitions.actions ?? []).map(fromAction),
          ...syntheticFixtures.map(fixture => fixture.document)
        ];
        if (!modelChanged && definitions.machines.length) {
          selectedDocument = fromStateMachine(definitions.machines[0]).id;
          changeDocument();
        }
        source = new EventSource('/api/capture/stream');
        source.onopen = () => { connection = 'Connected'; };
        source.onerror = () => { connection = 'Disconnected - reconnecting; history may be incomplete'; };
        source.onmessage = event => {
          try {
            const batch: { latest: number; oldest: number; records: Observation[] } = JSON.parse(event.data);
            const last = records.at(-1)?.sequence ?? 0;
            if (batch.oldest > last + 1) failure = 'Capture gap: older observations have expired.';
            const combined = new Map(records.map(record => [record.sequence, record]));
            for (const record of batch.records) combined.set(record.sequence, record);
            records = [...combined.values()].sort((a, b) => a.sequence - b.sequence).slice(-4096);
          } catch (error) { failure = `Capture decoding failed: ${error instanceof Error ? error.message : String(error)}`; }
        };
      } catch (error) {
        connection = 'Backend unavailable - synthetic fixtures remain usable';
        failure = error instanceof Error ? error.message : String(error);
      }
    }
    void load();
    return () => { disposed = true; source?.close(); };
  });

  function changeDocument() {
    selectedId = undefined; cursor = undefined; activeIds = []; commandStatus = ''; failure = '';
    presentation = { ...presentation, collapsedIds: [], collapsedOperationIds: [], collapsedSubstateIds: [] };
  }
  function newInstance() {
    instance = `demo-${crypto.randomUUID()}`;
    selectedId = undefined; cursor = undefined; activeIds = []; commandStatus = ''; failure = '';
  }
  function openBehavior(behavior: EmbeddedBehavior) {
    const family = behavior.type === 'StateMachine' ? 'stateMachine' : behavior.type === 'Activity' ? 'activity'
      : behavior.type === 'Action' ? 'action' : undefined;
    const target = documents.filter(model => model.family === family && model.name === behavior.name &&
      (behavior.version === undefined || model.version === behavior.version)).sort((a, b) => b.version - a.version)[0];
    if (!target) {
      failure = `Definition unavailable for embedded ${behavior.type} '${behavior.name}'.`;
      return;
    }
    modelChanged = true;
    selectedDocument = target.id;
    changeDocument();
  }
  async function command(name: string) {
    if (!document) return;
    commandBusy = true; failure = '';
    mode = 'live'; cursor = undefined; selectedId = undefined;
    try {
      const path = document.family === 'stateMachine'
        ? `/api/demo/machine/${encodeURIComponent(instance)}/${name}`
        : document.name === 'Telemetry'
        ? `/api/demo/telemetry/${encodeURIComponent(instance)}/${name}`
        : `/api/demo/activity/${encodeURIComponent(instance)}`;
      const response = await fetch(path, { method: 'POST' });
      if (!response.ok) throw new Error(`Command failed: HTTP ${response.status}: ${await response.text()}`);
      const result = await response.json();
      commandStatus = String(result.status);
      if (result.activeIds) activeIds = result.activeIds;
      if (!['Initialized', 'Consumed', 'Accepted', 'Duplicated'].includes(result.status))
        failure = `Command was not executed: ${result.status}`;
    } catch (error) { failure = error instanceof Error ? error.message : String(error); }
    finally { commandBusy = false; }
  }
</script>

<svelte:head><title>Stateflows visualizer laboratory</title></svelte:head>

<div class="workspace">
  <header>
    <div><h1>Stateflows <span>visualizer laboratory</span></h1><p>Reusable diagram component / separate inspection harness</p></div>
    <span class="connection">{connection}</span>
  </header>
  <div class="commands">
    <label>Model <select disabled={commandBusy} bind:value={selectedDocument} onchange={() => { modelChanged = true; changeDocument(); }}>
      {#each documents as model}<option value={model.id}>{model.name} v{model.version}</option>{/each}
    </select></label>
    <label>Instance <input disabled={commandBusy} bind:value={instance} onchange={() => { activeIds = []; cursor = undefined; commandStatus = ''; }} /></label>
    <button disabled={commandBusy} onclick={newInstance}>New instance</button>
    {#if document?.name === 'ReviewProcess'}
      {#each ['initialize', 'approve', 'reject', 'finish', 'cancel', 'synchronize', 'resume', 'refresh'] as name}
        <button disabled={commandBusy} onclick={() => command(name)}>{name}</button>
      {/each}
    {:else if document?.name === 'NestedCalculation'}
      <button disabled={commandBusy} onclick={() => command('initialize')}>Run calculation</button>
    {:else if document?.name === 'Telemetry'}
      {#each ['initialize', 'configure', 'samples'] as name}
        <button disabled={commandBusy} onclick={() => command(name)}>{name === 'samples' ? 'Send samples' : name}</button>
      {/each}
    {/if}
    <span role="status" data-testid="command-status">{commandBusy ? 'Executing...' : commandStatus}</span>
    <button onclick={() => {
      selectedId = undefined;
      presentation = { ...presentation, collapsedIds: document?.nodes.filter(node => !node.parentId).map(node => node.id) ?? [] };
    }}>Collapse top-level scopes</button>
    <button onclick={() => { presentation = { ...presentation, collapsedIds: [], collapsedOperationIds: [], collapsedSubstateIds: [] }; }}>Expand all scopes</button>
  </div>
  {#if failure}<div class="error" role="alert">{failure}</div>{/if}
  {#if pauses.length}
    <div class="debug-pauses" role="status">
      {#each pauses as pause (pause.id)}
        <div data-testid="debug-pause">Paused before {document?.nodes.find(node => node.id === pause.target.elementId)?.label ?? pause.target.elementId} · {pause.target.operation}
          <button disabled={resumeBusy} onclick={() => resume(pause.id)}>Resume</button>
        </div>
      {/each}
    </div>
  {/if}
  {#if fixture}<p class="fixture-note" data-testid="fixture-description">Synthetic definition only: {fixture.description} No runtime execution is simulated.</p>{/if}
  <main>
    <section class="diagram">
      {#if document}
        <Visualizer {document} {mode} {overlay} {selectedId} {presentation}
          allowBreakpoints={debugEnabled && !fixture && document.family === 'stateMachine'}
          {breakpoints} onbreakpointchange={changeBreakpoint}
          onopenbehavior={openBehavior}
          onpresentationchange={value => { presentation = value; }}
          onselect={selection => { selectedId = selection?.elementId; }}
          onlayout={layout => { layoutCount++; layoutTime = layout.durationMs; }}
          onerror={error => { failure = error.message; }}>
          {#snippet settings()}
           <label>Mode <select bind:value={mode}><option>definition</option><option>live</option><option>execution</option></select></label>
           <label>Flow direction <select value={presentation.direction ?? 'RIGHT'}
             onchange={event => { presentation = { ...presentation, direction: event.currentTarget.value as DiagramPresentation['direction'] }; }}>
             <option value="RIGHT">Horizontal</option><option value="DOWN">Vertical</option>
           </select></label>
           <label>Transition routing <select value={presentation.routing ?? 'ORTHOGONAL'}
             onchange={event => { presentation = { ...presentation, routing: event.currentTarget.value as DiagramPresentation['routing'] }; }}>
             <option value="ORTHOGONAL">Orthogonal</option><option value="POLYLINE">Polyline</option><option value="SPLINES">Spline</option>
           </select></label>
           <label>State ordering <select value={presentation.stateOrdering ?? 'PREFER_DEFINITION'}
             onchange={event => { presentation = { ...presentation, stateOrdering: event.currentTarget.value as DiagramPresentation['stateOrdering'] }; }}>
             <option value="AUTOMATIC">Automatic</option>
             <option value="PREFER_DEFINITION">Prefer definition order</option>
             <option value="PRESERVE_DEFINITION">Preserve definition order</option>
           </select></label>
           <label class="debug-option"><input type="checkbox" bind:checked={debugEnabled} />Enable breakpoints</label>
          {/snippet}
        </Visualizer>
      {:else}<p>Loading visitor-backed definitions...</p>{/if}
    </section>
    <aside>
      <h2>Selection</h2>
      <p class="hint">Click a diagram element or an observation below.</p>
      <pre>{selectedElement ? JSON.stringify(selectedElement, null, 2) : 'Nothing selected'}</pre>
      <h2>Execution capture</h2>
      {#if debugConnection}<p>{debugConnection}</p>{/if}
      <p data-testid="observation-count">{relevant.length} observations for this model / instance</p>
      <p class="hint">Observed operations, not commit receipts. Bounded to 4096 records. No token payloads captured.</p>
      <button onclick={() => { cursor = undefined; selectedId = undefined; }}>Latest observations</button>
      <div class="timeline">
        {#each [...elementRecords].reverse().slice(0, 200) as record (record.sequence)}
          <button class:chosen={cursor === record.sequence} onclick={() => {
            mode = 'execution'; cursor = record.sequence; selectedId = record.elementId;
          }}>
            <span>#{record.sequence} {record.operation}</span>
            <strong>{record.status} / {record.stage}</strong>
            <small title={record.elementId}>{document?.nodes.find(node => node.id === record.elementId)?.label ?? record.elementId}</small>
          </button>
        {/each}
      </div>
    </aside>
  </main>
  <footer>
    <span>{document?.nodes.length ?? 0} nodes / {document?.edges.length ?? 0} edges</span>
    <span data-testid="layout-count">Layouts: {layoutCount}</span><span>{layoutTime.toFixed(0)} ms last layout (worker)</span>
    <span>{fixture ? 'Synthetic fixture / no runtime observations' : 'Registered definition'}</span>
  </footer>
</div>

<style>
  :global(body) { margin: 0; background: #080f1c; color: #e2e8f0; font: 13px system-ui, sans-serif; }
  :global(*) { box-sizing: border-box; }
  .workspace { height: 100vh; display: flex; flex-direction: column; }
  header { display: flex; justify-content: space-between; align-items: center; padding: 16px 22px; border-bottom: 1px solid #25324a; }
  h1 { margin: 0; font-size: 19px; letter-spacing: -.03em; } h1 span { color: #94a3b8; font-weight: 400; }
  p { color: #94a3b8; margin: 4px 0; } .connection { font-size: 11px; color: #7dd3fc; }
  .commands { display: flex; flex-wrap: wrap; align-items: end; gap: 8px; padding: 12px 20px; }
  label { display: flex; flex-direction: column; gap: 4px; font-size: 10px; text-transform: uppercase; color: #94a3b8; }
  input, select, button { font: inherit; background: #152137; color: #e2e8f0; border: 1px solid #334155; border-radius: 5px; padding: 6px 9px; }
  button { cursor: pointer; } button:disabled { opacity: .5; cursor: wait; }
  main { display: flex; flex: 1; min-height: 0; border-block: 1px solid #25324a; }
  .diagram { flex: 1; min-width: 0; } aside { width: 310px; padding: 14px; overflow: auto; border-left: 1px solid #25324a; }
  h2 { font-size: 12px; text-transform: uppercase; letter-spacing: .08em; margin: 12px 0 8px; }
  pre { font-size: 10px; white-space: pre-wrap; overflow-wrap: anywhere; max-height: 220px; overflow: auto; color: #c4b5fd; }
  .hint { font-size: 11px; line-height: 1.5; margin-bottom: 10px; }
  .timeline { margin-top: 10px; } .timeline button { display: flex; flex-direction: column; text-align: left; width: 100%; margin-bottom: 5px; font-size: 10px; gap: 4px; }
  small { overflow-wrap: anywhere; color: #94a3b8; } .chosen { border-color: #c4b5fd; }
  footer { padding: 8px 20px; display: flex; gap: 20px; font-size: 10px; color: #94a3b8; }
  .error { padding: 10px 20px; background: #651a2d; color: white; }
  .fixture-note { padding: 8px 20px; font-size: 11px; }
  .debug-option { flex-direction: row; align-items: center; }
  .debug-pauses { padding: 8px 20px; background: #651a2d; }
  .debug-pauses button { margin-left: 12px; }
</style>

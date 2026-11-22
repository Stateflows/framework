import { expect, test } from '@playwright/test';

let vertexIds: Map<string, string>;
test.beforeEach(async ({ request }) => {
  const definitions = await (await request.get('/api/definitions')).json();
  const machine = definitions.machines.find((item: { name: string }) => item.name === 'ReviewProcess');
  vertexIds = new Map(machine.states.map((state: { id: string; name: string }) => [state.name, state.id]));
});
function vertexId(name: string): string {
  const id = vertexIds.get(name);
  if (id === undefined) throw new Error(`Unknown ReviewProcess state '${name}'.`);
  return id;
}

test('search focuses a subset without selecting or rearranging it', async ({ page }) => {
  await page.goto('/');
  await expect(page.locator('.canvas')).toHaveAttribute('data-layout-ready', 'true');
  await expect(page.locator('.svelte-flow__edge-path').first()).toBeAttached();
  const entry = page.locator(`.shape[data-element-id="${vertexId('Review')}"] .operation-row[data-operation="state.entry"]`).first();
  await entry.press('Enter');
  const selection = await page.locator('aside pre').innerText();
  const layouts = await page.getByTestId('layout-count').innerText();
  await page.getByRole('textbox', { name: 'Find diagram element' }).fill('Pending');
  await page.getByRole('button', { name: 'Find', exact: true }).click();
  await expect(page.locator('.shape.focused')).toHaveCount(2);
  await expect(page.locator('.shape.focused.selected')).toHaveCount(0);
  await expect(page.locator('aside pre')).toHaveText(selection);
  await expect(page.getByTestId('layout-count')).toHaveText(layouts);
  await page.getByRole('button', { name: 'Clear focus', exact: true }).click();
  await expect(page.locator('.focused')).toHaveCount(0);
  await page.getByRole('textbox', { name: 'Find diagram element' }).fill('Refresh');
  await page.getByRole('button', { name: 'Find', exact: true }).click();
  await expect(page.locator('.operation-row.focused')).toHaveCount(1);
  await expect(page.locator('aside pre')).toHaveText(selection);
  await page.getByRole('textbox', { name: 'Find diagram element' }).fill('does-not-exist');
  await page.getByRole('button', { name: 'Find', exact: true }).click();
  await expect(page.getByText('0 matches', { exact: true })).toBeVisible();
});

test('trigger filtering hides routes and internal rows without changing layout or geometry', async ({ page, request }) => {
  await page.goto('/');
  await expect(page.locator('.canvas')).toHaveAttribute('data-layout-ready', 'true');
  await expect(page.locator('.svelte-flow__edge-path').first()).toBeAttached();
  const layouts = await page.getByTestId('layout-count').innerText();
  const geometry = () => page.locator('.shape').evaluateAll(nodes => nodes.map(node => ({
    id: node.getAttribute('data-element-id'), position: node.parentElement!.getAttribute('style')
  })));
  const before = await geometry();
  const routes = await page.locator('.svelte-flow__edge-path').evaluateAll(paths => paths.map(path => path.getAttribute('d')));
  const definitions = await (await request.get('/api/definitions')).json();
  const machine = definitions.machines.find((item: { name: string }) => item.name === 'ReviewProcess');
  await page.locator('.trigger-filter summary').click();
  await page.locator('.trigger-options').getByRole('checkbox', { name: 'Approve', exact: true }).uncheck();
  const visibleIds = await page.locator('.svelte-flow__edge').evaluateAll(edges => edges.map(edge => edge.getAttribute('data-id')));
  for (const edge of machine.transitions.filter((edge: { triggers: string[] }) => edge.triggers.includes('Approve'))) {
    expect(visibleIds).not.toContain(edge.id);
  }
  await expect(page.getByTestId('layout-count')).toHaveText(layouts);
  expect(await geometry()).toEqual(before);
  const internal = page.locator('.operation-row').filter({ hasText: 'Refresh' });
  const height = await internal.evaluate(row => row.getBoundingClientRect().height);
  await page.locator('.trigger-options').getByRole('checkbox', { name: 'Refresh', exact: true }).uncheck();
  await expect(internal).toHaveCSS('visibility', 'hidden');
  expect(await internal.evaluate(row => row.getBoundingClientRect().height)).toBe(height);
  await page.locator('.trigger-options').getByRole('checkbox', { name: 'default', exact: true }).uncheck();
  await expect(page.locator('.shape[data-kind="Initial"]')).toHaveCount(4);
  await page.getByRole('button', { name: 'Show all triggers', exact: true }).click();
  await expect(internal).toHaveCSS('visibility', 'visible');
  expect(await page.locator('.svelte-flow__edge-path').evaluateAll(paths => paths.map(path => path.getAttribute('d')))).toEqual(routes);
  expect(await geometry()).toEqual(before);
  await expect(page.getByTestId('layout-count')).toHaveText(layouts);
});

test('toolbar settings preserve their values across model switches', async ({ page }) => {
  await page.goto('/');
  await expect(page.locator('.canvas')).toHaveAttribute('data-layout-ready', 'true');
  const toolbar = page.locator('.visualizer .toolbar');
  await expect(toolbar.getByRole('textbox', { name: 'Find diagram element' })).toBeVisible();
  for (const ordering of ['AUTOMATIC', 'PREFER_DEFINITION', 'PRESERVE_DEFINITION']) {
    await toolbar.getByRole('combobox', { name: 'State ordering', exact: true }).selectOption(ordering);
    await expect(page.locator('.canvas')).toHaveAttribute('data-layout-ready', 'true');
    await expect(page.locator('.workspace > [role="alert"]')).toHaveCount(0);
  }
  await toolbar.getByRole('combobox', { name: 'Flow direction', exact: true }).selectOption('DOWN');
  await toolbar.getByRole('combobox', { name: 'Transition routing', exact: true }).selectOption('SPLINES');
  await toolbar.getByRole('combobox', { name: 'Mode', exact: true }).selectOption('execution');
  await toolbar.getByRole('checkbox', { name: 'Enable breakpoints' }).check();
  for (const model of ['NestedCalculation v1', 'ReviewProcess v1']) {
    await page.getByRole('combobox', { name: 'Model', exact: true }).selectOption({ label: model });
    await expect(page.locator('.canvas')).toHaveAttribute('data-layout-ready', 'true');
    await expect(toolbar.getByRole('combobox', { name: 'State ordering', exact: true })).toHaveValue('PRESERVE_DEFINITION');
    await expect(toolbar.getByRole('combobox', { name: 'Flow direction', exact: true })).toHaveValue('DOWN');
    await expect(toolbar.getByRole('combobox', { name: 'Transition routing', exact: true })).toHaveValue('SPLINES');
    await expect(toolbar.getByRole('combobox', { name: 'Mode', exact: true })).toHaveValue('execution');
    await expect(toolbar.getByRole('checkbox', { name: 'Enable breakpoints' })).toBeChecked();
  }
  await expect(page.locator('.workspace > [role="alert"]')).toHaveCount(0);
});

test('flow direction switches independently of routing and rotates fork and join bars', async ({ page }) => {
  await page.goto('/');
  await expect(page.locator('.canvas')).toHaveAttribute('data-layout-ready', 'true');
  await page.getByRole('combobox', { name: 'Transition routing', exact: true }).selectOption('SPLINES');
  for (const direction of ['DOWN', 'RIGHT']) {
    const count = await page.getByTestId('layout-count').innerText();
    await page.getByRole('combobox', { name: 'Flow direction', exact: true }).selectOption(direction);
    await expect(page.getByTestId('layout-count')).not.toHaveText(count);
    await expect(page.locator('.canvas')).toHaveAttribute('data-layout-ready', 'true');
    await expect(page.getByRole('combobox', { name: 'Transition routing', exact: true })).toHaveValue('SPLINES');
    for (const kind of ['Fork', 'Join']) {
      const bar = page.locator(`.shape[data-kind="${kind}"]`);
      await expect.poll(async () => {
        const bounds = (await bar.boundingBox())!;
        return direction === 'DOWN' ? bounds.width > bounds.height : bounds.height > bounds.width;
      }).toBe(true);
    }
    await page.getByRole('button', { name: 'Collapse top-level scopes', exact: true }).click();
    await expect(page.locator('.canvas')).toHaveAttribute('data-layout-ready', 'true');
    await page.getByRole('button', { name: 'Expand all scopes', exact: true }).click();
    await expect(page.locator('.canvas')).toHaveAttribute('data-layout-ready', 'true');
    await expect(page.getByRole('combobox', { name: 'Flow direction', exact: true })).toHaveValue(direction);
  }
  await expect(page.locator('.workspace > [role="alert"]')).toHaveCount(0);
});

test('routing selector changes real transition routes and preserves collapse controls', async ({ page }) => {
  const errors: string[] = [];
  page.on('pageerror', error => errors.push(error.message));
  await page.goto('/');
  await expect(page.locator('.canvas')).toHaveAttribute('data-layout-ready', 'true');
  await expect.poll(() => page.locator('.svelte-flow__edge-path').count()).toBeGreaterThan(10);
  for (const routing of ['POLYLINE', 'SPLINES', 'ORTHOGONAL']) {
    const count = await page.getByTestId('layout-count').innerText();
    await page.getByRole('combobox', { name: 'Transition routing', exact: true }).selectOption(routing);
    await expect(page.getByTestId('layout-count')).not.toHaveText(count);
    await expect(page.locator('.canvas')).toHaveAttribute('data-layout-ready', 'true');
    const paths = await page.locator('.svelte-flow__edge-path').evaluateAll(paths => paths.map(path => path.getAttribute('d')!));
    expect(paths.every(path => path.startsWith('M ') && !/NaN|undefined/.test(path))).toBe(true);
    if (routing === 'POLYLINE') expect(paths.every(path => !/[QC]/.test(path))).toBe(true);
    if (routing === 'SPLINES') await expect.poll(() => page.locator('.svelte-flow__edge-path').evaluateAll(paths =>
      paths.some(path => path.getAttribute('d')?.includes(' C ')))).toBe(true);
    if (routing === 'ORTHOGONAL') expect(paths.some(path => path.includes(' Q '))).toBe(true);
    const review = page.locator(`.shape[data-element-id="${vertexId('Review')}"]`);
    await review.getByRole('button', { name: 'Collapse Review', exact: true }).click({ force: true });
    await expect(page.locator('.canvas')).toHaveAttribute('data-layout-ready', 'true');
    await expect(page.getByRole('combobox', { name: 'Transition routing', exact: true })).toHaveValue(routing);
    await review.getByRole('button', { name: 'Expand Review', exact: true }).click({ force: true });
    await expect(page.locator('.canvas')).toHaveAttribute('data-layout-ready', 'true');
    await page.getByRole('button', { name: 'Collapse top-level scopes', exact: true }).click();
    await expect(page.locator('.canvas')).toHaveAttribute('data-layout-ready', 'true');
    await expect(page.getByRole('combobox', { name: 'Transition routing', exact: true })).toHaveValue(routing);
    await page.getByRole('button', { name: 'Expand all scopes', exact: true }).click();
    await expect(page.locator('.canvas')).toHaveAttribute('data-layout-ready', 'true');
  }
  expect(errors).toEqual([]);
  await expect(page.locator('.workspace > [role="alert"]')).toHaveCount(0);
});

test('routing modes also render nested activity flows', async ({ page }) => {
  const errors: string[] = [];
  page.on('pageerror', error => errors.push(error.message));
  await page.goto('/');
  await page.getByRole('combobox', { name: 'Model', exact: true }).selectOption({ label: 'NestedCalculation v1' });
  await expect(page.locator('.visualizer .toolbar strong')).toHaveText('NestedCalculation');
  await expect(page.locator('.canvas')).toHaveAttribute('data-layout-ready', 'true');
  for (const routing of ['POLYLINE', 'SPLINES', 'ORTHOGONAL']) {
    const count = await page.getByTestId('layout-count').innerText();
    await page.getByRole('combobox', { name: 'Transition routing', exact: true }).selectOption(routing);
    await expect(page.getByTestId('layout-count')).not.toHaveText(count);
    await expect(page.locator('.canvas')).toHaveAttribute('data-layout-ready', 'true');
    await expect.poll(() => page.locator('.svelte-flow__edge-path').count()).toBeGreaterThan(0);
  }
  expect(errors).toEqual([]);
  await expect(page.locator('.workspace > [role="alert"]')).toHaveCount(0);
});

test('StockReady entry failure is captured and visualized', async ({ page, request }) => {
  await page.goto('/');
  await expect(page.getByText('Connected', { exact: true })).toBeVisible();
  await page.getByRole('button', { name: 'New instance', exact: true }).click();
  const instance = await page.getByRole('textbox', { name: 'Instance', exact: true }).inputValue();
  await page.getByRole('button', { name: 'initialize', exact: true }).click();
  await expect(page.getByTestId('command-status')).toHaveText('Initialized');
  await page.getByRole('button', { name: 'approve', exact: true }).click();
  await expect(page.locator(`.shape[data-element-id="${vertexId('StockPending')}"]`)).toHaveAttribute('data-status', 'active');
  await page.getByRole('button', { name: 'approve', exact: true }).click();
  await expect(page.locator(`.shape[data-element-id="${vertexId('StockReady')}"]`)).toHaveAttribute('data-status', 'failed');
  const capture = await (await request.get('/api/capture')).json();
  expect(capture.records.some((record: { instance: string; elementId: string; operation: string; stage: string }) =>
    record.instance === instance && record.elementId === vertexId('StockReady')
    && record.operation === 'state.entry' && record.stage === 'failed')).toBe(true);
});

test('all registered transitions expose guard breakpoints with stable text baselines and no disabled gutters', async ({ page, request }) => {
  await page.goto('/');
  await expect(page.locator('.canvas')).toHaveAttribute('data-layout-ready', 'true');
  const definitions = await (await request.get('/api/definitions')).json();
  const machine = definitions.machines.find((item: { name: string }) => item.name === 'ReviewProcess');
  const measure = () => page.locator('.operation-details, .edge-label, .heading .label').evaluateAll(elements =>
    elements.flatMap(element => {
      const walker = document.createTreeWalker(element, NodeFilter.SHOW_TEXT);
      const positions: { text: string; top: number; left: number; height: number }[] = [];
      while (walker.nextNode()) {
        if (!walker.currentNode.textContent?.trim() || walker.currentNode.parentElement?.closest('i')) continue;
        const range = document.createRange();
        range.selectNode(walker.currentNode);
        const box = range.getBoundingClientRect();
        positions.push({ text: walker.currentNode.textContent.trim(), top: box.top, left: 0, height: box.height });
      }
      return positions;
    }));
  await expect(page.locator('.edge-label')).toHaveCount(machine.transitions.filter((edge: { targetId: string | null; triggers: string[]; guards: number; effects: number; isElse: boolean }) =>
    edge.targetId !== null && (edge.triggers.some(trigger => !['', 'Completion', 'Stateflows.Common.Completion', 'Stateflows.StateMachines.Completion'].includes(trigger)) || edge.guards || edge.effects || edge.isElse)).length);
  let previous = '';
  await expect.poll(async () => {
    const current = JSON.stringify(await measure());
    const settled = previous === current;
    previous = current;
    return settled;
  }).toBe(true);
  const before = await measure();
  await expect(page.locator('.breakpoint-slot')).toHaveCount(0);
  const layouts = await page.getByTestId('layout-count').innerText();
  await page.getByRole('checkbox', { name: 'Enable breakpoints' }).check();
  await expect(page.locator('.shape[data-kind="Initial"] .breakpoint')).toHaveCount(0);
  for (const edge of machine.transitions as { id: string }[]) {
    await expect(page.locator('.transition-site').getByRole('button', { name: `Add breakpoint ${edge.id} transition.guard`, exact: true })).toHaveCount(1);
  }
  expect(await measure()).toEqual(before);
  await expect(page.getByTestId('layout-count')).toHaveText(layouts);
  await page.getByRole('checkbox', { name: 'Enable breakpoints' }).uncheck();
  await expect(page.locator('.breakpoint-slot')).toHaveCount(0);
  expect(await measure()).toEqual(before);
});

test('default transitions show italic default only in breakpoint mode', async ({ page, request }) => {
  await page.goto('/');
  await expect(page.locator('.canvas')).toHaveAttribute('data-layout-ready', 'true');
  const definitions = await (await request.get('/api/definitions')).json();
  const machine = definitions.machines.find((item: { name: string }) => item.name === 'ReviewProcess');
  const defaults = machine.transitions.filter((edge: { triggers: string[] }) =>
    edge.triggers.every(trigger => ['', 'Completion', 'Stateflows.Common.Completion', 'Stateflows.StateMachines.Completion'].includes(trigger)));
  expect(defaults.length).toBeGreaterThan(0);
  await expect(page.locator('.edge-label i')).toHaveCount(0);
  await page.getByRole('checkbox', { name: 'Enable breakpoints' }).check();
  await expect(page.locator('.edge-label i')).toHaveCount(defaults.length);
  for (const completion of await page.locator('.edge-label i').all()) {
    await expect(completion).toHaveText('default');
    await expect(completion).toBeVisible();
    await expect(completion).toHaveCSS('font-style', 'italic');
  }
  await page.getByRole('checkbox', { name: 'Enable breakpoints' }).uncheck();
  await expect(page.locator('.edge-label i')).toHaveCount(0);
});

for (const [stateId, command] of [['Review', 'initialize'], ['Cancelled', 'cancel']] as const) {
  test(`state header breakpoint pauses on entry to ${stateId}`, async ({ page, request }) => {
    await page.goto('/');
    await expect(page.locator('.canvas')).toHaveAttribute('data-layout-ready', 'true');
    await page.getByRole('button', { name: 'New instance', exact: true }).click();
    const instance = await page.getByRole('textbox', { name: 'Instance', exact: true }).inputValue();
    const scope = { family: 'stateMachine', name: 'ReviewProcess', version: 1, instance };
    const state = page.locator(`.shape[data-element-id="${vertexId(stateId)}"]`);
    if (stateId === 'Cancelled') await expect(state.locator('.operation-row[data-operation="state.entry"]')).toHaveCount(0);
    if (command !== 'initialize') await request.post(`/api/demo/machine/${instance}/initialize`);
    await page.getByRole('checkbox', { name: 'Enable breakpoints' }).check();
    const add = state.locator('.state-breakpoint').getByRole('button', { name: `Add breakpoint ${stateId} state.entry`, exact: true });
    await state.locator('.heading').hover({ force: true });
    await expect(add.locator('span')).toHaveCSS('opacity', '0.45');
    await add.click({ force: true });
    await expect(state.locator('.state-breakpoint button')).toHaveAttribute('aria-pressed', 'true');
    for (const entry of await state.locator('.operation-row[data-operation="state.entry"] .breakpoint').all())
      await expect(entry).toHaveAttribute('aria-pressed', 'true');
    if (stateId === 'Review') {
      await state.getByRole('button', { name: 'Collapse Review', exact: true }).click({ force: true });
      await expect(state.locator('.operation-details')).toHaveCount(0);
      await expect(state.locator('.state-breakpoint button')).toBeVisible();
    }
    const execution = request.post(`/api/demo/machine/${instance}/${command}`);
    try {
      await expect(page.getByTestId('debug-pause')).toContainText(`${stateId} · state.entry`);
      await page.getByRole('button', { name: 'Resume', exact: true }).click();
      expect((await execution).ok()).toBe(true);
    } finally {
      await request.post('/api/debug/breakpoints', { data: {
        scope, target: { elementId: vertexId(stateId), operation: 'state.entry' }, enabled: false
      } });
      await execution;
    }
  });
}

for (const [sourceId, targetId, command] of [
  ['Checking', 'Decision', 'finish'],
  ['Rejected junction', 'Rejected', 'reject']
] as const) {
  test(`transition without handlers pauses before ${sourceId} to ${targetId}`, async ({ page, request }) => {
    await page.goto('/');
    await expect(page.locator('.canvas')).toHaveAttribute('data-layout-ready', 'true');
    await page.getByRole('button', { name: 'New instance', exact: true }).click();
    const instance = await page.getByRole('textbox', { name: 'Instance', exact: true }).inputValue();
    const scope = { family: 'stateMachine', name: 'ReviewProcess', version: 1, instance };
    const definitions = await (await request.get('/api/definitions')).json();
    const machine = definitions.machines.find((item: { name: string }) => item.name === scope.name);
    const edge = machine.transitions.find((item: { sourceId: string; targetId: string }) =>
      item.sourceId === vertexId(sourceId) && item.targetId === vertexId(targetId));
    const target = { elementId: edge.id, operation: 'transition.guard' };
    await request.post(`/api/demo/machine/${instance}/initialize`);
    if (sourceId === 'Checking') await request.post(`/api/demo/machine/${instance}/approve`);
    await page.getByRole('checkbox', { name: 'Enable breakpoints' }).check();
    await page.locator('.transition-site').getByRole('button', { name: `Add breakpoint ${edge.id} transition.guard`, exact: true }).click({ force: true });
    await expect(page.locator('.transition-site').getByRole('button', { name: `Remove breakpoint ${edge.id} transition.guard`, exact: true })).toBeAttached();
    const execution = request.post(`/api/demo/machine/${instance}/${command}`);
    try {
      await expect(page.getByTestId('debug-pause')).toContainText(edge.id);
      await page.getByRole('button', { name: 'Resume', exact: true }).click();
      expect((await execution).ok()).toBe(true);
    } finally {
      await request.post('/api/debug/breakpoints', { data: { scope, target, enabled: false } });
      await execution;
    }
  });
}

test('state and pseudostate breakpoint slots sit immediately before their names', async ({ page, request }) => {
  await page.goto('/');
  await expect(page.locator('.canvas')).toHaveAttribute('data-layout-ready', 'true');
  const definitions = await (await request.get('/api/definitions')).json();
  const machine = definitions.machines.find((item: { name: string }) => item.name === 'ReviewProcess');
  await page.getByRole('checkbox', { name: 'Enable breakpoints' }).check();
  await expect(page.locator('.state-breakpoint')).toHaveCount(machine.states.length);
  const gaps = await page.locator('.state-breakpoint').evaluateAll(slots => slots.map(slot => {
    const label = slot.parentElement!;
    const walker = document.createTreeWalker(label, NodeFilter.SHOW_TEXT);
    while (walker.nextNode()) {
      if (!walker.currentNode.textContent?.trim()) continue;
      const range = document.createRange();
      range.selectNode(walker.currentNode);
      return range.getBoundingClientRect().left - slot.getBoundingClientRect().right;
    }
    throw new Error('Missing name after breakpoint slot');
  }));
  for (const gap of gaps) expect(Math.abs(gap)).toBeLessThan(0.5);
  await expect(page.locator('.shape[data-kind="Region"] .state-breakpoint, .shape[data-kind="Initial"] .state-breakpoint')).toHaveCount(0);
});

for (const [elementId, preparation, command] of [
  ['Dispatch', [], 'approve'],
  ['Rejected junction', [], 'reject'],
  ['Decision', ['approve'], 'finish'],
  ['Joined', ['approve'], 'synchronize'],
  ['Resume processing', ['cancel'], 'resume']
] as const) {
  test(`pseudostate breakpoint pauses at ${elementId}`, async ({ page, request }) => {
    await page.goto('/');
    await expect(page.locator('.canvas')).toHaveAttribute('data-layout-ready', 'true');
    await page.getByRole('button', { name: 'New instance', exact: true }).click();
    const instance = await page.getByRole('textbox', { name: 'Instance', exact: true }).inputValue();
    const scope = { family: 'stateMachine', name: 'ReviewProcess', version: 1, instance };
    await request.post(`/api/demo/machine/${instance}/initialize`);
    for (const step of preparation) expect((await request.post(`/api/demo/machine/${instance}/${step}`)).ok()).toBe(true);
    await page.getByRole('checkbox', { name: 'Enable breakpoints' }).check();
    await page.locator('.marker-label').getByRole('button', {
      name: `Add breakpoint ${elementId} state.entry`, exact: true
    }).click();
    await expect(page.locator('.marker-label').getByRole('button', {
      name: `Remove breakpoint ${elementId} state.entry`, exact: true
    })).toHaveAttribute('aria-pressed', 'true');
    const execution = request.post(`/api/demo/machine/${instance}/${command}`);
    try {
      await expect(page.getByTestId('debug-pause')).toContainText(`${elementId} · state.entry`);
      await page.getByRole('button', { name: 'Resume', exact: true }).click();
      expect((await execution).ok()).toBe(true);
    } finally {
      await request.post('/api/debug/breakpoints', { data: {
        scope, target: { elementId, operation: 'state.entry' }, enabled: false
      } });
      await execution;
    }
  });
}

test('event rows select all handlers of an operation without selecting the state', async ({ page }) => {
  await page.goto('/');
  await expect(page.locator('.canvas')).toHaveAttribute('data-layout-ready', 'true');
  const review = page.locator(`.shape[data-element-id="${vertexId('Review')}"]`);
  const entries = review.locator('.operation-row[data-operation="state.entry"]');
  await expect(entries).toHaveCount(2);
  await entries.last().click();
  for (const entry of await entries.all()) await expect(entry).toHaveAttribute('aria-pressed', 'true');
  await expect(review).not.toHaveClass(/selected/);
  await expect(review.locator('.operation-row[data-operation="state.exit"]')).toHaveAttribute('aria-pressed', 'false');
  await expect(page.locator('aside pre')).toContainText('entry / onEntry()');
  await expect(page.locator('aside pre')).toContainText('entry / ReviewAudit');
  await review.locator('.operation-row[data-operation="state.exit"]').press('Enter');
  for (const entry of await entries.all()) await expect(entry).toHaveAttribute('aria-pressed', 'false');
  await expect(review.locator('.operation-row[data-operation="state.exit"]')).toHaveAttribute('aria-pressed', 'true');
});

test('breakpoints are opt-in, show hover circles, and pause execution until Resume', async ({ page, request }) => {
  await page.goto('/');
  await expect(page.locator('.canvas')).toHaveAttribute('data-layout-ready', 'true');
  await expect(page.locator('.breakpoint')).toHaveCount(0);
  await page.getByRole('button', { name: 'New instance', exact: true }).click();
  const layouts = await page.getByTestId('layout-count').innerText();
  await page.getByRole('checkbox', { name: 'Enable breakpoints' }).check();
  const entries = page.locator(`.shape[data-element-id="${vertexId('Review')}"] .operation-row[data-operation="state.entry"]`);
  const add = entries.first().getByRole('button', { name: 'Add breakpoint Review state.entry', exact: true });
  await expect(add.locator('span')).toHaveCSS('opacity', '0');
  await entries.first().hover();
  await expect(add.locator('span')).toHaveCSS('opacity', '0.45');
  await add.click();
  for (const button of await entries.locator('.breakpoint').all()) await expect(button).toHaveAttribute('aria-pressed', 'true');
  await page.getByRole('button', { name: 'initialize', exact: true }).click();
  try {
    await expect(page.getByTestId('debug-pause')).toContainText('Review · state.entry');
    await expect(page.getByTestId('command-status')).toHaveText('Executing...');
    const instance = await page.getByRole('textbox', { name: 'Instance', exact: true }).inputValue();
    const capture = await (await request.get('/api/capture')).json();
    expect(capture.records.some((record: { instance: string; elementId: string; operation: string }) =>
      record.instance === instance && record.elementId === vertexId('Review') && record.operation === 'state.entry')).toBe(false);
    const other = await request.post(`/api/demo/machine/unrelated-${Date.now()}/initialize`);
    expect(other.ok()).toBe(true);
    await page.getByRole('button', { name: 'Resume', exact: true }).click();
    await expect(page.getByTestId('command-status')).toHaveText('Initialized');
    await expect(page.getByTestId('debug-pause')).toHaveCount(0);
    await entries.first().getByRole('button', { name: 'Remove breakpoint Review state.entry', exact: true }).click();
    for (const button of await entries.locator('.breakpoint').all()) await expect(button).toHaveAttribute('aria-pressed', 'false');
    await page.getByRole('checkbox', { name: 'Enable breakpoints' }).uncheck();
    await expect(page.locator('.breakpoint')).toHaveCount(0);
    await expect(page.getByTestId('layout-count')).toHaveText(layouts);
  } finally {
    const instance = await page.getByRole('textbox', { name: 'Instance', exact: true }).inputValue();
    await request.post('/api/debug/breakpoints', { data: {
      scope: { family: 'stateMachine', name: 'ReviewProcess', version: 1, instance },
      target: { elementId: vertexId('Review'), operation: 'state.entry' }, enabled: false
    } });
  }
});

for (const [elementId, operation, command, initializeFirst] of [
  ['Processing', 'state.initialize', 'initialize', false],
  ['Review', 'state.do', 'initialize', false],
  ['Review', 'state.exit', 'approve', true],
  ['Review', 'state.defer:Finish', 'finish', true]
] as const) {
  test(`runtime breakpoint pauses before ${operation}`, async ({ page, request }) => {
    await page.goto('/');
    await expect(page.locator('.canvas')).toHaveAttribute('data-layout-ready', 'true');
    await page.getByRole('button', { name: 'New instance', exact: true }).click();
    const instance = await page.getByRole('textbox', { name: 'Instance', exact: true }).inputValue();
    const scope = { family: 'stateMachine', name: 'ReviewProcess', version: 1, instance };
    const target = { elementId: vertexId(elementId), operation };
    if (initializeFirst) expect((await request.post(`/api/demo/machine/${instance}/initialize`)).ok()).toBe(true);
    expect((await request.post('/api/debug/breakpoints', { data: { scope, target, enabled: true } })).ok()).toBe(true);
    const execution = request.post(`/api/demo/machine/${instance}/${command}`);
    try {
      await expect(page.getByTestId('debug-pause')).toContainText(operation);
      await page.getByRole('button', { name: 'Resume', exact: true }).click();
      expect((await execution).ok()).toBe(true);
      await expect(page.getByTestId('debug-pause')).toHaveCount(0);
    } finally {
      await request.post('/api/debug/breakpoints', { data: { scope, target, enabled: false } });
      await execution;
    }
  });
}

test('transition guard and effect breakpoints pause independently, including internal transitions', async ({ page, request }) => {
  await page.goto('/');
  await expect(page.locator('.canvas')).toHaveAttribute('data-layout-ready', 'true');
  await page.getByRole('button', { name: 'New instance', exact: true }).click();
  const instance = await page.getByRole('textbox', { name: 'Instance', exact: true }).inputValue();
  const scope = { family: 'stateMachine', name: 'ReviewProcess', version: 1, instance };
  const definitions = await (await request.get('/api/definitions')).json();
  const transitions: { id: string; sourceId: string; targetId: string | null }[] = definitions.machines.find((item: { name: string }) => item.name === scope.name).transitions;
  const external = transitions.find(edge => edge.sourceId === vertexId('Review') && edge.targetId === vertexId('Dispatch'))!;
  const internal = transitions.find(edge => edge.sourceId === vertexId('Review') && edge.targetId === null)!;
  const targets = [{ elementId: external.id, operation: 'transition.guard' },
    { elementId: external.id, operation: 'transition.effect' },
    { elementId: internal.id, operation: 'transition.effect' }];
  await request.post(`/api/demo/machine/${instance}/initialize`);
  await page.getByRole('checkbox', { name: 'Enable breakpoints' }).check();
  for (const target of targets) {
    const add = page.getByRole('button', { name: `Add breakpoint ${target.elementId} ${target.operation}`, exact: true }).first();
    await add.click({ force: true });
    for (const button of await page.getByRole('button', { name: `Remove breakpoint ${target.elementId} ${target.operation}`, exact: true }).all())
      await expect(button).toHaveAttribute('aria-pressed', 'true');
  }
  try {
    await page.getByRole('button', { name: 'refresh', exact: true }).click();
    await expect(page.getByTestId('debug-pause')).toContainText('transition.effect');
    await page.getByRole('button', { name: 'Resume', exact: true }).click();
    await expect(page.getByTestId('command-status')).toHaveText('Consumed');
    await page.getByRole('button', { name: 'approve', exact: true }).click();
    await expect(page.getByTestId('debug-pause')).toContainText('transition.guard');
    await page.getByRole('button', { name: 'Resume', exact: true }).click();
    await expect(page.getByTestId('debug-pause')).toContainText('transition.effect');
    await expect(page.getByTestId('command-status')).toHaveText('Executing...');
    await page.getByRole('button', { name: 'Resume', exact: true }).click();
    await expect(page.getByTestId('command-status')).toHaveText('Consumed');
  } finally {
    for (const target of targets) await request.post('/api/debug/breakpoints', { data: { scope, target, enabled: false } });
  }
});

test('renders UML compartments, initial pseudostates and typed operation labels', async ({ page }) => {
  await page.goto('/');
  await expect(page.locator('.visualizer .toolbar strong')).toHaveText('ReviewProcess');
  await expect(page.locator('.canvas')).toHaveAttribute('data-layout-ready', 'true');
  const review = page.locator(`.shape[data-element-id="${vertexId('Review')}"]`);
  await expect(review.locator('.label')).toHaveText('Review');
  await expect(review.locator('.operations .detail')).toHaveText(['entry / onEntry()', 'entry / ReviewAudit', 'exit / onExit()', 'Finish [Deny] / defer', 'do / ReviewPreparation', 'Refresh / ReviewAudit']);
  await expect(page.locator('.shape[data-kind="Initial"]')).toHaveCount(4);
  await expect(page.locator('.shape[data-kind="Initial"] .marker-label')).toHaveCount(0);
  await expect(page.locator('.svelte-flow__edge-path').first()).toBeAttached();
  await expect.poll(() => page.locator('.svelte-flow__edge-path').count()).toBeGreaterThan(10);
  const initialGaps = await page.locator('.shape[data-kind="Initial"]').evaluateAll(initials => initials.map(initial => {
    const circle = initial.querySelector('circle')!;
    const center = new DOMPoint(circle.cx.baseVal.value, circle.cy.baseVal.value).matrixTransform(circle.getScreenCTM()!);
    const radius = (circle.r.baseVal.value + parseFloat(getComputedStyle(circle).strokeWidth) / 2)
      * circle.getScreenCTM()!.a;
    const id = initial.getAttribute('data-element-id')!;
    const edge = [...document.querySelectorAll('.svelte-flow__edge')]
      .find(element => element.getAttribute('data-id') === `${id}:edge`)!;
    const path = edge.querySelector<SVGPathElement>('.svelte-flow__edge-path')!;
    const start = path.getPointAtLength(0).matrixTransform(path.getScreenCTM()!);
    return Math.abs(Math.hypot(start.x - center.x, start.y - center.y) - radius);
  }));
  for (const gap of initialGaps) expect(gap).toBeLessThan(0.5);
  await expect(page.locator('.shape[data-kind="Region"]')).toHaveCount(2);
  await expect(page.locator(`.shape[data-element-id="${vertexId('Processing')}"] .substates`)).toHaveCount(1);
  await expect(page.locator('.operations .compartment-heading, .substates .compartment-heading span')).toHaveCount(0);
  await expect(page.locator(`.shape[data-element-id="${vertexId('Checking')}"] .substates`)).toHaveCount(0);
  const transition = page.locator('.edge-label').filter({ hasText: 'Approve [ApprovalAllowed] / ReviewAudit' });
  await expect(transition).toHaveCount(1);
  await expect(transition).toHaveCSS('border-top-width', '0px');
  await expect(transition).toHaveCSS('box-shadow', 'none');
  await expect(transition.locator('..')).toHaveCSS('background-color', 'rgba(0, 0, 0, 0)');
  await expect(transition.locator('..')).toHaveCSS('padding-top', '0px');
  await expect(page.locator('.edge-label').filter({ hasText: 'Refresh' })).toHaveCount(0);
  const heading = await review.locator('.heading').boundingBox();
  const operations = await review.locator('.operations').boundingBox();
  const bounds = await review.boundingBox();
  expect(operations!.y).toBeGreaterThanOrEqual(heading!.y + heading!.height - 1);
  expect(operations!.y + operations!.height).toBeLessThanOrEqual(bounds!.y + bounds!.height + 1);
  const labelBounds = await transition.boundingBox();
  expect(labelBounds!.x).toBeGreaterThanOrEqual(bounds!.x + bounds!.width);
  for (const id of ['Review', 'Done']) {
    const state = page.locator(`.shape[data-element-id="${vertexId(id)}"]`);
    const stateBounds = (await state.boundingBox())!;
    const labelBounds = (await state.locator('.label').boundingBox())!;
    expect(Math.abs(labelBounds.x + labelBounds.width / 2 - stateBounds.x - stateBounds.width / 2)).toBeLessThan(0.5);
  }
  await expect.poll(() => page.locator('.svelte-flow__edge-path').evaluateAll(paths =>
    paths.some(path => path.getAttribute('d')?.includes(' Q ')))).toBe(true);
});

test('generated initial markers cannot be selected and lifecycle compartments collapse independently', async ({ page }) => {
  await page.goto('/');
  await expect(page.locator('.canvas')).toHaveAttribute('data-layout-ready', 'true');
  for (const kind of ['Fork', 'Join', 'Junction', 'Choice', 'History'])
    await expect(page.locator(`.shape[data-kind="${kind}"]`)).toHaveCount(1);
  const processing = page.locator(`.shape[data-element-id="${vertexId('Processing')}"]`);
  await expect(processing.locator('.operations .detail')).toHaveText([
    'initialization / onInitialization()', 'initialization / ReviewAudit', 'entry / onEntry()', 'exit / onExit()',
    'finalization / onFinalization()', 'finalization / ReviewAudit'
  ]);
  const initial = page.locator('.shape[data-kind="Initial"]').first();
  await initial.locator('circle').click({ force: true });
  await expect(page.locator('aside pre')).toHaveText('Nothing selected');
  const initialNode = initial.locator('..');
  await expect(initialNode).not.toHaveAttribute('tabindex', '0');
  const initialId = await initial.getAttribute('data-element-id');
  const initialEdge = page.locator('.svelte-flow__edge').filter({ has: page.locator('path') });
  const edgeIndex = await initialEdge.evaluateAll((elements, id) => elements.findIndex(element => element.getAttribute('data-id') === `${id}:edge`), initialId);
  const edge = initialEdge.nth(edgeIndex);
  await edge.locator('.svelte-flow__edge-interaction').dispatchEvent('click');
  await expect(page.locator('aside pre')).toHaveText('Nothing selected');
  const before = (await processing.boundingBox())!.height;
  await processing.getByRole('button', { name: 'Collapse events of Processing', exact: true }).click();
  await expect(processing.locator('.operations')).toContainText('Events (6)');
  await expect(page.locator(`.shape[data-element-id="${vertexId('Review')}"]`)).toBeVisible();
  await expect(page.locator('.canvas')).toHaveAttribute('data-layout-ready', 'true');
  expect((await processing.boundingBox())!.height).toBeLessThan(before);
  await processing.getByRole('button', { name: 'Expand events of Processing', exact: true }).click();
  await expect(processing.locator('.operations .detail')).toHaveCount(6);
  const region = page.locator('.shape[data-kind="Region"]').first();
  for (const button of [
    processing.getByRole('button', { name: 'Collapse Processing', exact: true }),
    processing.getByRole('button', { name: 'Collapse events of Processing', exact: true }),
    processing.getByRole('button', { name: 'Collapse substates of Processing', exact: true }),
    region.locator('.heading > button')
  ]) {
    await expect(button).toHaveCSS('width', '22px');
    await expect(button).toHaveCSS('height', '22px');
    await expect(button).toHaveCSS('right', '6px');
    await expect(button).toHaveCSS('top', '8px');
  }
  await processing.getByRole('button', { name: 'Collapse substates of Processing', exact: true }).click();
  await expect(page.locator(`.shape[data-element-id="${vertexId('Review')}"]`)).toHaveCount(0);
  await expect(processing.locator('.operations .detail')).toHaveCount(6);
  await processing.getByRole('button', { name: 'Collapse events of Processing', exact: true }).click();
  await processing.getByRole('button', { name: 'Collapse Processing', exact: true }).click();
  await expect(processing.locator('.operations, .substates')).toHaveCount(0);
  await expect(page.locator('.shape[data-kind="Region"]')).toHaveCount(0);
  await processing.getByRole('button', { name: 'Expand Processing', exact: true }).click();
  await expect(processing.getByRole('button', { name: 'Expand events of Processing', exact: true })).toBeVisible();
  await expect(processing.getByRole('button', { name: 'Expand substates of Processing', exact: true })).toBeVisible();
  await processing.getByRole('button', { name: 'Expand substates of Processing', exact: true }).click();
  await expect(page.locator(`.shape[data-element-id="${vertexId('Review')}"]`)).toBeVisible();
  const review = page.locator(`.shape[data-element-id="${vertexId('Review')}"]`);
  await review.getByRole('button', { name: 'Collapse Review', exact: true }).click();
  await expect(review.locator('.operations')).toHaveCount(0);
  await expect(review).toHaveCSS('height', '42px');
  await review.getByRole('button', { name: 'Expand Review', exact: true }).click();
  await expect(review.locator('.operations .detail')).toHaveCount(6);
  for (const kind of ['Fork', 'Join']) {
    const bar = page.locator(`.shape[data-kind="${kind}"]`);
    const box = (await bar.boundingBox())!;
    expect(box.height).toBeGreaterThan(box.width);
  }
});

test('embedded activity icons are inline in events and navigate from ordinary and orthogonal states', async ({ page }) => {
  await page.goto('/');
  await expect(page.locator('.canvas')).toHaveAttribute('data-layout-ready', 'true');
  for (const id of ['Review', 'Checking']) {
    const state = page.locator(`.shape[data-element-id="${vertexId(id)}"]`);
    const icon = state.getByRole('button', { name: `Open Activity ReviewProcess.${id}.do`, exact: true });
    await expect(icon).toBeVisible();
    await expect(icon).toHaveCSS('border-top-width', '0px');
    await expect(icon.locator('..')).toHaveAttribute('data-operation', 'state.do');
    await state.getByRole('button', { name: `Collapse ${id}`, exact: true }).click();
    await expect(state.locator('.operations, .substates')).toHaveCount(0);
    await expect(icon).toHaveCount(0);
    await state.getByRole('button', { name: `Expand ${id}`, exact: true }).click();
    await state.getByRole('button', { name: `Collapse events of ${id}`, exact: true }).click();
    await expect(icon).toHaveCount(0);
    await state.getByRole('button', { name: `Expand events of ${id}`, exact: true }).click();
    await expect(icon).toBeVisible();
    await icon.click();
    await expect(page.locator('.visualizer .toolbar strong')).toHaveText(`ReviewProcess.${id}.do`);
    await expect(page.locator('.canvas')).toHaveAttribute('data-layout-ready', 'true');
    await expect(page.locator('.shape').filter({ has: page.locator('.label', { hasText: /^Prepare review$/ }) })).toBeVisible();
    await expect(page.locator('.workspace > [role="alert"]')).toHaveCount(0);
    await page.getByRole('combobox', { name: 'Model', exact: true }).selectOption({ label: 'ReviewProcess v1' });
    await expect(page.locator('.canvas')).toHaveAttribute('data-layout-ready', 'true');
  }
});

test('doAction and submachine icons navigate to their registered definitions', async ({ page }) => {
  await page.goto('/');
  await expect(page.locator('.canvas')).toHaveAttribute('data-layout-ready', 'true');
  for (const [stateId, type, suffix, targetLabel] of [
    ['Done', 'Action', 'do', 'CompletionAudit'],
    ['Rejected', 'StateMachine', 'submachine', 'Record rejection']
  ]) {
    const name = `ReviewProcess.${stateId}.${suffix}`;
    const state = page.locator(`.shape[data-element-id="${vertexId(stateId)}"]`);
    const icon = state.getByRole('button', { name: `Open ${type} ${name}`, exact: true });
    await expect(icon).toBeVisible();
    await state.getByRole('button', { name: `Collapse ${stateId}`, exact: true }).click();
    await expect(state.locator('.operations, .substates')).toHaveCount(0);
    await expect(icon).toHaveCount(0);
    await state.getByRole('button', { name: `Expand ${stateId}`, exact: true }).click();
    await icon.click();
    await expect(page.locator('.visualizer .toolbar strong')).toHaveText(name);
    await expect(page.locator('.canvas')).toHaveAttribute('data-layout-ready', 'true');
    await expect(page.locator('.shape .label', { hasText: new RegExp(`^${targetLabel}$`) })).toBeVisible();
    await expect(page.locator('.workspace > [role="alert"]')).toHaveCount(0);
    await page.getByRole('combobox', { name: 'Model', exact: true }).selectOption({ label: 'ReviewProcess v1' });
    await expect(page.locator('.canvas')).toHaveAttribute('data-layout-ready', 'true');
  }
});

test('junction incoming and outgoing arrows meet its visible circle', async ({ page }) => {
  await page.goto('/');
  await expect(page.locator('.canvas')).toHaveAttribute('data-layout-ready', 'true');
  const gaps = await page.locator('.shape[data-kind="Junction"]').evaluate(junction => {
    const circle = junction.querySelector('circle')!;
    const center = new DOMPoint(50, 50).matrixTransform(circle.getScreenCTM()!);
    const radius = (circle.r.baseVal.value + parseFloat(getComputedStyle(circle).strokeWidth) / 2)
      * circle.getScreenCTM()!.a;
    return [...document.querySelectorAll('.svelte-flow__edge-path')].flatMap(element => {
      const path = element as SVGPathElement;
      return [0, path.getTotalLength()].map(length => {
        const point = path.getPointAtLength(length).matrixTransform(path.getScreenCTM()!);
        return Math.abs(Math.hypot(point.x - center.x, point.y - center.y) - radius);
      });
    }).sort((a, b) => a - b).slice(0, 2);
  });
  expect(gaps).toHaveLength(2);
  for (const gap of gaps) expect(gap).toBeLessThan(0.5);
});

test('orthogonal regions are full-width, independently collapsible state compartments', async ({ page }) => {
  await page.goto('/');
  await expect(page.locator('.canvas')).toHaveAttribute('data-layout-ready', 'true');
  const state = page.locator(`.shape[data-element-id="${vertexId('Checking')}"]`);
  const regions = page.locator('.shape[data-kind="Region"]');
  const first = regions.filter({ has: page.locator('.label', { hasText: 'Region 1' }) });
  const second = regions.filter({ has: page.locator('.label', { hasText: 'Region 2' }) });
  await expect(regions).toHaveCount(2);
  for (const region of [first, second]) {
    await expect(region).toHaveCSS('background-color', 'rgba(0, 0, 0, 0)');
    await expect(region).toHaveCSS('border-left-width', '0px');
    await expect(region).toHaveCSS('border-right-width', '0px');
    await expect(region).toHaveCSS('border-bottom-width', '0px');
    await expect(region.locator('.substates')).toHaveCount(0);
    await expect(region.locator('.label')).toHaveCSS('text-align', 'left');
  }
  await expect(page.locator('.shape[data-kind="Region"].region-separator')).toHaveCount(1);
  const stateBox = (await state.boundingBox())!;
  const firstBox = (await first.boundingBox())!;
  const secondBox = (await second.boundingBox())!;
  expect(firstBox.width).toBeGreaterThanOrEqual(stateBox.width - 3);
  expect(firstBox.width).toBeLessThan(stateBox.width);
  expect(Math.abs(firstBox.x - secondBox.x)).toBeLessThan(1);
  expect(Math.abs(firstBox.width - secondBox.width)).toBeLessThan(1);
  const [upper, lower] = [firstBox, secondBox].sort((a, b) => a.y - b.y);
  expect(Math.abs(upper.y + upper.height - lower.y)).toBeLessThan(1);

  await first.getByRole('button', { name: 'Collapse Region 1', exact: true }).click();
  await expect(page.locator('.canvas')).toHaveAttribute('data-layout-ready', 'true');
  await expect(page.locator(`.shape[data-element-id="${vertexId('PaymentPending')}"]`)).toHaveCount(0);
  await expect(page.locator(`.shape[data-element-id="${vertexId('StockPending')}"]`)).toBeVisible();
  await expect(first).toContainText('4 hidden elements');
  await expect(first).toHaveCSS('height', '38px');
  expect((await first.boundingBox())!.height).toBeLessThan(firstBox.height);
  await state.getByRole('button', { name: 'Collapse Checking', exact: true }).click();
  await expect(regions).toHaveCount(0);
  await expect(state.locator('.operations, .substates')).toHaveCount(0);
  await state.getByRole('button', { name: 'Expand Checking', exact: true }).click();
  await expect(first.getByRole('button', { name: 'Expand Region 1', exact: true })).toBeVisible();
  await expect(page.locator(`.shape[data-element-id="${vertexId('PaymentPending')}"]`)).toHaveCount(0);
  await expect(page.locator(`.shape[data-element-id="${vertexId('StockPending')}"]`)).toBeVisible();
  await second.getByRole('button', { name: 'Collapse Region 2', exact: true }).click();
  await expect(page.locator(`.shape[data-element-id="${vertexId('StockPending')}"]`)).toHaveCount(0);
  await expect(first).toHaveCSS('height', '38px');
  await expect(second).toHaveCSS('height', '38px');
  await first.getByRole('button', { name: 'Expand Region 1', exact: true }).click();
  await expect(page.locator(`.shape[data-element-id="${vertexId('PaymentPending')}"]`)).toBeVisible();
  await expect(page.locator(`.shape[data-element-id="${vertexId('StockPending')}"]`)).toHaveCount(0);
  await second.getByRole('button', { name: 'Expand Region 2', exact: true }).click();
  await expect(page.locator(`.shape[data-element-id="${vertexId('StockPending')}"]`)).toBeVisible();
});

test('executes C# state machine and shows guard outcomes and current states', async ({ page }) => {
  const errors: string[] = [];
  page.on('pageerror', error => errors.push(error.message));
  await page.goto('/');
  await expect(page.locator('.visualizer .toolbar strong')).toHaveText('ReviewProcess');
  await expect(page.getByText('Connected', { exact: true })).toBeVisible();
  await expect(page.locator('.canvas')).toHaveAttribute('data-layout-ready', 'true');
  await page.getByRole('button', { name: 'New instance', exact: true }).click();
  const count = await page.getByTestId('layout-count').innerText();
  await page.getByRole('button', { name: 'initialize', exact: true }).click();
  await expect(page.getByTestId('command-status')).toHaveText('Initialized');
  await expect(page.locator(`.shape[data-element-id="${vertexId('Review')}"]`)).toHaveAttribute('data-status', 'active');
  await page.getByRole('button', { name: 'approve', exact: true }).click();
  await expect(page.getByTestId('command-status')).toHaveText('Consumed');
  await expect(page.locator(`.shape[data-element-id="${vertexId('PaymentPending')}"]`)).toHaveAttribute('data-status', 'active');
  await expect(page.locator('.timeline')).toContainText('guard-rejected');
  await expect(page.locator('.timeline')).toContainText('taken');
  await expect(page.getByTestId('layout-count')).toHaveText(count);
  await page.locator('.timeline button').first().click();
  await expect(page.getByRole('combobox', { name: 'Mode', exact: true })).toHaveValue('execution');
  await expect(page.locator('aside pre')).not.toHaveText('Nothing selected');
  await page.getByRole('button', { name: 'finish', exact: true }).click();
  await expect(page.locator(`.shape[data-element-id="${vertexId('Done')}"]`)).toHaveAttribute('data-status', 'active');
  await page.getByRole('button', { name: 'New instance', exact: true }).click();
  await expect(page.getByTestId('observation-count')).toContainText('0 observations');
  await page.getByRole('button', { name: 'initialize', exact: true }).click();
  await expect(page.getByTestId('command-status')).toHaveText('Initialized');
  await page.getByRole('button', { name: 'reject', exact: true }).click();
  await expect(page.locator(`.shape[data-element-id="${vertexId('Rejected')}"]`)).toHaveAttribute('data-status', 'active');
  await page.getByRole('button', { name: 'cancel', exact: true }).click();
  await expect(page.locator(`.shape[data-element-id="${vertexId('Cancelled')}"]`)).toHaveAttribute('data-status', 'active');
  expect(errors).toEqual([]);
});

test('executes fork, history re-entry, synchronized join and decision routes', async ({ page }) => {
  await page.goto('/');
  await expect(page.getByText('Connected', { exact: true })).toBeVisible();
  await page.getByRole('button', { name: 'New instance', exact: true }).click();
  await page.getByRole('button', { name: 'initialize', exact: true }).click();
  await expect(page.getByTestId('command-status')).toHaveText('Initialized');
  await page.getByRole('button', { name: 'approve', exact: true }).click();
  await expect(page.locator(`.shape[data-element-id="${vertexId('PaymentPending')}"]`)).toHaveAttribute('data-status', 'active');
  await expect(page.locator(`.shape[data-element-id="${vertexId('StockPending')}"]`)).toHaveAttribute('data-status', 'active');
  await page.getByRole('button', { name: 'cancel', exact: true }).click();
  await expect(page.locator(`.shape[data-element-id="${vertexId('Cancelled')}"]`)).toHaveAttribute('data-status', 'active');
  await page.getByRole('button', { name: 'resume', exact: true }).click();
  await expect(page.locator(`.shape[data-element-id="${vertexId('Checking')}"]`)).toHaveAttribute('data-status', 'active');
  await page.getByRole('button', { name: 'synchronize', exact: true }).click();
  await expect(page.getByTestId('command-status')).toHaveText('Consumed');
  await expect(page.locator(`.shape[data-element-id="${vertexId('Done')}"]`)).toHaveAttribute('data-status', 'active');
  await expect(page.locator('.workspace > [role="alert"]')).toHaveCount(0);
});

for (const name of ['NestedCalculation', 'Telemetry']) {
  test(`executes C# activity and displays SSE observations: ${name}`, async ({ page }) => {
    const errors: string[] = [];
    page.on('pageerror', error => errors.push(error.message));
    await page.goto('/');
    await expect(page.getByText('Connected', { exact: true })).toBeVisible();
    await page.getByRole('combobox', { name: 'Model', exact: true }).selectOption({ label: `${name} v1` });
    await expect(page.locator('.canvas')).toHaveAttribute('data-layout-ready', 'true', { timeout: 15_000 });
    await page.getByRole('button', { name: 'New instance', exact: true }).click();
    if (name === 'Telemetry') {
      await page.getByRole('button', { name: 'initialize', exact: true }).click();
      await expect(page.getByTestId('command-status')).toHaveText('Initialized');
      await page.getByRole('button', { name: 'configure', exact: true }).click();
      await expect(page.getByTestId('command-status')).toHaveText('Consumed');
      await page.getByRole('button', { name: 'Send samples', exact: true }).click();
    } else await page.getByRole('button', { name: 'Run calculation', exact: true }).click();
    await expect(page.getByTestId('command-status')).toHaveText(/Initialized|Consumed/);
    await expect(page.locator('.timeline')).toContainText('node.execute', { timeout: 20_000 });
    await expect(page.locator('.timeline')).toContainText('flow.activation');
    await expect(page.locator('.shape[data-status="activated"]').first()).toBeAttached();
    if (name === 'NestedCalculation') {
      await expect(page.locator('.timeline small').filter({ hasText: /^Result$/ }).first()).toBeVisible();
    }
    await expect(page.locator('.visualizer [role="alert"]')).toHaveCount(0);
    expect(errors).toEqual([]);
  });
}

import { expect, test } from '@playwright/test';
import { syntheticFixtures } from '../development/fixtures.js';
import type { StateMachineDefinition } from '../src/lib/adapters.js';

test('marker selection follows every symbol contour rather than its rectangular bounds', async ({ page }) => {
  const kinds = ['Junction', 'Choice', 'History', 'FinalState', 'Fork', 'Join', 'Decision', 'Merge', 'Final'];
  const definition: StateMachineDefinition = {
    name: 'Selection contours', version: 1, initialStateId: null,
    states: kinds.map(kind => ({
      id: kind, name: kind, kind, regionId: null, initializeActions: 0, finalizeActions: 0,
      entryActions: 0, exitActions: 0, deferredEvents: [], behaviorType: null, behaviorName: null
    })), regions: [], transitions: []
  };
  await page.route('**/api/definitions', route => route.fulfill({ json: { machines: [definition], activities: [] } }));
  await page.goto('/');
  await expect(page.locator('.visualizer .toolbar strong')).toHaveText(definition.name);
  await expect(page.locator('.canvas')).toHaveAttribute('data-layout-ready', 'true');
  for (const kind of kinds) {
    const marker = page.locator(`.shape[data-kind="${kind}"]`);
    await marker.locator('svg > :first-child').dispatchEvent('click');
    await expect(marker).toHaveClass(/selected/);
    await expect(marker).toHaveCSS('outline-style', 'none');
    await expect(marker.locator('.selection-contour')).toHaveCSS('stroke', 'rgb(196, 181, 253)');
    await expect(marker.locator('.selection-contour')).toHaveCSS('pointer-events', 'none');
    const gap = await marker.evaluate(element => {
      const symbol = element.querySelector<SVGGeometryElement>('svg > :first-child')!;
      const outline = element.querySelector<SVGGeometryElement>('.selection-contour > :first-child')!;
      const symbolBounds = symbol.getBoundingClientRect();
      const outlineBounds = outline.getBoundingClientRect();
      const scale = element.getBoundingClientRect().width / (element as HTMLElement).offsetWidth;
      const stroke = parseFloat(getComputedStyle(symbol).strokeWidth) * symbol.getScreenCTM()!.a / scale;
      const expansion = (outlineBounds.width - symbolBounds.width) / 2 / scale;
      const perpendicularExpansion = ['Choice', 'Decision', 'Merge'].includes(element.getAttribute('data-kind')!)
        ? expansion / Math.SQRT2 : expansion;
      return perpendicularExpansion - (getComputedStyle(symbol).stroke === 'none' ? 0 : stroke / 2) - 1.5;
    });
    expect(gap).toBeCloseTo(3, 1);
    if (kind === 'FinalState' || kind === 'Final')
      await expect(marker.locator('svg').first().locator('circle').nth(1)).not.toHaveCSS('stroke', 'rgb(196, 181, 253)');
  }
});

test('expanded event text and collapsed compartment labels have the same top offset', async ({ page }) => {
  const definition: StateMachineDefinition = {
    name: 'Compartment alignment', version: 1, initialStateId: null,
    states: [{ id: 'state', name: 'State', kind: 'State', regionId: null,
      initializeActions: 1, finalizeActions: 0, entryActions: 1, exitActions: 1,
      deferredEvents: [], behaviorType: null, behaviorName: null }], regions: [], transitions: []
  };
  await page.route('**/api/definitions', route => route.fulfill({ json: { machines: [definition], activities: [] } }));
  await page.goto('/');
  await expect(page.locator('.visualizer .toolbar strong')).toHaveText(definition.name);
  await expect(page.locator('.canvas')).toHaveAttribute('data-layout-ready', 'true');
  const state = page.locator('.shape[data-element-id="state"]');
  const topOffset = (element: Element) => {
    const compartment = element.closest<HTMLElement>('.operations')!;
    const bounds = compartment.getBoundingClientRect();
    const text = document.createRange();
    text.selectNodeContents(element);
    return (text.getBoundingClientRect().top - bounds.top) / (bounds.height / compartment.offsetHeight);
  };
  const expandedOffset = await state.locator('.detail').first().evaluate(topOffset);
  await state.getByRole('button', { name: 'Collapse events of State', exact: true }).click();
  await expect(state.locator('.compartment-heading span')).toHaveText('Events (3)');
  const collapsedOffset = await state.locator('.compartment-heading span').evaluate(topOffset);
  expect(Math.abs(expandedOffset - collapsedOffset)).toBeLessThan(0.5);
  await state.getByRole('button', { name: 'Expand events of State', exact: true }).click();
  await expect(state.locator('.detail')).toHaveCount(3);
  expect(Math.abs(await state.locator('.detail').first().evaluate(topOffset) - collapsedOffset)).toBeLessThan(0.5);
});

for (const { document } of syntheticFixtures) {
  test(`renders and collapses without a backend: ${document.id}`, async ({ page }) => {
    const errors: string[] = [];
    page.on('pageerror', error => errors.push(error.message));
    // Deliberately unavailable: fixture selection and layout must not depend on the .NET host.
    await page.route('**/api/definitions', route => route.fulfill({ status: 503, body: 'Backend intentionally unavailable' }));
    await page.goto('/');
    await expect(page.getByText('Backend unavailable - synthetic fixtures remain usable')).toBeVisible();
    await page.getByRole('combobox', { name: 'Model', exact: true }).selectOption(document.id);
    await expect(page.locator('.visualizer .toolbar strong')).toHaveText(document.name);
    await expect(page.locator('.canvas')).toHaveAttribute('data-layout-ready', 'true', { timeout: 30_000 });
    await expect(page.locator('.shape')).toHaveCount(document.nodes.length);
    const labeledEdges = document.edges.filter(edge => edge.label && (document.family !== 'stateMachine' || edge.target !== undefined)).length;
    await expect(page.locator('.edge-label')).toHaveCount(labeledEdges);
    await expect(page.locator('.visualizer [role="alert"]')).toHaveCount(0);
    await expect(page.getByTestId('fixture-description')).toContainText('No runtime execution is simulated.');
    await expect(page.locator('.shape[data-status]')).toHaveCount(0);
    const compact = document.id.endsWith(':small') || document.id.endsWith(':deep');
    const containerId = document.id.endsWith(':deep') ? 'scope-0'
      : document.family === 'stateMachine' ? 'module-0/Order' : 'module-0/Process';
    const container = page.locator(`.shape[data-element-id="${containerId}"]`);
    if (compact) await container.locator('.heading > button').click();
    else await page.getByRole('button', { name: 'Collapse top-level scopes', exact: true }).click();
    await expect(page.locator('.canvas')).toHaveAttribute('data-layout-ready', 'true', { timeout: 30_000 });
    await expect.poll(() => page.locator('.shape').count()).toBeLessThan(document.nodes.length);
    if (compact) {
      await page.locator('.svelte-flow__controls-fitview').click();
      await container.locator('.heading > button').click();
    }
    else await page.getByRole('button', { name: 'Expand all scopes', exact: true }).click();
    await expect(page.locator('.shape')).toHaveCount(document.nodes.length, { timeout: 30_000 });
    await expect(page.locator('.edge-label')).toHaveCount(labeledEdges);
    expect(errors).toEqual([]);
  });
}

test('local ancestor transitions stay inside while external transitions cross the state boundary', async ({ page }, testInfo) => {
  const cases = [
    { id: 'local-in', isLocal: true, fromParent: true },
    { id: 'local-out', isLocal: true, fromParent: false },
    { id: 'external-in', isLocal: false, fromParent: true },
    { id: 'external-out', isLocal: false, fromParent: false }
  ];
  const definition: StateMachineDefinition = {
    name: 'Ancestor transitions', version: 1, initialStateId: null,
    states: cases.flatMap(({ id, isLocal }) => [
      { id, name: `${isLocal ? 'Local' : 'External'} ${id.endsWith('in') ? 'inward' : 'outward'}`,
        kind: 'CompositeState', regionId: null, initializeActions: 0, finalizeActions: 0,
        entryActions: 0, exitActions: 0, deferredEvents: [], behaviorType: null, behaviorName: null },
      { id: `${id}-child`, name: 'Child', kind: 'State', regionId: `${id}-region`,
        initializeActions: 0, finalizeActions: 0, entryActions: 0, exitActions: 0,
        deferredEvents: [], behaviorType: null, behaviorName: null }
    ]),
    regions: cases.map(({ id }) => ({
      id: `${id}-region`, stateId: id, index: 0, initialStateId: null, historyStateId: null
    })),
    transitions: cases.map(({ id, isLocal, fromParent }) => ({
      id: `${id}-edge`, sourceId: fromParent ? id : `${id}-child`, targetId: fromParent ? `${id}-child` : id,
      triggers: ['Trigger'], isElse: false, isLocal, guards: 0, effects: 0
    }))
  };
  await page.route('**/api/definitions', route => route.fulfill({ json: { machines: [definition], activities: [] } }));
  await page.goto('/');
  await expect(page.locator('.visualizer .toolbar strong')).toHaveText(definition.name);
  await expect(page.locator('.canvas')).toHaveAttribute('data-layout-ready', 'true');
  await expect(page.locator('.svelte-flow__edge-path')).toHaveCount(4);
  for (const { id, isLocal } of cases) {
    const bounds = (await page.locator(`.shape[data-element-id="${id}"]`).boundingBox())!;
    const path = page.locator(`.svelte-flow__edge[data-id="${id}-edge"] .svelte-flow__edge-path`);
    const points = await path.evaluate(element => {
      const path = element as SVGPathElement;
      return Array.from({ length: 101 }, (_, index) => {
        const point = path.getPointAtLength(path.getTotalLength() * index / 100).matrixTransform(path.getScreenCTM()!);
        return { x: point.x, y: point.y };
      });
    });
    expect(points.some(point => point.x < bounds.x - 0.5 || point.x > bounds.x + bounds.width + 0.5 ||
      point.y < bounds.y - 0.5 || point.y > bounds.y + bounds.height + 0.5)).toBe(!isLocal);
    if (!isLocal) await expect(path).toHaveAttribute('d', /Q/);
  }
  await page.screenshot({ path: testInfo.outputPath('ancestor-transitions.png') });
});

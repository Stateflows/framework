import { expect, test } from '@playwright/test';

const models = [
  'AssistanceHoursProcess', 'BackgroundCleanerStateMachine', 'BinderProcess',
  'DeviationProcess', 'DossierProcess', 'ExecutionsProcess', 'FeesProcess',
  'DemandForCareProcess', 'PatientJournalProcess', 'Identification', 'PersonalProcess'
];

for (const name of models) {
  test(`production definition renders and interacts: ${name}`, async ({ page, request }) => {
    const errors: string[] = [];
    page.on('pageerror', error => errors.push(error.message));
    const definitions = await (await request.get('/api/definitions')).json();
    expect(definitions.machines.some((machine: { name: string }) => machine.name === 'ProcessBase')).toBe(false);
    const machine = definitions.machines.find((machine: { name: string }) => machine.name === name);
    expect(machine).toBeDefined();
    await page.goto('/');
    await page.getByRole('combobox', { name: 'Model', exact: true }).selectOption({ label: `${name} v1` });
    await expect(page.locator('.visualizer .toolbar strong')).toHaveText(name);
    await expect(page.locator('.canvas')).toHaveAttribute('data-layout-ready', 'true');
    for (const ordering of ['AUTOMATIC', 'PREFER_DEFINITION', 'PRESERVE_DEFINITION']) {
      const orderControl = page.getByRole('combobox', { name: 'State ordering', exact: true });
      if (await orderControl.inputValue() !== ordering) {
        const count = await page.getByTestId('layout-count').innerText();
        await orderControl.selectOption(ordering);
        await expect(page.getByTestId('layout-count')).not.toHaveText(count);
      }
      for (const direction of ['RIGHT', 'DOWN']) {
        for (const routing of ['ORTHOGONAL', 'POLYLINE', 'SPLINES']) {
          for (const [label, value] of [['Flow direction', direction], ['Transition routing', routing]]) {
            const control = page.getByRole('combobox', { name: label, exact: true });
            if (await control.inputValue() !== value) {
              const count = await page.getByTestId('layout-count').innerText();
              await control.selectOption(value);
              await expect(page.getByTestId('layout-count')).not.toHaveText(count);
            }
          }
          await expect(page.locator('.canvas')).toHaveAttribute('data-layout-ready', 'true');
          await expect(page.locator('.shape:not([data-kind="Region"]):not([data-kind="Initial"])')).toHaveCount(machine.states.length);
          const routes = await page.locator('.svelte-flow__edge-path').evaluateAll(paths =>
            paths.map(path => ({ d: path.getAttribute('d'), length: (path as SVGPathElement).getTotalLength() })));
          expect(routes.every(route => route.d && !/NaN|undefined/.test(route.d) && route.length > 0)).toBe(true);
          await expect(page.locator('.workspace > [role="alert"]')).toHaveCount(0);
        }
      }
    }
    await page.getByRole('checkbox', { name: 'Enable breakpoints' }).check();
    const layouts = await page.getByTestId('layout-count').innerText();
    await page.getByRole('textbox', { name: 'Find diagram element' }).fill('Ongoing');
    await page.getByRole('button', { name: 'Find', exact: true }).click();
    await expect(page.locator('.shape.focused')).toHaveCount(machine.states.filter((state: { name: string }) =>
      state.name.toLowerCase().includes('ongoing')).length);
    await page.getByRole('button', { name: 'Clear focus', exact: true }).click();
    await page.locator('.trigger-filter summary').click();
    const trigger = page.locator('.trigger-options input[type="checkbox"]').first();
    await trigger.uncheck();
    await expect(page.getByTestId('layout-count')).toHaveText(layouts);
    await page.getByRole('button', { name: 'Show all triggers', exact: true }).click();
    await page.locator('.trigger-filter summary').click();
    await expect(page.locator('.state-breakpoint')).toHaveCount(machine.states.length);
    for (const edge of machine.transitions as { id: string }[])
      await expect(page.locator('.transition-site').getByRole('button', {
        name: `Add breakpoint ${edge.id} transition.guard`, exact: true
      })).toHaveCount(1);
    const operation = page.locator('.operation-row').first();
    if (await operation.count()) {
      await operation.press('Enter');
      await expect(operation).toHaveAttribute('aria-pressed', 'true');
    }
    const collapse = page.locator('.shape .heading > .collapse-toggle').first();
    if (await collapse.count()) {
      await page.getByRole('button', { name: 'Fit View', exact: true }).click();
      await collapse.click({ force: true });
      await expect(page.locator('.canvas')).toHaveAttribute('data-layout-ready', 'true');
      await page.getByRole('button', { name: 'Expand all scopes', exact: true }).click();
      await expect(page.locator('.canvas')).toHaveAttribute('data-layout-ready', 'true');
    }
    const embedded = page.getByRole('button', { name: /^Open (Activity|Action|StateMachine) / }).first();
    if (await embedded.count()) {
      await embedded.press('Enter');
      await expect(page.locator('.canvas')).toHaveAttribute('data-layout-ready', 'true');
      await expect(page.locator('.visualizer .toolbar strong')).not.toHaveText(name);
    }
    expect(errors).toEqual([]);
    await expect(page.locator('.workspace > [role="alert"]')).toHaveCount(0);
  });
}

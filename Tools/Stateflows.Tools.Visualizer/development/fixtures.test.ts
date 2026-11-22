import { describe, expect, test } from 'vitest';
import { deepModel, syntheticFixtures } from './fixtures.js';
import { layoutDiagram, validateDocument } from '../src/lib/layout.js';
import type { DiagramDocument, DiagramLayout } from '../src/lib/model.js';

function verifyGeometry(document: DiagramDocument, layout: DiagramLayout) {
  const nodes = new Map(layout.nodes.map(node => [node.id, node]));
  for (const node of layout.nodes) {
    for (const value of [node.position.x, node.position.y, node.width, node.height]) expect(Number.isFinite(value)).toBe(true);
    expect(node.width).toBeGreaterThan(0);
    expect(node.height).toBeGreaterThan(0);
    if (node.parentId) {
      const parent = nodes.get(node.parentId)!;
      expect(parent).toBeDefined();
      expect(node.position.x).toBeGreaterThanOrEqual(0);
      expect(node.position.y).toBeGreaterThanOrEqual(0);
      expect(node.position.x + node.width).toBeLessThanOrEqual(parent.width + 0.1);
      expect(node.position.y + node.height).toBeLessThanOrEqual(parent.height + 0.1);
      expect(layout.nodes.indexOf(parent)).toBeLessThan(layout.nodes.indexOf(node));
    }
  }
  for (const edge of layout.edges) {
    expect(document.edges.some(item => item.id === edge.id)).toBe(true);
    expect(nodes.has(edge.visibleSource)).toBe(true);
    expect(nodes.has(edge.visibleTarget)).toBe(true);
    expect(edge.paths.length).toBeGreaterThan(0);
    for (const path of edge.paths) {
      expect(path.length).toBeGreaterThanOrEqual(2);
      for (const point of path) {
        expect(Number.isFinite(point.x)).toBe(true);
        expect(Number.isFinite(point.y)).toBe(true);
      }
    }
  }
}

describe('synthetic definition fixtures', () => {
  test('have stable, unique definition identities and valid endpoints', () => {
    expect(new Set(syntheticFixtures.map(fixture => fixture.document.id)).size).toBe(8);
    for (const { document } of syntheticFixtures) {
      expect(() => validateDocument(document)).not.toThrow();
      expect(document.id.startsWith('synthetic:')).toBe(true);
    }
    expect(deepModel('stateMachine')).toEqual(deepModel('stateMachine'));
  });

  test('large fixtures contain production-scale element counts', () => {
    for (const family of ['stateMachine', 'activity'] as const) {
      const document = syntheticFixtures.find(item => item.document.id === `synthetic:${family}:large`)!.document;
      expect(document.nodes.length).toBeGreaterThanOrEqual(480);
      expect(document.edges.length).toBeGreaterThanOrEqual(480);
    }
  });

  for (const { document } of syntheticFixtures) {
    test(`routes every edge and contains children: ${document.id}`, async () => {
      const layout = await layoutDiagram(document);
      expect(layout.nodes.length).toBe(document.nodes.length);
      expect(layout.edges.length).toBe(document.edges.filter(edge => document.family !== 'stateMachine' || edge.target !== undefined).length);
      verifyGeometry(document, layout);
      // Regression ceiling, not a claim about interactive performance on every machine.
      expect(layout.durationMs).toBeLessThan(20_000);
    }, 30_000);
  }

  test.each(['stateMachine', 'activity'] as const)('collapse preserves crossing identities: %s', async family => {
    const document = deepModel(family).document;
    const layout = await layoutDiagram(document, { collapsedIds: ['scope-1'], direction: 'DOWN' });
    verifyGeometry(document, layout);
    expect(layout.nodes.map(node => node.id).sort()).toEqual(['scope-0', 'leaf-0-0', 'leaf-0-1', 'scope-1'].sort());
    expect(layout.nodes.find(node => node.id === 'scope-1')?.hiddenCount).toBe(32);
    const escape = layout.edges.find(edge => edge.id === 'escape')!;
    expect(escape.source).toBe('leaf-11-1');
    expect(escape.visibleSource).toBe('scope-1');
    expect(escape.visibleTarget).toBe('leaf-0-0');
    expect(escape.hiddenEndpoints).toBe(true);
    expect(layout.edges.some(edge => edge.id === 'step-11')).toBe(false);
  });
});

import { expect, test } from 'vitest';
import { roundedPath, routedPath } from '../src/lib/edgePath.js';

test('renders polylines and smooth cubic routes distinctly without moving endpoints', () => {
  const points = [{ x: 0, y: 0 }, { x: 10, y: 0 }, { x: 20, y: 30 }, { x: 40, y: 30 }];
  expect(routedPath(points, 'POLYLINE')).toBe('M 0 0 L 10 0 L 20 30 L 40 30');
  const spline = routedPath(points, 'SPLINES');
  expect(spline).toContain(' C ');
  expect(spline).toMatch(/^M 0 0 .* L 40 30$/);
  expect(spline).not.toMatch(/NaN|undefined/);
  expect(routedPath([], 'SPLINES')).toBe('');
  expect(routedPath([{ x: 0, y: 0 }, { x: 40, y: 0 }], 'SPLINES')).toBe('M 0 0 L 40 0');
});

test('rounds bends while preserving transition endpoints', () => {
  expect(roundedPath([{ x: 0, y: 0 }, { x: 40, y: 0 }, { x: 40, y: 40 }]))
    .toBe('M 0 0 L 32 0 Q 40 0 40 8 L 40 40');
});

test('limits rounding to half the shortest adjacent segment', () => {
  expect(roundedPath([{ x: 0, y: 0 }, { x: 4, y: 0 }, { x: 4, y: 2 }, { x: 8, y: 2 }]))
    .toBe('M 0 0 L 3 0 Q 4 0 4 1 L 4 1 Q 4 2 5 2 L 8 2');
});

test('handles straight, duplicate and empty routes without invalid coordinates', () => {
  expect(roundedPath([])).toBe('');
  expect(roundedPath([{ x: 1, y: 2 }])).toBe('M 1 2');
  expect(roundedPath([{ x: 0, y: 0 }, { x: 0, y: 0 }, { x: 10, y: 0 }, { x: 20, y: 0 }]))
    .toBe('M 0 0 L 10 0 L 20 0');
});

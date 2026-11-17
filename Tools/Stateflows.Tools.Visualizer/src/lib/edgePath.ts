import type { Point, DiagramPresentation } from './model.js';

export function routedPath(points: readonly Point[], routing: DiagramPresentation['routing'] = 'ORTHOGONAL'): string {
  if (routing === 'ORTHOGONAL') return roundedPath(points);
  if (routing === 'SPLINES') return roundedPath(points, true);
  if (!points.length) return '';
  let path = `M ${points[0].x} ${points[0].y}`;
  for (const point of points.slice(1)) path += ` L ${point.x} ${point.y}`;
  return path;
}

export function roundedPath(points: readonly Point[], spline = false): string {
  const route = points.filter((point, index) => !index ||
    point.x !== points[index - 1].x || point.y !== points[index - 1].y);
  if (!route.length) return '';
  let path = `M ${route[0].x} ${route[0].y}`;
  for (let index = 1; index < route.length - 1; index++) {
    const previous = route[index - 1];
    const corner = route[index];
    const next = route[index + 1];
    const incoming = Math.hypot(corner.x - previous.x, corner.y - previous.y);
    const outgoing = Math.hypot(next.x - corner.x, next.y - corner.y);
    if ((corner.x - previous.x) * (next.y - corner.y) ===
        (corner.y - previous.y) * (next.x - corner.x)) {
      path += ` L ${corner.x} ${corner.y}`;
      continue;
    }
    const radius = Math.min(spline ? 32 : 8, incoming / 2, outgoing / 2);
    const start = {
      x: corner.x + (previous.x - corner.x) * radius / incoming,
      y: corner.y + (previous.y - corner.y) * radius / incoming
    };
    const end = {
      x: corner.x + (next.x - corner.x) * radius / outgoing,
      y: corner.y + (next.y - corner.y) * radius / outgoing
    };
    if (spline) {
      const first = { x: start.x + (corner.x - start.x) * 2 / 3, y: start.y + (corner.y - start.y) * 2 / 3 };
      const second = { x: end.x + (corner.x - end.x) * 2 / 3, y: end.y + (corner.y - end.y) * 2 / 3 };
      path += ` L ${start.x} ${start.y} C ${first.x} ${first.y} ${second.x} ${second.y} ${end.x} ${end.y}`;
    } else path += ` L ${start.x} ${start.y} Q ${corner.x} ${corner.y} ${end.x} ${end.y}`;
  }
  if (route.length > 1) path += ` L ${route.at(-1)!.x} ${route.at(-1)!.y}`;
  return path;
}

import { documentId } from '../src/lib/adapters.js';
import type { DiagramDocument, DiagramOverlay, ElementObservation, ElementStatus } from '../src/lib/model.js';

export interface Observation {
  sequence: number;
  invocationId: string;
  at: string;
  family: DiagramDocument['family'];
  name: string;
  version: number;
  instance: string;
  eventId: string | null;
  executionMode: string;
  elementId: string;
  operation: string;
  stage: 'started' | 'completed' | 'failed';
  status: ElementStatus;
  tokenCount: number | null;
  requiredTokens: number | null;
  exceptionType: string | null;
}

export function matches(record: Observation, document: DiagramDocument, instance: string): boolean {
  return documentId(record.family, record.name, record.version) === document.id && record.instance === instance;
}

/** Reconstructs an observed frame, not persisted state or a transaction outcome. */
export function frame(document: DiagramDocument, instance: string, records: readonly Observation[]): DiagramOverlay {
  const elements = new Map<string, ElementObservation>();
  const running = new Map<string, Set<string>>();
  for (const record of records) {
    if (!matches(record, document, instance)) continue;
    const invocations = running.get(record.elementId) ?? new Set<string>();
    if (record.stage === 'started') invocations.add(record.invocationId);
    else invocations.delete(record.invocationId);
    running.set(record.elementId, invocations);
    elements.set(record.elementId, {
      elementId: record.elementId, status: invocations.size ? 'running' : record.status,
      running: invocations.size, operation: record.operation,
      tokenCount: record.tokenCount ?? undefined, requiredTokens: record.requiredTokens ?? undefined
    });
  }
  return {
    documentId: document.id, instance, observedAt: records.at(-1)?.at ?? '',
    elements: [...elements.values()]
  };
}

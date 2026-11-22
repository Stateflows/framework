import { layoutDiagram } from './layout.js';
import type { DiagramDocument, DiagramPresentation } from './model.js';

self.onmessage = async (event: MessageEvent<{ id: number; document: DiagramDocument; presentation: DiagramPresentation }>) => {
  const { id, document, presentation } = event.data;
  try {
    self.postMessage({ id, layout: await layoutDiagram(document, presentation) });
  } catch (error) {
    self.postMessage({ id, error: error instanceof Error ? error.message : String(error) });
  }
};

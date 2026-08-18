import { Pipe, PipeTransform } from '@angular/core';

/** Extracts the short name from a fully qualified event type name.
 *  e.g. "Stateflows.Examples.MyEvent" → "MyEvent"
 */
@Pipe({ name: 'shortEventName', standalone: true, pure: true })
export class ShortEventNamePipe implements PipeTransform {
  transform(value: string): string {
    if (!value) return value;
    const dotIdx = value.lastIndexOf('.');
    return dotIdx >= 0 ? value.slice(dotIdx + 1) : value;
  }
}


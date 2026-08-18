import { Component, OnDestroy, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { forkJoin, Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { StateflowsApiService } from '../../core/services/stateflows-api.service';
import { BehaviorClass, BehaviorInstance } from '../../shared/models/behavior.models';

@Component({
  selector: 'sf-state-machines',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="page">
      <h1 class="page__title">State Machines</h1>

      <ng-container *ngIf="!loading(); else loadingTpl">
        <section class="section">
          <h2 class="section__title">Registered Classes ({{ classes().length }})</h2>
          <div *ngIf="classes().length === 0" class="empty">No state machine classes registered.</div>
          <table class="table" *ngIf="classes().length > 0">
            <thead>
              <tr><th>Name</th><th>Version</th></tr>
            </thead>
            <tbody>
              <tr *ngFor="let c of classes()">
                <td>{{ c.name ?? '-' }}</td>
                <td>{{ c.version ?? '-' }}</td>
              </tr>
            </tbody>
          </table>
        </section>

        <section class="section">
          <h2 class="section__title">Active Instances ({{ instances().length }})</h2>
          <div *ngIf="instances().length === 0" class="empty">No active instances.</div>
          <table class="table" *ngIf="instances().length > 0">
            <thead>
              <tr><th>Class</th><th>Instance</th></tr>
            </thead>
            <tbody>
              <tr *ngFor="let i of instances()">
                <td>{{ i.id?.behaviorClass?.name ?? '-' }}</td>
                <td>{{ i.id?.instance ?? '-' }}</td>
              </tr>
            </tbody>
          </table>
        </section>
      </ng-container>

      <ng-template #loadingTpl><div class="loading">Loading...</div></ng-template>
      <div class="error" *ngIf="error()">{{ error() }}</div>
    </div>
  `,
  styles: [`
    .page__title { font-size: 22px; font-weight: 700; margin: 0 0 24px; }
    .section { margin-bottom: 32px; }
    .section__title { font-size: 15px; font-weight: 600; margin: 0 0 12px; }
    .table { width: 100%; border-collapse: collapse; background: var(--sf-surface);
      border: 1px solid var(--sf-border); border-radius: 8px; overflow: hidden; }
    .table th, .table td { padding: 10px 14px; text-align: left; border-bottom: 1px solid var(--sf-border); }
    .table th { background: var(--sf-bg); font-weight: 600; font-size: 12px; color: var(--sf-text-muted); }
    .table tr:last-child td { border-bottom: none; }
    .empty { color: var(--sf-text-muted); padding: 12px 0; }
    .loading, .error { padding: 16px; color: var(--sf-text-muted); }
    .error { color: var(--sf-error); }
  `],
})
export class StateMachinesComponent implements OnInit, OnDestroy {
  classes = signal<BehaviorClass[]>([]);
  instances = signal<BehaviorInstance[]>([]);
  loading = signal(true);
  error = signal('');

  private destroy$ = new Subject<void>();

  constructor(private api: StateflowsApiService) {}

  ngOnInit(): void {
    forkJoin({
      classes: this.api.getStateMachineClasses(),
      instances: this.api.getStateMachineInstances(),
    })
    .pipe(takeUntil(this.destroy$))
    .subscribe({
      next: (data) => {
        this.classes.set(data.classes);
        this.instances.set(data.instances);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(`Failed to load: ${err.message}`);
        this.loading.set(false);
      },
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
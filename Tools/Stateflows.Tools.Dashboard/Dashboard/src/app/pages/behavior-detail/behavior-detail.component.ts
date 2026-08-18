import { Component, OnInit, OnDestroy, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { Subject } from 'rxjs';
import { switchMap, takeUntil } from 'rxjs/operators';
import { ListboxModule } from 'primeng/listbox';
import { TagModule } from 'primeng/tag';
import { ChipModule } from 'primeng/chip';
import { SkeletonModule } from 'primeng/skeleton';
import { DividerModule } from 'primeng/divider';
import { FormsModule } from '@angular/forms';
import { StateflowsApiService } from '../../core/services/stateflows-api.service';
import {
  BehaviorType, BehaviorInstance, BehaviorStatusInfo,
  BEHAVIOR_TYPE_CONFIGS, BEHAVIOR_STATUS_LABELS, BEHAVIOR_STATUS_SEVERITY,
} from '../../shared/models/behavior.models';
import { ShortEventNamePipe } from '../../shared/pipes/short-event-name.pipe';

interface InstanceOption {
  label: string;
  instance: string;
}

function flattenStates(node: any, depth = 0): string[] {
  if (!node?.value) return [];
  const indent = '  '.repeat(depth);
  const result = [`${indent}${node.value}`];
  if (Array.isArray(node.children)) {
    for (const child of node.children) result.push(...flattenStates(child, depth + 1));
  }
  return result;
}

@Component({
  selector: 'sf-behavior-detail',
  standalone: true,
  imports: [CommonModule, FormsModule, ListboxModule, TagModule, ChipModule, SkeletonModule, DividerModule, ShortEventNamePipe],
  template: `
    <div class="page">
      <!-- Header -->
      <div class="page-header">
        <button class="back-btn" (click)="goBack()">
          <i class="pi pi-arrow-left"></i>
        </button>
        <div class="header-icon" [style.background]="config().bgColor" [style.color]="config().color">
          <i class="pi {{ config().icon }}"></i>
        </div>
        <div>
          <h1 class="page-title">{{ behaviorName() }}</h1>
          <span class="page-subtitle">{{ config().label }}</span>
        </div>
      </div>

      <!-- Main split layout -->
      <div class="detail-layout">
        <!-- Left: Instance List -->
        <div class="instances-panel">
          <div class="panel-header">
            <i class="pi pi-list"></i>
            <span>Instances</span>
            <span class="instance-count" *ngIf="!loadingInstances()">{{ instanceOptions().length }}</span>
          </div>

          <ng-container *ngIf="loadingInstances()">
            <div class="skeleton-list">
              <p-skeleton *ngFor="let _ of [1,2,3,4]" height="38px" borderRadius="6px" styleClass="mb-2"></p-skeleton>
            </div>
          </ng-container>

          <ng-container *ngIf="!loadingInstances()">
            <div *ngIf="instanceOptions().length === 0" class="empty-instances">
              <i class="pi pi-inbox"></i>
              <span>No instances</span>
            </div>
            <p-listbox
              *ngIf="instanceOptions().length > 0"
              [options]="instanceOptions()"
              optionLabel="label"
              [ngModel]="selectedOption()"
              (ngModelChange)="selectInstance($event)"
              styleClass="instance-listbox"
              [style]="{ border: 'none', width: '100%' }">
              <ng-template #item let-opt>
                <div class="instance-item">
                  <i class="pi pi-circle-fill instance-dot" [style.color]="config().color"></i>
                  <span>{{ opt.label }}</span>
                </div>
              </ng-template>
            </p-listbox>
          </ng-container>
        </div>

        <!-- Center: Status Panel -->
        <div class="status-panel">
          <ng-container *ngIf="!selectedOption()">
            <div class="no-selection">
              <i class="pi pi-hand-pointer"></i>
              <p>Select an instance to view its status</p>
            </div>
          </ng-container>

          <ng-container *ngIf="selectedOption()">
            <div class="panel-header">
              <i class="pi pi-info-circle"></i>
              <span>{{ selectedOption()!.label }}</span>
            </div>

            <ng-container *ngIf="loadingStatus()">
              <div class="status-skeletons">
                <p-skeleton height="28px" width="120px" borderRadius="6px" styleClass="mb-4"></p-skeleton>
                <p-skeleton height="18px" width="60%" styleClass="mb-2"></p-skeleton>
                <p-skeleton height="18px" width="40%" styleClass="mb-2"></p-skeleton>
                <p-skeleton height="18px" width="50%" styleClass="mb-2"></p-skeleton>
              </div>
            </ng-container>

            <ng-container *ngIf="!loadingStatus() && status()">
              <!-- Status badge -->
              <div class="status-row">
                <span class="status-label">Status</span>
                <p-tag
                  [value]="statusLabel()"
                  [severity]="statusSeverity()"
                  [rounded]="true">
                </p-tag>
              </div>

              <p-divider></p-divider>

              <!-- Current States (State Machines) -->
              <ng-container *ngIf="currentStates().length > 0">
                <div class="info-section">
                  <div class="info-section-title">
                    <i class="pi pi-map-marker"></i> Current States
                  </div>
                  <div class="state-list">
                    <span class="state-chip" *ngFor="let s of currentStates()"
                          [style.background]="config().bgColor"
                          [style.color]="config().color"
                          [style.border-color]="config().borderColor">
                      {{ s }}
                    </span>
                  </div>
                </div>
              </ng-container>

              <!-- Expected Events -->
              <ng-container *ngIf="expectedEvents().length > 0">
                <div class="info-section">
                  <div class="info-section-title">
                    <i class="pi pi-send"></i> Expected Events
                  </div>
                  <div class="events-list">
                    <p-chip *ngFor="let ev of expectedEvents()" [label]="ev | shortEventName" styleClass="event-chip"></p-chip>
                  </div>
                </div>
              </ng-container>

              <!-- Metadata -->
              <ng-container *ngIf="metadataEntries().length > 0">
                <div class="info-section">
                  <div class="info-section-title">
                    <i class="pi pi-tags"></i> Metadata
                  </div>
                  <div class="metadata-table">
                    <div class="metadata-row" *ngFor="let entry of metadataEntries()">
                      <span class="meta-key">{{ entry.key }}</span>
                      <span class="meta-value">{{ entry.value }}</span>
                    </div>
                  </div>
                </div>
              </ng-container>
            </ng-container>

            <ng-container *ngIf="!loadingStatus() && !status()">
              <div class="no-status">
                <i class="pi pi-exclamation-circle"></i>
                <p>Status unavailable for this instance.</p>
              </div>
            </ng-container>
          </ng-container>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .page { display: flex; flex-direction: column; height: calc(100vh - 56px); }

    /* Header */
    .page-header {
      display: flex; align-items: center; gap: 14px; margin-bottom: 24px; flex-shrink: 0;
    }
    .back-btn {
      background: none; border: 1px solid var(--sf-border); border-radius: 8px;
      width: 36px; height: 36px; display: flex; align-items: center; justify-content: center;
      cursor: pointer; color: var(--sf-text-muted); transition: all 0.15s;
    }
    .back-btn:hover { background: var(--sf-surface); color: var(--sf-text); }
    .header-icon {
      width: 40px; height: 40px; border-radius: 10px;
      display: flex; align-items: center; justify-content: center; font-size: 19px;
    }
    .page-title { font-size: 20px; font-weight: 700; margin: 0; line-height: 1.2; }
    .page-subtitle { font-size: 12px; color: var(--sf-text-muted); }

    /* Split layout */
    .detail-layout {
      display: flex; gap: 0; flex: 1; overflow: hidden;
      background: var(--sf-surface); border: 1px solid var(--sf-border); border-radius: 12px;
    }

    /* Instances panel (left) */
    .instances-panel {
      width: 240px; flex-shrink: 0;
      border-right: 1px solid var(--sf-border);
      display: flex; flex-direction: column;
      overflow: hidden;
    }

    /* Status panel (center) */
    .status-panel {
      flex: 1; overflow-y: auto; padding: 20px 24px;
      display: flex; flex-direction: column;
    }

    .panel-header {
      display: flex; align-items: center; gap: 8px;
      padding: 14px 16px; font-size: 13px; font-weight: 600;
      border-bottom: 1px solid var(--sf-border); color: var(--sf-text-muted);
      flex-shrink: 0;
    }
    .panel-header i { font-size: 14px; }
    .instance-count {
      margin-left: auto;
      background: var(--sf-bg); border: 1px solid var(--sf-border);
      font-size: 11px; padding: 1px 7px; border-radius: 20px;
    }

    .skeleton-list { padding: 12px; }

    .empty-instances {
      display: flex; flex-direction: column; align-items: center;
      padding: 32px 16px; gap: 8px; color: var(--sf-text-muted); font-size: 13px;
    }
    .empty-instances i { font-size: 22px; opacity: 0.4; }

    :host ::ng-deep .instance-listbox {
      border-radius: 0 !important;
      box-shadow: none !important;
    }
    :host ::ng-deep .instance-listbox .p-listbox-list-wrapper {
      padding: 6px;
    }
    :host ::ng-deep .instance-listbox .p-listbox-item {
      border-radius: 6px !important;
      padding: 8px 10px !important;
    }
    .instance-item {
      display: flex; align-items: center; gap: 8px; font-size: 13px;
    }
    .instance-dot { font-size: 8px; }

    /* Status panel inner */
    .no-selection, .no-status {
      flex: 1; display: flex; flex-direction: column;
      align-items: center; justify-content: center;
      color: var(--sf-text-muted); gap: 10px;
    }
    .no-selection i, .no-status i { font-size: 32px; opacity: 0.3; }
    .no-selection p, .no-status p { margin: 0; font-size: 13px; }

    .status-skeletons { padding-top: 8px; }

    .status-row {
      display: flex; align-items: center; justify-content: space-between;
      margin-bottom: 8px;
    }
    .status-label { font-size: 13px; font-weight: 600; color: var(--sf-text-muted); }

    .info-section { margin-bottom: 20px; }
    .info-section-title {
      font-size: 12px; font-weight: 700; color: var(--sf-text-muted); text-transform: uppercase;
      letter-spacing: 0.04em; margin-bottom: 10px;
      display: flex; align-items: center; gap: 6px;
    }

    .state-list { display: flex; flex-wrap: wrap; gap: 6px; }
    .state-chip {
      padding: 4px 12px; border-radius: 20px; font-size: 13px; font-weight: 600;
      border: 1px solid;
    }

    .events-list { display: flex; flex-wrap: wrap; gap: 6px; }
    :host ::ng-deep .event-chip { font-size: 12px !important; }

    .metadata-table { display: flex; flex-direction: column; gap: 4px; }
    .metadata-row {
      display: flex; gap: 12px; font-size: 12px;
      padding: 5px 8px; background: var(--sf-bg); border-radius: 5px;
    }
    .meta-key { font-weight: 600; color: var(--sf-text-muted); min-width: 100px; }
    .meta-value { color: var(--sf-text); word-break: break-word; }
  `],
})
export class BehaviorDetailComponent implements OnInit, OnDestroy {
  behaviorType = signal<BehaviorType>('stateMachine');
  behaviorName = signal('');
  instanceOptions = signal<InstanceOption[]>([]);
  selectedOption = signal<InstanceOption | null>(null);
  status = signal<BehaviorStatusInfo | null>(null);
  loadingInstances = signal(true);
  loadingStatus = signal(false);

  config = computed(() => BEHAVIOR_TYPE_CONFIGS[this.behaviorType()]);
  statusLabel = computed(() =>
    BEHAVIOR_STATUS_LABELS[this.status()?.behaviorStatus ?? 0] ?? 'Unknown'
  );
  statusSeverity = computed(() =>
    BEHAVIOR_STATUS_SEVERITY[this.status()?.behaviorStatus ?? 0] ?? 'secondary'
  );
  currentStates = computed(() => flattenStates(this.status()?.currentStates));
  expectedEvents = computed(() => this.status()?.expectedEvents ?? []);
  metadataEntries = computed(() =>
    Object.entries(this.status()?.metadata ?? {}).map(([key, value]) => ({
      key, value: JSON.stringify(value),
    }))
  );

  private destroy$ = new Subject<void>();

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private api: StateflowsApiService,
  ) {}

  ngOnInit(): void {
    this.route.params.pipe(takeUntil(this.destroy$)).subscribe(params => {
      this.behaviorType.set(params['type'] as BehaviorType);
      this.behaviorName.set(params['name']);
      this.selectedOption.set(null);
      this.status.set(null);
      this.loadInstances();
    });
  }

  loadInstances(): void {
    const cfg = this.config();
    this.loadingInstances.set(true);
    this.api.getClassInstances(cfg.apiSegment, this.behaviorName())
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (instances) => {
          this.instanceOptions.set(
            instances.map(i => ({
              label: i.id?.instance || '(default)',
              instance: i.id?.instance ?? '',
            }))
          );
          this.loadingInstances.set(false);
        },
        error: () => this.loadingInstances.set(false),
      });
  }

  selectInstance(opt: InstanceOption | null): void {
    if (!opt) return;
    this.selectedOption.set(opt);
    this.loadStatus(opt.instance);
  }

  loadStatus(instance: string): void {
    const cfg = this.config();
    this.loadingStatus.set(true);
    this.status.set(null);
    this.api.getInstanceStatus(cfg.apiSegment, this.behaviorName(), instance)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => {
          this.status.set(res.response ?? null);
          this.loadingStatus.set(false);
        },
        error: () => this.loadingStatus.set(false),
      });
  }

  goBack(): void {
    this.router.navigate(['overview']);
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}




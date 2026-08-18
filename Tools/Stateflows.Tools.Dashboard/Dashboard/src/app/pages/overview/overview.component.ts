import { Component, OnInit, OnDestroy, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { forkJoin, Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { SkeletonModule } from 'primeng/skeleton';
import { StateflowsApiService } from '../../core/services/stateflows-api.service';
import {
  BehaviorClass, BehaviorInstance, BehaviorType,
  BehaviorTypeConfig, BEHAVIOR_TYPE_CONFIGS,
} from '../../shared/models/behavior.models';

interface ClassCard {
  cls: BehaviorClass;
  instanceCount: number;
}

interface BehaviorGroup {
  config: BehaviorTypeConfig;
  cards: ClassCard[];
}

function countByClass(instances: BehaviorInstance[]): Map<string, number> {
  const map = new Map<string, number>();
  for (const inst of instances) {
    const name = inst.id?.behaviorClass?.name ?? '';
    map.set(name, (map.get(name) ?? 0) + 1);
  }
  return map;
}

@Component({
  selector: 'sf-overview',
  standalone: true,
  imports: [CommonModule, SkeletonModule],
  template: `
    <div class="page">
      <h1 class="page-title">Overview</h1>

      <div *ngIf="error()" class="error-banner">
        <i class="pi pi-exclamation-triangle"></i> {{ error() }}
      </div>

      <!-- Skeleton loading state -->
      <ng-container *ngIf="loading()">
        <div class="group-skeleton" *ngFor="let _ of [1,2,3]">
          <p-skeleton width="140px" height="18px" styleClass="mb-3"></p-skeleton>
          <div class="cards-grid">
            <p-skeleton *ngFor="let __ of [1,2,3]" height="110px" borderRadius="12px"></p-skeleton>
          </div>
        </div>
      </ng-container>

      <!-- Groups -->
      <ng-container *ngIf="!loading()">
        <div *ngIf="groups().length === 0" class="empty-state">
          <i class="pi pi-inbox"></i>
          <p>No behavior classes registered.</p>
        </div>

        <section class="behavior-group" *ngFor="let group of groups()">
          <div class="group-header">
            <span class="group-icon" [style.background]="group.config.bgColor" [style.color]="group.config.color">
              <i class="pi {{ group.config.icon }}"></i>
            </span>
            <h2 class="group-title">{{ group.config.label }}s</h2>
            <span class="group-badge" [style.background]="group.config.bgColor" [style.color]="group.config.color">
              {{ group.cards.length }}
            </span>
          </div>

          <div class="cards-grid">
            <div
              class="behavior-card"
              *ngFor="let card of group.cards"
              (click)="navigate(group.config.type, card.cls.name!)"
              [style.border-top-color]="group.config.color"
              tabindex="0"
              (keydown.enter)="navigate(group.config.type, card.cls.name!)">

              <div class="card-type-icon" [style.background]="group.config.bgColor" [style.color]="group.config.color">
                <i class="pi {{ group.config.icon }}"></i>
              </div>

              <div class="card-name">{{ card.cls.name }}</div>
              <div class="card-version" *ngIf="card.cls.version">v{{ card.cls.version }}</div>

              <div class="card-instances" [style.color]="group.config.color">
                <i class="pi pi-circle-fill" style="font-size:8px; vertical-align: middle; margin-right:4px;"></i>
                {{ card.instanceCount }} {{ card.instanceCount === 1 ? 'instance' : 'instances' }}
              </div>

              <div class="card-arrow">
                <i class="pi pi-arrow-right"></i>
              </div>
            </div>
          </div>
        </section>
      </ng-container>
    </div>
  `,
  styles: [`
    .page-title { font-size: 22px; font-weight: 700; margin: 0 0 28px; color: var(--sf-text); }

    .error-banner {
      display: flex; align-items: center; gap: 8px;
      background: #fef2f2; border: 1px solid #fecaca; color: #dc2626;
      padding: 10px 14px; border-radius: 8px; margin-bottom: 20px; font-size: 13px;
    }

    .empty-state {
      text-align: center; padding: 60px 20px; color: var(--sf-text-muted);
    }
    .empty-state i { font-size: 40px; display: block; margin-bottom: 12px; opacity: 0.4; }

    .behavior-group { margin-bottom: 36px; }
    .group-skeleton { margin-bottom: 36px; }

    .group-header {
      display: flex; align-items: center; gap: 10px; margin-bottom: 14px;
    }
    .group-icon {
      width: 30px; height: 30px; border-radius: 8px;
      display: flex; align-items: center; justify-content: center; font-size: 15px;
    }
    .group-title { font-size: 15px; font-weight: 700; margin: 0; color: var(--sf-text); }
    .group-badge {
      font-size: 11px; font-weight: 700; padding: 2px 8px; border-radius: 20px;
    }

    .cards-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
      gap: 14px;
    }

    .behavior-card {
      background: var(--sf-surface);
      border: 1px solid var(--sf-border);
      border-top: 3px solid transparent;
      border-radius: 12px;
      padding: 18px 16px 14px;
      cursor: pointer;
      transition: box-shadow 0.15s, transform 0.12s;
      position: relative;
      display: flex; flex-direction: column; gap: 4px;
      outline: none;
    }
    .behavior-card:hover {
      box-shadow: 0 4px 20px rgba(0,0,0,0.10);
      transform: translateY(-2px);
    }
    .behavior-card:focus-visible { box-shadow: 0 0 0 3px rgba(0,137,191,0.35); }

    .card-type-icon {
      width: 36px; height: 36px; border-radius: 9px;
      display: flex; align-items: center; justify-content: center;
      font-size: 17px; margin-bottom: 6px;
    }

    .card-name { font-size: 15px; font-weight: 700; color: var(--sf-text); line-height: 1.2; }
    .card-version { font-size: 11px; color: var(--sf-text-muted); }
    .card-instances { font-size: 12px; font-weight: 600; margin-top: 8px; }

    .card-arrow {
      position: absolute; top: 14px; right: 14px;
      color: var(--sf-text-muted); font-size: 12px; opacity: 0.5;
      transition: opacity 0.15s, transform 0.15s;
    }
    .behavior-card:hover .card-arrow { opacity: 1; transform: translateX(2px); }
  `],
})
export class OverviewComponent implements OnInit, OnDestroy {
  loading = signal(true);
  error = signal('');
  groups = signal<BehaviorGroup[]>([]);

  private destroy$ = new Subject<void>();

  constructor(private api: StateflowsApiService, private router: Router) {}

  ngOnInit(): void {
    forkJoin({
      smClasses:      this.api.getStateMachineClasses(),
      actClasses:     this.api.getActivityClasses(),
      actionClasses:  this.api.getActionClasses(),
      entityClasses:  this.api.getEntityClasses(),
      smInstances:    this.api.getStateMachineInstances(),
      actInstances:   this.api.getActivityInstances(),
      actionInstances: this.api.getActionInstances(),
      entityInstances: this.api.getEntityInstances(),
    })
    .pipe(takeUntil(this.destroy$))
    .subscribe({
      next: (data) => {
        const smCount     = countByClass(data.smInstances);
        const actCount    = countByClass(data.actInstances);
        const actionCount = countByClass(data.actionInstances);
        const entityCount = countByClass(data.entityInstances);

        const buildCards = (classes: BehaviorClass[], counts: Map<string, number>): ClassCard[] =>
          classes.map(cls => ({ cls, instanceCount: counts.get(cls.name ?? '') ?? 0 }));

        const allGroups: BehaviorGroup[] = [
          { config: BEHAVIOR_TYPE_CONFIGS.stateMachine, cards: buildCards(data.smClasses, smCount) },
          { config: BEHAVIOR_TYPE_CONFIGS.activity,     cards: buildCards(data.actClasses, actCount) },
          { config: BEHAVIOR_TYPE_CONFIGS.action,       cards: buildCards(data.actionClasses, actionCount) },
          { config: BEHAVIOR_TYPE_CONFIGS.entity,       cards: buildCards(data.entityClasses, entityCount) },
        ];

        this.groups.set(allGroups.filter(g => g.cards.length > 0));
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(`Failed to load data: ${err.message}`);
        this.loading.set(false);
      },
    });
  }

  navigate(type: BehaviorType, name: string): void {
    this.router.navigate(['behaviors', type, name]);
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}

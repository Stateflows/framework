import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { BehaviorClass, BehaviorInstance, StatusApiResponse } from '../../shared/models/behavior.models';

@Injectable({ providedIn: 'root' })
export class StateflowsApiService {
  private readonly base = '/stateflows-dashboard';

  constructor(private http: HttpClient) {}

  getAllClasses(): Observable<BehaviorClass[]> {
    return this.http.get<BehaviorClass[]>(`${this.base}/classes`).pipe(catchError(() => of([])));
  }

  getStateMachineClasses(): Observable<BehaviorClass[]> {
    return this.http.get<BehaviorClass[]>(`${this.base}/classes/stateMachines`).pipe(catchError(() => of([])));
  }

  getActivityClasses(): Observable<BehaviorClass[]> {
    return this.http.get<BehaviorClass[]>(`${this.base}/classes/activities`).pipe(catchError(() => of([])));
  }

  getActionClasses(): Observable<BehaviorClass[]> {
    return this.http.get<BehaviorClass[]>(`${this.base}/classes/actions`).pipe(catchError(() => of([])));
  }

  getEntityClasses(): Observable<BehaviorClass[]> {
    return this.http.get<BehaviorClass[]>(`${this.base}/classes/entities`).pipe(catchError(() => of([])));
  }

  getAllInstances(): Observable<BehaviorInstance[]> {
    return this.http.get<BehaviorInstance[]>(`${this.base}/`).pipe(catchError(() => of([])));
  }

  getStateMachineInstances(): Observable<BehaviorInstance[]> {
    return this.http.get<BehaviorInstance[]>(`${this.base}/stateMachines`).pipe(catchError(() => of([])));
  }

  getActivityInstances(): Observable<BehaviorInstance[]> {
    return this.http.get<BehaviorInstance[]>(`${this.base}/activities`).pipe(catchError(() => of([])));
  }

  getActionInstances(): Observable<BehaviorInstance[]> {
    return this.http.get<BehaviorInstance[]>(`${this.base}/actions`).pipe(catchError(() => of([])));
  }

  getEntityInstances(): Observable<BehaviorInstance[]> {
    return this.http.get<BehaviorInstance[]>(`${this.base}/entities`).pipe(catchError(() => of([])));
  }

  /** Instances of a single named behavior class, e.g. stateMachines/Doc */
  getClassInstances(apiSegment: string, name: string): Observable<BehaviorInstance[]> {
    return this.http.get<BehaviorInstance[]>(`${this.base}/${apiSegment}/${name}`)
      .pipe(catchError(() => of([])));
  }

  /** Status of a single instance */
  getInstanceStatus(apiSegment: string, name: string, instance: string): Observable<StatusApiResponse> {
    const url = instance
      ? `${this.base}/${apiSegment}/${name}/${instance}/status`
      : `${this.base}/${apiSegment}/${name}/status`;
    return this.http.get<StatusApiResponse>(url).pipe(catchError(() => of({})));
  }
}

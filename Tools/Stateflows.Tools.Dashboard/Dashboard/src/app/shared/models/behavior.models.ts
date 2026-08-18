export interface BehaviorClass {
  type?: string;
  name?: string;
  version?: number;
}

export interface BehaviorInstance {
  id?: BehaviorId;
}

export interface BehaviorId {
  behaviorClass?: BehaviorClass;
  instance?: string;
}

export interface DashboardSummary {
  stateMachineClassCount: number;
  activityClassCount: number;
  actionClassCount: number;
  stateMachineInstanceCount: number;
  activityInstanceCount: number;
  actionInstanceCount: number;
}

export interface StatusApiResponse {
  status?: number;
  response?: BehaviorStatusInfo;
}

export interface BehaviorStatusInfo {
  id?: any;
  behaviorStatus?: number;
  behaviorStatusText?: string;
  expectedEvents?: string[];
  currentStates?: StateNode;
  metadata?: Record<string, any>;
}

export interface StateNode {
  value?: string;
  children?: StateNode[];
}

export type BehaviorType = 'stateMachine' | 'activity' | 'action' | 'entity';

export interface BehaviorTypeConfig {
  type: BehaviorType;
  label: string;
  apiSegment: string;
  color: string;
  bgColor: string;
  borderColor: string;
  icon: string;
}

export const BEHAVIOR_TYPE_CONFIGS: Record<BehaviorType, BehaviorTypeConfig> = {
  stateMachine: {
    type: 'stateMachine',
    label: 'State Machine',
    apiSegment: 'stateMachines',
    color: '#0089BF',
    bgColor: '#e8f6fb',
    borderColor: '#b3dff0',
    icon: 'pi-sitemap',
  },
  activity: {
    type: 'activity',
    label: 'Activity',
    apiSegment: 'activities',
    color: '#16a34a',
    bgColor: '#f0fdf4',
    borderColor: '#bbf7d0',
    icon: 'pi-cog',
  },
  action: {
    type: 'action',
    label: 'Action',
    apiSegment: 'actions',
    color: '#d97706',
    bgColor: '#fffbeb',
    borderColor: '#fde68a',
    icon: 'pi-bolt',
  },
  entity: {
    type: 'entity',
    label: 'Entity',
    apiSegment: 'entities',
    color: '#7c3aed',
    bgColor: '#f5f3ff',
    borderColor: '#ddd6fe',
    icon: 'pi-database',
  },
};

export const BEHAVIOR_STATUS_LABELS: Record<number, string> = {
  0: 'Unknown',
  1: 'Not Initialized',
  2: 'Initialized',
  3: 'Finalized',
};

export const BEHAVIOR_STATUS_SEVERITY: Record<number, 'success' | 'warn' | 'info' | 'secondary'> = {
  0: 'secondary',
  1: 'warn',
  2: 'success',
  3: 'info',
};

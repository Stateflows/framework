import { SendResult } from "../classes/send-result";
import { Event } from "../events/event";
import { BehaviorClass } from "../ids/behavior.class";
import { BehaviorId } from "../ids/behavior.id";
import { IWatcher } from "./watcher";

export interface IStateflowsClientTransport {
    getAvailableClasses(): Promise<BehaviorClass[]>;
    send(behaviorId: BehaviorId, event: Event): Promise<SendResult>;
    watch(watcher: IWatcher, notificationName: string, interval: number): Promise<void>;
    unwatch(watcher: IWatcher, notificationName: string): Promise<void>;
}
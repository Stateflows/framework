import { EventHolder } from "../classes/event-holder";
import { SendResult } from "../classes/send-result";
import { BehaviorClass } from "../ids/behavior.class";
import { BehaviorId } from "../ids/behavior.id";
import { IWatcher } from "./watcher";

export interface IStateflowsClientTransport {
    getAvailableClasses(): Promise<BehaviorClass[]>;
    send(behaviorId: BehaviorId, eventHolder: EventHolder): Promise<SendResult>;
    watch(watcher: IWatcher, notificationName: string): Promise<void>;
    unwatch(watcher: IWatcher, notificationName: string): Promise<void>;
}
import { Notification } from "../events/notification";
import { BehaviorId } from "../ids/behavior.id";

export interface IWatcher {
    id: BehaviorId;
    notify(notification: Notification): void;
}
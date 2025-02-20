import { EventHolder } from "../classes/event-holder";
import { BehaviorId } from "../ids/behavior.id";

export interface IWatcher {
    id: BehaviorId;
    notify(notification: EventHolder): void;
    
    // unwatch(): Promise<void>;
}
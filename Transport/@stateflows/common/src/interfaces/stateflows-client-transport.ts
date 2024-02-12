import { SendResult } from "../classes/send-result";
import { Event } from "../events/event";
import { BehaviorClass } from "../ids/behavior.class";
import { BehaviorId } from "../ids/behavior.id";

export interface IStateflowsClientTransport {
    getAvailableClasses(): Promise<BehaviorClass[]>;
    send(behaviorId: BehaviorId, event: Event): Promise<SendResult>;
}
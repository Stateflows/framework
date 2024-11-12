import { BehaviorId } from "../ids/behavior.id";
import { EventHeader } from "./event-header";

export class EventHolder {
    public $type = "Stateflows.Common.EventHolder<>, Stateflows.Common";

    public id: string;
    public name: string;
    public sentAt: string;
    public senderId: BehaviorId;

    constructor(
        public payload: any,
        public headers: EventHeader[] = [],
    ) {
        this.$type = this.$type.replace('<>', '`1[[' + payload.$type + ']]');
    }
}
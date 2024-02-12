import { BehaviorClass } from "./behavior.class";
import { BehaviorId } from "./behavior.id";

export class ActivityId extends BehaviorId {
    constructor(
        name: string,
        instance: string
    ) {
        super(new BehaviorClass("Activity", name), instance);
    }
}
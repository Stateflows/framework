import { IActivityBehavior } from "../interfaces/activity.behavior";
import { Behavior } from "./behavior";

export class Activity extends Behavior implements IActivityBehavior {
    constructor(behavior: Behavior) {
        super(behavior, behavior.id);
    }
}
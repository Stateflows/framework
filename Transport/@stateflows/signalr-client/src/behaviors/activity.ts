import { IActivity } from "../interfaces/activity";
import { Behavior } from "./behavior";

export class Activity extends Behavior implements IActivity {
    constructor(behavior: Behavior) {
        super(behavior, behavior.behaviorId);
    }
}
import { StateflowsEvent } from "../decorators/stateflows-event";
import { BehaviorStatus } from "../enums/behavior-status";
import { Event } from "./event";

@StateflowsEvent("Stateflows.Common.Events.BehaviorInfo, Stateflows.Common")
export class BehaviorInfo extends Event {
    public behaviorStatus: BehaviorStatus;
    public expectedEvents: Array<string>;
}
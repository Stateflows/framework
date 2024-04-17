import { BehaviorStatus } from "../enums/behavior-status";
import { BehaviorStatusResponse } from "./behavior-status.response";

export class CurrentStateResponse extends BehaviorStatusResponse {
    constructor(
        behaviorStatus: BehaviorStatus,
        expectedEvents: Array<string>,
        public statesStack: Array<string>,
    ) {
        super(behaviorStatus, expectedEvents);
    }
}
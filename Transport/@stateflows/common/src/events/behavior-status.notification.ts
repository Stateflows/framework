import { Notification } from "./notification";
import { BehaviorStatus } from "../enums/behavior-status";

export class BehaviorStatusNotification extends Notification {
    constructor(
        public behaviorStatus: BehaviorStatus,
    ) {
        super();
    }

    public static notificationName = "Stateflows.Common.BehaviorStatusNotification";
}
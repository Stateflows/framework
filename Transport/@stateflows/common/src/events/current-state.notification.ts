import { Notification } from "./notification";

export class CurrentStateNotification extends Notification {
    constructor(
        public statesStack: Array<string>,
        public expectedEvents: Array<string>,
    ) {
        super();
    }

    public static notificationName = "Stateflows.StateMachines.Events.CurrentStateNotification";
}
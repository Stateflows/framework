import { CurrentStateNotification } from "../events/current-state.notification";
import { CurrentStateRequest } from "../events/current-state.request";
import { IStateMachine } from "../interfaces/state-machine";
import { NotificationHandler } from "../utils/notification-handler";
import { Behavior } from "./behavior";

export class StateMachine extends Behavior implements IStateMachine {
    constructor(behavior: Behavior) {
        super(behavior, behavior.id);
    }

    getCurrentState(): Promise<any> {
        return this.request(new CurrentStateRequest());
    }

    watchCurrentState(handler: NotificationHandler<CurrentStateNotification>): Promise<void> {
        return this.watch<CurrentStateNotification>(CurrentStateNotification.notificationName, handler);
    }

    unwatchCurrentState(): Promise<void> {
        return this.unwatch(CurrentStateNotification.notificationName);
    }
}